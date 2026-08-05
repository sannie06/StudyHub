import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-reset-password',
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

          <!-- Header Title -->
          <div class="text-left space-y-1">
            <div class="w-14 h-14 bg-purple-100 text-[#5B4DFF] rounded-2xl flex items-center justify-center mb-4 shadow-sm">
              <i class="pi pi-lock-open text-2xl"></i>
            </div>
            <h2 class="text-3xl font-extrabold text-gray-900 tracking-tight">
              Đặt lại <span class="text-[#5B4DFF]">mật khẩu</span>
            </h2>
            <p class="text-sm text-gray-500 leading-relaxed pt-1">
              Nhập mã OTP 6 số vừa gửi tới <strong>{{ email }}</strong> và thiết lập mật khẩu mới của bạn.
            </p>
          </div>

          <!-- Error Alert Banner -->
          <div *ngIf="errorMessage" class="bg-rose-50 border border-rose-200 text-rose-600 p-3.5 rounded-2xl text-xs flex items-center gap-2.5">
            <i class="pi pi-exclamation-triangle text-base text-rose-500"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Form -->
          <form [formGroup]="resetForm" (ngSubmit)="onSubmit()" class="space-y-4">
            
            <!-- 1. Input OTP Code -->
            <div>
              <label class="text-xs font-semibold text-gray-700 mb-1.5 block">
                Mã OTP (6 chữ số)
              </label>
              <div class="relative">
                <input
                  type="text"
                  maxLength="6"
                  formControlName="code"
                  placeholder="Nhập 6 số OTP (VD: 839210)"
                  class="bg-white border border-gray-200 rounded-2xl pl-4 pr-10 py-3.5 text-sm text-gray-800 font-mono tracking-wider focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none w-full transition-all placeholder:text-gray-400 placeholder:tracking-normal"
                />
                <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 text-sm pointer-events-none">
                  <i class="pi pi-shield"></i>
                </span>
              </div>
              <div *ngIf="resetForm.get('code')?.touched && resetForm.get('code')?.invalid" class="text-xs text-rose-500 mt-1">
                Vui lòng nhập đủ 6 chữ số OTP.
              </div>
            </div>

            <!-- 2. Input Mật khẩu mới -->
            <div>
              <label class="text-xs font-semibold text-gray-700 mb-1.5 block">
                Mật khẩu mới
              </label>
              <div class="relative">
                <input
                  [type]="showPassword ? 'text' : 'password'"
                  formControlName="newPassword"
                  placeholder="Tối thiểu 6 ký tự"
                  class="bg-white border border-gray-200 rounded-2xl pl-4 pr-16 py-3.5 text-sm text-gray-800 focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none w-full transition-all placeholder:text-gray-400"
                />
                <div class="absolute right-3.5 top-1/2 -translate-y-1/2 flex items-center gap-1.5 text-gray-400 text-sm">
                  <i class="pi pi-lock"></i>
                  <button
                    type="button"
                    (click)="showPassword = !showPassword"
                    class="hover:text-gray-600 focus:outline-none transition-colors p-0.5"
                  >
                    <i [class]="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
                  </button>
                </div>
              </div>
              <div *ngIf="resetForm.get('newPassword')?.touched && resetForm.get('newPassword')?.invalid" class="text-xs text-rose-500 mt-1">
                Mật khẩu mới phải từ 6 ký tự trở lên.
              </div>
            </div>

            <!-- 3. Input Xác nhận mật khẩu mới -->
            <div>
              <label class="text-xs font-semibold text-gray-700 mb-1.5 block">
                Xác nhận mật khẩu mới
              </label>
              <div class="relative">
                <input
                  [type]="showConfirmPassword ? 'text' : 'password'"
                  formControlName="confirmPassword"
                  placeholder="Nhập lại mật khẩu mới"
                  class="bg-white border border-gray-200 rounded-2xl pl-4 pr-10 py-3.5 text-sm text-gray-800 focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none w-full transition-all placeholder:text-gray-400"
                />
                <button
                  type="button"
                  (click)="showConfirmPassword = !showConfirmPassword"
                  class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 text-sm focus:outline-none transition-colors p-1"
                >
                  <i [class]="showConfirmPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
                </button>
              </div>
              <div *ngIf="(resetForm.get('confirmPassword')?.touched && resetForm.get('confirmPassword')?.invalid) || resetForm.hasError('mismatch')" class="text-xs text-rose-500 mt-1">
                Mật khẩu xác nhận không trùng khớp.
              </div>
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              [disabled]="loading"
              class="w-full py-3.5 bg-gradient-to-r from-[#5B4DFF] to-[#6366F1] hover:opacity-95 text-white font-bold rounded-2xl shadow-lg shadow-indigo-200 hover:shadow-indigo-300 transition-all text-sm flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 mt-2"
            >
              <i *ngIf="loading" class="pi pi-spin pi-spinner text-sm"></i>
              <span>{{ loading ? 'Đang đổi mật khẩu...' : 'Đặt lại mật khẩu' }}</span>
              <i *ngIf="!loading" class="pi pi-arrow-right text-xs"></i>
            </button>
          </form>

          <!-- Back to Login Link -->
          <div class="pt-2 text-center">
            <a routerLink="/login" class="text-xs text-gray-500 hover:text-[#5B4DFF] font-semibold transition-colors">
              Hủy bỏ và quay lại Đăng nhập
            </a>
          </div>

        </div>
      </main>

    </div>
  `
})
export class ResetPasswordComponent implements OnInit {
  resetForm!: FormGroup;
  loading = false;
  errorMessage = '';
  email = '';
  showPassword = false;
  showConfirmPassword = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
    });

    this.resetForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  onSubmit(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const resetData = {
      email: this.email,
      code: this.resetForm.value.code,
      newPassword: this.resetForm.value.newPassword
    };

    this.authService.resetPassword(resetData).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/login'], { queryParams: { resetSuccess: 'true' } });
      },
      error: (err) => {
        this.loading = false;
        if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else if (err.error && typeof err.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = 'Mã OTP không chính xác hoặc đã hết hạn.';
        }
      }
    });
  }
}
