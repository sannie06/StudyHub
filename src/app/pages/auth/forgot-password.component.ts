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
    <div class="min-h-screen w-full bg-gradient-to-br from-[#F4F5FF] via-[#EEF0FF] to-[#EAEFFF] relative overflow-hidden flex flex-col justify-between selection:bg-[#5B4DFF] selection:text-white font-sans">

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

      <!-- Ambient Glow Orbs -->
      <div class="absolute -top-32 -left-32 w-96 h-96 bg-[#5B4DFF]/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-32 -right-32 w-96 h-96 bg-indigo-400/20 rounded-full blur-3xl pointer-events-none"></div>

      <!-- Main Center Content -->
      <main class="relative z-10 min-h-screen flex items-center justify-center p-6 lg:p-12 w-full">
        <!-- Floating Soft Card with Zero Hard Border -->
        <div class="bg-white p-8 sm:p-12 rounded-[40px] shadow-[0_25px_70px_rgba(91,77,255,0.09),0_10px_30px_rgba(0,0,0,0.03)] text-center max-w-[500px] w-full relative my-auto">

          <!-- 1. Top Graphic: Envelope + ? Badge + 3x3 Dot Grids -->
          <div class="w-full flex items-center justify-center mb-6">
            <div class="relative flex items-center justify-center">
              <!-- Left 3x3 Dot Grid -->
              <div class="grid grid-cols-3 gap-1.5 mr-6 opacity-60">
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
              </div>

              <!-- Center Glowing Circle with Envelope Graphic -->
              <div class="w-24 h-24 rounded-full bg-[#EEF0FF] flex items-center justify-center relative shadow-inner">
                <svg class="w-16 h-16 overflow-visible" viewBox="0 0 100 100" fill="none">
                  <!-- Letter Sheet inside Envelope -->
                  <rect x="24" y="16" width="52" height="38" rx="6" fill="#FFFFFF" stroke="#E2E8F0" stroke-width="1.5"/>
                  <line x1="32" y1="25" x2="56" y2="25" stroke="#CBD5E1" stroke-width="2" stroke-linecap="round"/>
                  <line x1="32" y1="32" x2="68" y2="32" stroke="#CBD5E1" stroke-width="2" stroke-linecap="round"/>
                  <line x1="32" y1="39" x2="60" y2="39" stroke="#CBD5E1" stroke-width="2" stroke-linecap="round"/>

                  <!-- Purple Envelope Main Body -->
                  <path d="M12 36 L88 36 L88 74 C88 80 82 85 76 85 L24 85 C18 85 12 80 12 74 Z" fill="#5B4DFF"/>
                  <!-- Top Flap Fold Shadow -->
                  <path d="M12 36 L50 62 L88 36 Z" fill="#4335E6"/>
                  <!-- Front Diagonal Seams -->
                  <path d="M12 85 L44 58" stroke="#4D3EF7" stroke-width="1.5"/>
                  <path d="M88 85 L56 58" stroke="#4D3EF7" stroke-width="1.5"/>

                  <!-- Floating Question Mark Circle Badge -->
                  <circle cx="78" cy="70" r="14" fill="#FFFFFF" stroke="#5B4DFF" stroke-width="2"/>
                  <text x="78" y="76" font-size="16" font-weight="900" fill="#5B4DFF" text-anchor="middle" font-family="-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif">?</text>
                </svg>
              </div>

              <!-- Right 3x3 Dot Grid -->
              <div class="grid grid-cols-3 gap-1.5 ml-6 opacity-60">
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
              </div>
            </div>
          </div>

          <!-- 2. Stepper Progress (Step 1 & Step 2) -->
          <div class="flex items-center justify-center mb-7 max-w-[280px] mx-auto">
            <!-- Step 1: Active -->
            <div class="flex flex-col items-center gap-1.5 shrink-0">
              <div class="w-8 h-8 rounded-full bg-[#5B4DFF] text-white font-bold text-xs flex items-center justify-center shadow-md shadow-indigo-200">
                1
              </div>
              <span class="text-xs font-bold text-[#5B4DFF] whitespace-nowrap">Xác minh email</span>
            </div>

            <!-- Connector Line -->
            <div class="flex-1 h-[1.5px] bg-gray-200 mx-4 -mt-5"></div>

            <!-- Step 2: Pending -->
            <div class="flex flex-col items-center gap-1.5 shrink-0">
              <div class="w-8 h-8 rounded-full bg-[#F3F4F6] text-gray-400 font-bold text-xs flex items-center justify-center">
                2
              </div>
              <span class="text-xs font-medium text-gray-400 whitespace-nowrap">Đặt lại mật khẩu</span>
            </div>
          </div>

          <!-- 3. Title & Description (Centered) -->
          <div class="mb-6">
            <h2 class="text-3xl sm:text-[34px] font-black text-gray-900 tracking-tight leading-tight">
              Quên <span class="text-[#5B4DFF]">mật khẩu?</span>
            </h2>
            <p class="text-sm text-gray-500 mt-2.5 leading-relaxed max-w-sm mx-auto">
              Nhập email tài khoản của bạn.<br/>
              Chúng tôi sẽ gửi mã xác thực để đặt lại mật khẩu.
            </p>
          </div>

          <!-- Error Alert Banner -->
          <div *ngIf="errorMessage" class="bg-rose-50 text-rose-600 p-3.5 rounded-2xl text-xs flex items-center justify-center gap-2 mb-4">
            <i class="pi pi-exclamation-triangle text-sm text-rose-500"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <!-- 4. Form -->
          <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <div>
              <label class="text-xs font-bold text-gray-800 block text-left mb-2">
                Email
              </label>
              <div class="relative flex items-center">
                <span class="absolute left-4 top-1/2 -translate-y-1/2 text-[#5B4DFF] text-base pointer-events-none flex items-center justify-center w-5 h-5">
                  <i class="pi pi-envelope"></i>
                </span>
                <input
                  type="email"
                  formControlName="email"
                  placeholder="Nhập email của bạn"
                  class="bg-white border border-gray-200/90 rounded-2xl pl-11 pr-4 py-3.5 text-sm text-gray-800 shadow-[inset_0_2px_4px_rgba(0,0,0,0.02)] focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none w-full transition-all placeholder:text-gray-400 font-medium"
                />
              </div>
              <div *ngIf="forgotForm.get('email')?.touched && forgotForm.get('email')?.invalid" class="text-xs text-rose-500 mt-1.5 text-left">
                Vui lòng nhập email hợp lệ.
              </div>
            </div>

            <!-- 5. Primary Capsule Button (#5B4DFF with Subtle Glow) -->
            <button
              type="submit"
              [disabled]="loading"
              class="w-full py-4 bg-[#5B4DFF] hover:bg-[#4d3ef7] text-white font-bold rounded-2xl shadow-lg shadow-indigo-200/80 hover:shadow-indigo-300 transition-all text-sm flex items-center justify-center gap-2.5 cursor-pointer disabled:opacity-50 mt-6 active:scale-[0.99]"
            >
              <i *ngIf="loading" class="pi pi-spin pi-spinner text-sm"></i>
              <i *ngIf="!loading" class="pi pi-send text-xs"></i>
              <span>{{ loading ? 'Đang gửi mã...' : 'Gửi mã xác thực' }}</span>
            </button>

            <!-- 6. Bottom Centered Link -->
            <div class="pt-2 text-center">
              <a routerLink="/login" class="text-xs text-[#5B4DFF] hover:text-[#4335E6] font-semibold transition-colors inline-flex items-center justify-center gap-1.5 py-1">
                <i class="pi pi-arrow-left text-[10px]"></i>
                <span>Quay lại đăng nhập</span>
              </a>
            </div>
          </form>

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
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.pattern(emailPattern)]]
    });
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
    const emailVal = this.forgotForm.value.email ? this.forgotForm.value.email.trim() : '';

    if (this.forgotForm.invalid || this.isTypoDomain(emailVal)) {
      this.forgotForm.markAllAsTouched();
      const emailControl = this.forgotForm.get('email');
      if (emailControl?.hasError('pattern') || this.isTypoDomain(emailVal)) {
        this.errorMessage = 'Tên miền Email không hợp lệ hoặc có lỗi chính tả (ví dụ: @gmail.com hoặc @*.edu.vn).';
      }
      return;
    }

    this.loading = true;
    this.errorMessage = '';

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
