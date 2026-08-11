import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule,
    FormsModule, 
    RouterModule
  ],
  templateUrl: './login.component.html',
  styles: []
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Clear any stale/expired tokens from local storage when arriving at login page
    this.authService.clearSession();

    this.route.queryParams.subscribe(params => {
      if (params['registered']) {
        this.successMessage = 'Đăng ký tài khoản thành công! Vui lòng kiểm tra email để kích hoạt tài khoản.';
      } else if (params['resetSuccess']) {
        this.successMessage = 'Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.';
      }
    });

    const rememberedEmail = localStorage.getItem('studyhub_remembered_email') || '';
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    this.loginForm = this.fb.group({
      email: [rememberedEmail, [Validators.required, Validators.pattern(emailPattern)]],
      password: ['', [Validators.required]],
      rememberMe: [!!rememberedEmail]
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  isTypoDomain(email: string): boolean {
    if (!email) return false;
    const parts = email.split('@');
    if (parts.length !== 2) return true;
    const domain = parts[1].toLowerCase().trim();
    const invalidDomains = ['gmsha.com', 'glioail.com', 'gmai.com', 'gamil.com', 'yaho.com', 'hotmial.com', 'outlok.com', 'test.com'];
    if (invalidDomains.includes(domain)) return true;
    if (!domain.includes('.') || domain.startsWith('.') || domain.endsWith('.')) return true;
    const tld = domain.split('.').pop() || '';
    if (tld.length < 2) return true;
    return false;
  }

  onSubmit(): void {
    const emailVal = this.loginForm.value.email ? this.loginForm.value.email.trim() : '';

    if (this.loginForm.invalid || this.isTypoDomain(emailVal)) {
      this.loginForm.markAllAsTouched();
      const emailControl = this.loginForm.get('email');
      if (emailControl?.hasError('pattern') || this.isTypoDomain(emailVal)) {
        this.errorMessage = 'Tên miền Email không hợp lệ hoặc có lỗi chính tả (ví dụ: @gmail.com hoặc @*.edu.vn).';
      }
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const loginData = {
      email: emailVal,
      matKhau: this.loginForm.value.password
    };

    if (this.loginForm.value.rememberMe) {
      localStorage.setItem('studyhub_remembered_email', emailVal);
    } else {
      localStorage.removeItem('studyhub_remembered_email');
    }

    this.authService.login(loginData).subscribe({
      next: () => {
        this.authService.getProfile().subscribe({
          next: () => {
            this.loading = false;
            if (this.authService.isAdmin()) {
              this.router.navigate(['/admin/dashboard']);
            } else {
              this.router.navigate(['/dashboard']);
            }
          },
          error: () => {
            this.loading = false;
            if (this.authService.isAdmin()) {
              this.router.navigate(['/admin/dashboard']);
            } else {
              this.router.navigate(['/dashboard']);
            }
          }
        });
      },
      error: (err) => {
        this.loading = false;
        if (err.error && err.error.errors) {
          const firstKey = Object.keys(err.error.errors)[0];
          const msgs = err.error.errors[firstKey];
          this.errorMessage = Array.isArray(msgs) ? msgs[0] : msgs;
        } else if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else if (err.error && err.error.title && !err.error.title.toLowerCase().includes('error occurred') && !err.error.title.includes('xảy ra lỗi')) {
          this.errorMessage = err.error.title;
        } else if (err.error && typeof err.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = 'Tài khoản không tồn tại hoặc mật khẩu không chính xác.';
        }
      }
    });
  }
}
