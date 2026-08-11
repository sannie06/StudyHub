import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule,
    FormsModule, 
    RouterModule,
    InputTextModule, 
    PasswordModule, 
    ButtonModule, 
    MessageModule,
    SelectModule
  ],
  templateUrl: './register.component.html',
  styles: []
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  loading = false;
  errorMessage = '';
  showPassword = false;
  showConfirmPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.currentUserValue) {
      this.router.navigate(['/dashboard']);
    }

    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    this.registerForm = this.fb.group({
      hoTen: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.pattern(emailPattern)]],
      matKhau: ['', [Validators.required, Validators.minLength(6)]],
      confirmMatKhau: ['', [Validators.required]],
      agreeTerms: [false]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('matKhau')?.value === g.get('confirmMatKhau')?.value
      ? null : { mismatch: true };
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  showTermsModal = false;
  activeTermsTab: 'terms' | 'privacy' = 'terms';

  openTermsModal(tab: 'terms' | 'privacy' = 'terms'): void {
    this.activeTermsTab = tab;
    this.showTermsModal = true;
  }

  setTermsTab(tab: 'terms' | 'privacy'): void {
    this.activeTermsTab = tab;
  }

  closeTermsModal(): void {
    this.showTermsModal = false;
  }

  confirmTerms(): void {
    this.registerForm.patchValue({ agreeTerms: true });
    this.showTermsModal = false;
    if (this.errorMessage === 'Vui lòng đồng ý với Điều khoản dịch vụ và Chính sách bảo mật.') {
      this.errorMessage = '';
    }
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
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
    const emailVal = this.registerForm.value.email ? this.registerForm.value.email.trim() : '';

    if (this.registerForm.invalid || this.isTypoDomain(emailVal)) {
      this.registerForm.markAllAsTouched();
      const emailControl = this.registerForm.get('email');
      if (emailControl?.hasError('pattern') || this.isTypoDomain(emailVal)) {
        this.errorMessage = 'Tên miền Email không hợp lệ hoặc có lỗi chính tả (ví dụ: @gmail.com hoặc @*.edu.vn).';
      }
      return;
    }

    if (!this.registerForm.value.agreeTerms) {
      this.errorMessage = 'Vui lòng đồng ý với Điều khoản dịch vụ và Chính sách bảo mật.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const registerData = {
      hoTen: this.registerForm.value.hoTen.trim(),
      email: emailVal,
      matKhau: this.registerForm.value.matKhau
    };

    this.authService.register(registerData).subscribe({
      next: () => {
        this.loading = false;
        this.authService.clearSession();
        this.router.navigate(['/verify-otp'], { queryParams: { email: registerData.email } });
      },
      error: (err) => {
        this.loading = false;
        console.error('Registration error details:', err);

        if (err.error && err.error.errors) {
          const firstKey = Object.keys(err.error.errors)[0];
          const msgs = err.error.errors[firstKey];
          this.errorMessage = Array.isArray(msgs) ? msgs[0] : msgs;
        } else if (err.error && err.error.title && !err.error.title.toLowerCase().includes('error occurred') && !err.error.title.includes('xảy ra lỗi')) {
          this.errorMessage = err.error.title;
        } else if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else if (err.error && err.error.detail && !err.error.detail.toLowerCase().includes('at microsoft') && !err.error.detail.toLowerCase().includes('at studyhub')) {
          this.errorMessage = err.error.detail;
        } else if (err.error && typeof err.error === 'string') {
          this.errorMessage = err.error;
        } else if (err.status === 400) {
          this.errorMessage = 'Email này đã tồn tại trong hệ thống. Vui lòng đăng nhập hoặc dùng email khác!';
        } else {
          this.errorMessage = 'Đăng ký không thành công. Vui lòng kiểm tra lại kết nối hoặc thông tin!';
        }
      }
    });
  }
}
