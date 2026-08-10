import { Component, OnInit, OnDestroy, ViewChild, ViewChildren, QueryList, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
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

          <!-- 1. Top Graphic: Padlock + Verified Check + 3x3 Dot Grids -->
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

              <!-- Center Glowing Circle with Padlock Graphic -->
              <div class="w-24 h-24 rounded-full bg-[#EEF0FF] flex items-center justify-center relative shadow-inner">
                <svg class="w-16 h-16 overflow-visible" viewBox="0 0 100 100" fill="none">
                  <!-- Shackle Loop -->
                  <path d="M30 42 C30 24 38 14 50 14 C62 14 70 24 70 42 L70 48 L30 48 Z" fill="none" stroke="#5B4DFF" stroke-width="6" stroke-linecap="round"/>

                  <!-- Lock Body -->
                  <rect x="20" y="44" width="60" height="42" rx="12" fill="#5B4DFF"/>
                  <rect x="26" y="50" width="48" height="30" rx="8" fill="#4335E6" opacity="0.95"/>
                  
                  <!-- Golden Keyhole -->
                  <circle cx="50" cy="62" r="4.5" fill="#FBBF24"/>
                  <polygon points="47.5,64 52.5,64 51.5,73 48.5,73" fill="#FBBF24"/>

                  <!-- Green Verified Badge -->
                  <circle cx="76" cy="74" r="13" fill="#10B981" stroke="#FFFFFF" stroke-width="2.5"/>
                  <path d="M71 74 L74.5 77.5 L81.5 70.5" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
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

          <!-- 2. Stepper Progress (Step 1 Completed, Step 2 Active) -->
          <div class="flex items-center justify-center mb-6 max-w-[280px] mx-auto">
            <!-- Step 1: Completed with Checkmark -->
            <div class="flex flex-col items-center gap-1.5 shrink-0">
              <div class="w-8 h-8 rounded-full border-2 border-[#5B4DFF] bg-white text-[#5B4DFF] font-bold text-sm flex items-center justify-center shadow-xs">
                <i class="pi pi-check text-xs text-[#5B4DFF]"></i>
              </div>
              <span class="text-xs font-medium text-gray-500 whitespace-nowrap">Xác minh email</span>
            </div>

            <!-- Connector Line -->
            <div class="flex-1 h-[1.5px] bg-indigo-200 mx-4 -mt-5"></div>

            <!-- Step 2: Active -->
            <div class="flex flex-col items-center gap-1.5 shrink-0">
              <div class="w-8 h-8 rounded-full bg-[#5B4DFF] text-white font-bold text-xs flex items-center justify-center shadow-md shadow-indigo-200">
                2
              </div>
              <span class="text-xs font-bold text-[#5B4DFF] whitespace-nowrap">Đặt lại mật khẩu</span>
            </div>
          </div>

          <!-- 3. Title & Subtitle -->
          <div class="mb-5">
            <h2 class="text-3xl sm:text-[34px] font-black text-gray-900 tracking-tight leading-tight">
              Đặt lại <span class="text-[#5B4DFF]">mật khẩu</span>
            </h2>
            <p class="text-sm text-gray-500 mt-2 leading-relaxed max-w-sm mx-auto">
              Nhập mã xác thực và thiết lập mật khẩu mới
            </p>
          </div>

          <!-- 4. Email Pill Banner -->
          <div *ngIf="email" class="bg-[#F4F5FF] rounded-2xl px-4 py-3 flex items-center justify-between text-xs mb-5 border border-purple-100/50">
            <div class="flex items-center gap-2 text-gray-700 font-medium overflow-hidden">
              <i class="pi pi-envelope text-[#5B4DFF] text-base shrink-0"></i>
              <span class="truncate max-w-[210px] font-semibold text-gray-900">{{ email }}</span>
            </div>
            <a routerLink="/forgot-password" class="text-[#5B4DFF] font-bold hover:underline shrink-0 cursor-pointer">
              Đổi email
            </a>
          </div>

          <!-- Error Alert Banner -->
          <div *ngIf="errorMessage" class="bg-rose-50 text-rose-600 p-3.5 rounded-2xl text-xs flex items-center justify-center gap-2 mb-4">
            <i class="pi pi-exclamation-triangle text-sm text-rose-500"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Success Alert Banner -->
          <div *ngIf="successMessage" class="bg-emerald-50 text-emerald-700 p-3.5 rounded-2xl text-xs flex items-center justify-center gap-2 mb-4 border border-emerald-100">
            <i class="pi pi-check-circle text-sm text-emerald-500"></i>
            <span>{{ successMessage }}</span>
          </div>

          <!-- 5. Form -->
          <form [formGroup]="resetForm" (ngSubmit)="onSubmit()" class="space-y-4">
            
            <!-- OTP Digits Section (6 Distinct Input Boxes) -->
            <div>
              <label class="text-xs font-bold text-gray-800 block text-left mb-2">
                Mã xác thực OTP
              </label>
              <div class="flex justify-between items-center gap-2 mb-2.5" (paste)="onOtpPaste($event)">
                <input
                  *ngFor="let digit of otpDigits; let i = index; trackBy: trackByIndex"
                  #otpInput
                  type="text"
                  maxLength="1"
                  pattern="[0-9]*"
                  inputmode="numeric"
                  [value]="otpDigits[i]"
                  (input)="onOtpInput(i, $event)"
                  (keydown)="onOtpKeyDown(i, $event)"
                  class="w-11 h-13 sm:w-12 sm:h-14 text-center font-extrabold text-2xl text-gray-900 bg-white border border-gray-200/90 rounded-2xl shadow-[inset_0_2px_4px_rgba(0,0,0,0.02)] focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none transition-all"
                />
              </div>

              <!-- Expiration Timer -->
              <div class="flex items-center justify-center gap-1.5 text-xs text-gray-500 mt-1 mb-2">
                <span class="text-[#5B4DFF]"><i class="pi pi-shield text-xs"></i></span>
                <span>Mã có hiệu lực trong <strong class="text-[#5B4DFF] font-bold">{{ formatSeconds(validityCountdown) }}</strong></span>
              </div>
            </div>

            <!-- Mật khẩu mới -->
            <div>
              <label class="text-xs font-bold text-gray-800 block text-left mb-1.5">
                Mật khẩu mới
              </label>
              <div class="relative flex items-center">
                <span class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 text-sm pointer-events-none flex items-center justify-center w-5 h-5">
                  <i class="pi pi-lock"></i>
                </span>
                <input
                  #newPasswordInput
                  [type]="showPassword ? 'text' : 'password'"
                  formControlName="newPassword"
                  placeholder="••••••••••••"
                  class="bg-white border border-gray-200/90 rounded-2xl pl-11 pr-11 py-3.5 text-sm text-gray-800 shadow-[inset_0_2px_4px_rgba(0,0,0,0.02)] focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none w-full transition-all placeholder:text-gray-400 font-medium"
                />
                <button
                  type="button"
                  (click)="showPassword = !showPassword"
                  class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#5B4DFF] focus:outline-none transition-colors p-1"
                >
                  <i [class]="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
                </button>
              </div>
              <div *ngIf="resetForm.get('newPassword')?.touched && resetForm.get('newPassword')?.invalid" class="text-xs text-rose-500 mt-1 text-left">
                Mật khẩu mới phải từ 6 ký tự trở lên.
              </div>
            </div>

            <!-- Xác nhận mật khẩu mới -->
            <div>
              <label class="text-xs font-bold text-gray-800 block text-left mb-1.5">
                Xác nhận mật khẩu mới
              </label>
              <div class="relative flex items-center">
                <span class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 text-sm pointer-events-none flex items-center justify-center w-5 h-5">
                  <i class="pi pi-lock"></i>
                </span>
                <input
                  [type]="showConfirmPassword ? 'text' : 'password'"
                  formControlName="confirmPassword"
                  placeholder="••••••••••••"
                  class="bg-white border border-gray-200/90 rounded-2xl pl-11 pr-11 py-3.5 text-sm text-gray-800 shadow-[inset_0_2px_4px_rgba(0,0,0,0.02)] focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none w-full transition-all placeholder:text-gray-400 font-medium"
                />
                <button
                  type="button"
                  (click)="showConfirmPassword = !showConfirmPassword"
                  class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#5B4DFF] focus:outline-none transition-colors p-1"
                >
                  <i [class]="showConfirmPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
                </button>
              </div>
              <div *ngIf="(resetForm.get('confirmPassword')?.touched && resetForm.get('confirmPassword')?.invalid) || resetForm.hasError('mismatch')" class="text-xs text-rose-500 mt-1 text-left">
                Mật khẩu xác nhận không trùng khớp.
              </div>
            </div>

            <!-- Primary Submit Button (#5B4DFF with Checkmark) -->
            <button
              type="submit"
              [disabled]="loading || getOtpString().length < 6"
              class="w-full py-4 bg-[#5B4DFF] hover:bg-[#4d3ef7] text-white font-bold rounded-2xl shadow-lg shadow-indigo-200/80 hover:shadow-indigo-300 transition-all text-sm flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 mt-5 active:scale-[0.99]"
            >
              <i *ngIf="loading" class="pi pi-spin pi-spinner text-sm"></i>
              <i *ngIf="!loading" class="pi pi-check text-sm font-bold"></i>
              <span>{{ loading ? 'Đang cập nhật...' : 'Cập nhật mật khẩu' }}</span>
            </button>

            <!-- Divider -->
            <div class="relative flex py-1 items-center text-xs text-gray-400 font-medium">
              <div class="flex-grow border-t border-gray-100"></div>
              <span class="flex-shrink mx-4 text-gray-400 font-medium">Hoặc</span>
              <div class="flex-grow border-t border-gray-100"></div>
            </div>

            <!-- Resend OTP Button -->
            <button
              type="button"
              [disabled]="resendCountdown > 0 || resending"
              (click)="onResendOtp()"
              class="w-full py-3.5 bg-white border border-gray-200/90 hover:bg-gray-50 text-gray-700 font-semibold rounded-2xl flex items-center justify-center gap-2 text-xs transition-all shadow-xs cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed active:scale-[0.99]"
            >
              <i class="pi pi-refresh text-xs text-[#5B4DFF]" [class.pi-spin]="resending"></i>
              <span>{{ resendCountdown > 0 ? 'Gửi lại mã (' + resendCountdown + 's)' : 'Gửi lại mã OTP' }}</span>
            </button>

            <!-- Bottom Centered Link -->
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
export class ResetPasswordComponent implements OnInit, OnDestroy {
  @ViewChildren('otpInput') otpInputs!: QueryList<ElementRef>;
  @ViewChild('newPasswordInput') newPasswordInput!: ElementRef;

  resetForm!: FormGroup;
  otpDigits: string[] = ['', '', '', '', '', ''];
  loading = false;
  resending = false;
  errorMessage = '';
  successMessage = '';
  email = '';
  showPassword = false;
  showConfirmPassword = false;

  validityCountdown: number = 600; // 10:00
  resendCountdown: number = 60;
  countdownInterval: any;
  resendInterval: any;

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
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.startCountdown();
    this.startResendTimer();
  }

  ngOnDestroy(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
    if (this.resendInterval) {
      clearInterval(this.resendInterval);
    }
  }

  trackByIndex(index: number): number {
    return index;
  }

  startCountdown(): void {
    if (this.countdownInterval) clearInterval(this.countdownInterval);
    this.validityCountdown = 600;
    this.countdownInterval = setInterval(() => {
      if (this.validityCountdown > 0) {
        this.validityCountdown--;
      } else {
        clearInterval(this.countdownInterval);
      }
    }, 1000);
  }

  startResendTimer(): void {
    if (this.resendInterval) clearInterval(this.resendInterval);
    this.resendCountdown = 60;
    this.resendInterval = setInterval(() => {
      if (this.resendCountdown > 0) {
        this.resendCountdown--;
      } else {
        clearInterval(this.resendInterval);
      }
    }, 1000);
  }

  formatSeconds(totalSeconds: number): string {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  getOtpString(): string {
    return this.otpDigits.join('');
  }

  onOtpInput(index: number, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    const rawVal = inputEl.value || '';
    const digitsOnly = rawVal.replace(/\D/g, '');
    const lastChar = digitsOnly ? digitsOnly.slice(-1) : '';

    this.otpDigits[index] = lastChar;
    inputEl.value = lastChar;

    if (lastChar) {
      if (index < 5) {
        setTimeout(() => {
          const inputsArray = this.otpInputs.toArray();
          if (inputsArray[index + 1]) {
            inputsArray[index + 1].nativeElement.focus();
            inputsArray[index + 1].nativeElement.select();
          }
        }, 0);
      } else {
        setTimeout(() => {
          this.newPasswordInput?.nativeElement?.focus();
        }, 0);
      }
    }
  }

  onOtpKeyDown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace') {
      const inputEl = event.target as HTMLInputElement;
      if (!inputEl.value && index > 0) {
        event.preventDefault();
        this.otpDigits[index - 1] = '';
        setTimeout(() => {
          const inputsArray = this.otpInputs.toArray();
          if (inputsArray[index - 1]) {
            inputsArray[index - 1].nativeElement.value = '';
            inputsArray[index - 1].nativeElement.focus();
          }
        }, 0);
      }
    }
  }

  onOtpPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pastedData = event.clipboardData?.getData('text') || '';
    const cleanDigits = pastedData.replace(/\D/g, '').slice(0, 6);
    const inputsArray = this.otpInputs.toArray();

    for (let i = 0; i < 6; i++) {
      const char = cleanDigits[i] || '';
      this.otpDigits[i] = char;
      if (inputsArray[i]) {
        inputsArray[i].nativeElement.value = char;
      }
    }

    if (cleanDigits.length > 0) {
      const focusIndex = Math.min(cleanDigits.length, 5);
      setTimeout(() => {
        if (cleanDigits.length === 6) {
          this.newPasswordInput?.nativeElement?.focus();
        } else if (inputsArray[focusIndex]) {
          inputsArray[focusIndex].nativeElement.focus();
        }
      }, 0);
    }
  }

  onResendOtp(): void {
    if (!this.email) {
      this.errorMessage = 'Không tìm thấy địa chỉ email để gửi lại mã.';
      return;
    }

    this.resending = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.resending = false;
        this.successMessage = 'Mã xác thực mới đã được gửi đến email của bạn!';
        this.startResendTimer();
        this.startCountdown();
        this.otpDigits = ['', '', '', '', '', ''];
        setTimeout(() => {
          const firstInput = this.otpInputs?.first;
          if (firstInput) {
            firstInput.nativeElement.focus();
          }
        }, 0);
      },
      error: (err) => {
        this.resending = false;
        if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else {
          this.errorMessage = 'Gửi lại mã xác thực thất bại. Vui lòng thử lại sau.';
        }
      }
    });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  onSubmit(): void {
    const code = this.getOtpString();
    if (code.length < 6) {
      this.errorMessage = 'Vui lòng nhập đủ 6 chữ số mã xác thực OTP.';
      return;
    }

    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const resetData = {
      email: this.email,
      code: code,
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
          this.errorMessage = 'Mã xác thực không chính xác hoặc đã hết hạn.';
        }
      }
    });
  }
}
