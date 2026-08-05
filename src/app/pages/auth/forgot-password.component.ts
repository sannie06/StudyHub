import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen w-full bg-gradient-to-br from-[#F4F5FF] via-[#EEF0FF] to-[#EAEFFF] relative overflow-hidden flex flex-col justify-between selection:bg-[#5B4DFF] selection:text-white">

      <!-- Top Navigation Header -->
      <header class="absolute top-0 left-0 right-0 w-full p-6 lg:px-12 flex justify-between items-center z-30 pointer-events-auto">
        <div class="flex items-center gap-3 cursor-pointer" routerLink="/">
          <div class="bg-[#5B4DFF] p-2.5 rounded-2xl text-white shadow-lg shadow-indigo-200 flex items-center justify-center">
            <i class="pi pi-graduation-cap text-xl"></i>
          </div>
          <span class="font-extrabold text-2xl tracking-tight text-gray-900">
            Study<span class="text-[#5B4DFF]">Hub</span>
          </span>
        </div>
        <div>
          <a routerLink="/login" class="px-5 py-2 bg-white border border-[#5B4DFF] text-[#5B4DFF] hover:bg-[#5B4DFF] hover:text-white font-semibold text-xs rounded-full transition-all cursor-pointer shadow-xs">
            Đăng nhập
          </a>
        </div>
      </header>

      <!-- Background Glow Orbs -->
      <div class="absolute -top-32 -left-32 w-96 h-96 bg-[#5B4DFF]/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-32 -right-32 w-96 h-96 bg-indigo-400/20 rounded-full blur-3xl pointer-events-none"></div>

      <!-- Main Center Content -->
      <main class="relative z-10 min-h-screen flex items-center justify-center p-6 lg:p-12 max-w-md mx-auto w-full">
        <div class="bg-white p-8 lg:p-10 rounded-[32px] shadow-2xl shadow-indigo-100/60 border border-gray-50 space-y-5 w-full my-auto">

          <!-- Header Icon & Title -->
          <div class="text-left space-y-1">
            <div class="w-14 h-14 bg-purple-100 text-[#5B4DFF] rounded-2xl flex items-center justify-center mb-4 shadow-sm">
              <i class="pi pi-key text-2xl"></i>
            </div>
            <h2 class="text-3xl font-extrabold text-gray-900 tracking-tight">
              Quên <span class="text-[#5B4DFF]">mật khẩu?</span>
            </h2>
            <p class="text-sm text-gray-500 leading-relaxed pt-1">
              Nhập email tài khoản của bạn. Chúng tôi sẽ gửi mã OTP 6 số để bạn đặt lại mật khẩu mới.
            </p>
          </div>

          <!-- Error Alert Banner -->
          <div *ngIf="errorMessage" class="bg-rose-50 border border-rose-200 text-rose-600 p-3.5 rounded-2xl text-xs flex items-center gap-2.5">
            <i class="pi pi-exclamation-triangle text-base text-rose-500"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Form -->
          <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()" class="space-y-4">
            <div>
              <label class="text-xs font-semibold text-gray-700 mb-1.5 block">
                Địa chỉ Email
              </label>
              <div class="relative">
                <input
                  type="email"
                  formControlName="email"
                  placeholder="Nhập email của bạn"
                  class="bg-white border border-gray-200 rounded-2xl pl-4 pr-10 py-3.5 text-sm text-gray-800 focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none w-full transition-all placeholder:text-gray-400"
                />
                <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 text-sm pointer-events-none">
                  <i class="pi pi-envelope"></i>
                </span>
              </div>
              <div *ngIf="forgotForm.get('email')?.touched && forgotForm.get('email')?.invalid" class="text-xs text-rose-500 mt-1">
                Vui lòng nhập email hợp lệ.
              </div>
            </div>

            <!-- Primary Button -->
            <button
              type="submit"
              [disabled]="loading"
              class="w-full py-3.5 bg-gradient-to-r from-[#5B4DFF] to-[#6366F1] hover:opacity-95 text-white font-bold rounded-2xl shadow-lg shadow-indigo-200 hover:shadow-indigo-300 transition-all text-sm flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 mt-2"
            >
              <i *ngIf="loading" class="pi pi-spin pi-spinner text-sm"></i>
              <span>{{ loading ? 'Đang gửi mã...' : 'Gửi mã OTP' }}</span>
              <i *ngIf="!loading" class="pi pi-arrow-right text-xs"></i>
            </button>
          </form>

          <!-- Back to Login Link -->
          <div class="pt-2 text-center">
            <a routerLink="/login" class="text-xs text-gray-500 hover:text-[#5B4DFF] font-semibold transition-colors flex items-center justify-center gap-1.5">
              <i class="pi pi-arrow-left text-[10px]"></i>
              <span>Quay lại trang Đăng nhập</span>
            </a>
          </div>

        </div>
      </main>

    </div>
  `
})
export class ForgotPasswordComponent {
  forgotForm: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    const emailVal = this.forgotForm.value.email;

    this.authService.forgotPassword(emailVal).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/reset-password'], { queryParams: { email: emailVal } });
      },
      error: (err) => {
        this.loading = false;
        if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else {
          this.errorMessage = 'Có lỗi xảy ra. Vui lòng kiểm tra lại địa chỉ email.';
        }
      }
    });
  }
}
