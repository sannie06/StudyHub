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
  currentStep = 1;
  loading = false;
  errorMessage = '';

  // Dropdown option mocks for Step 2
  universities = [
    { label: 'Đại học Bách Khoa', value: 'DHBK' },
    { label: 'Đại học FPT', value: 'FPTU' },
    { label: 'Đại học Công nghệ - ĐHQGHN', value: 'UET' },
    { label: 'Đại học Khoa học Tự nhiên', value: 'HUS' }
  ];

  faculties = [
    { label: 'Công nghệ thông tin', value: 'CNTT' },
    { label: 'Điện tử viễn thông', value: 'DTVT' },
    { label: 'Kinh tế & Quản trị kinh doanh', value: 'KT' }
  ];

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
      // Step 1: Personal Info
      hoTen: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      matKhau: ['', [Validators.required, Validators.minLength(6)]],
      confirmMatKhau: ['', [Validators.required]],
      
      // Step 2: Learning Info
      maSinhVien: ['', [Validators.required]],
      truongHoc: [null, [Validators.required]],
      khoa: [null, [Validators.required]],
      nganhHoc: ['', [Validators.required]],
      nienKhoa: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('matKhau')?.value === g.get('confirmMatKhau')?.value
      ? null : { mismatch: true };
  }

  nextStep(): void {
    // Validate fields for Step 1
    const step1Fields = ['hoTen', 'email', 'matKhau', 'confirmMatKhau'];
    let step1Valid = true;

    step1Fields.forEach(field => {
      const control = this.registerForm.get(field);
      control?.markAsTouched();
      if (control?.invalid) {
        step1Valid = false;
      }
    });

    if (this.registerForm.hasError('mismatch')) {
      step1Valid = false;
    }

    if (step1Valid) {
      this.currentStep = 2;
    }
  }

  prevStep(): void {
    this.currentStep = 1;
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
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        if (err.error && err.error.title) {
          this.errorMessage = err.error.title;
        } else if (err.error && err.error.errors) {
          const firstKey = Object.keys(err.error.errors)[0];
          this.errorMessage = err.error.errors[firstKey][0];
        } else {
          this.errorMessage = 'Đăng ký không thành công. Vui lòng thử lại.';
        }
      }
    });
  }
}
