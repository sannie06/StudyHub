import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
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

    this.registerForm = this.fb.group({
      hoTen: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
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

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const registerData = {
      hoTen: this.registerForm.value.hoTen,
      email: this.registerForm.value.email,
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
        if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else if (err.error && err.error.title) {
          this.errorMessage = err.error.title;
        } else if (err.error && err.error.errors) {
          const firstKey = Object.keys(err.error.errors)[0];
          this.errorMessage = err.error.errors[firstKey][0];
        } else if (err.error && typeof err.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = 'Đăng ký không thành công. Email có thể đã tồn tại hoặc thông tin không hợp lệ.';
        }
      }
    });
  }
}
