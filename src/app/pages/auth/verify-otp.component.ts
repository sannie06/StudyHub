import { Component, OnInit, OnDestroy, ElementRef, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="min-h-screen w-full bg-gradient-to-br from-[#F4F5FF] via-[#EEF0FF] to-[#EAEFFF] relative overflow-hidden flex flex-col justify-between selection:bg-[#5B4DFF] selection:text-white">

      <!-- ================================================== -->
      <!-- TOP NAVIGATION HEADER                              -->
      <!-- ================================================== -->
      <header class="absolute top-0 left-0 right-0 w-full p-6 lg:px-12 flex justify-between items-center z-30 pointer-events-auto">
        <!-- Left Logo Unit -->
        <div class="flex items-center gap-3 cursor-pointer" routerLink="/">
          <div class="bg-[#5B4DFF] p-2.5 rounded-2xl text-white shadow-lg shadow-indigo-200 flex items-center justify-center">
            <i class="pi pi-graduation-cap text-xl"></i>
          </div>
          <span class="font-extrabold text-2xl tracking-tight text-gray-900">
            Study<span class="text-[#5B4DFF]">Hub</span>
          </span>
        </div>

        <!-- Right Action Link -->
        <div class="flex items-center gap-3 text-sm">
          <span class="text-gray-500 hidden sm:inline">Đã có tài khoản?</span>
          <a routerLink="/login" class="px-5 py-2 bg-white border border-[#5B4DFF] text-[#5B4DFF] hover:bg-[#5B4DFF] hover:text-white font-semibold text-xs rounded-full transition-all cursor-pointer shadow-xs">
            Đăng nhập
          </a>
        </div>
      </header>

      <!-- Background Decorative Glow Shapes -->
      <div class="absolute -top-32 -left-32 w-96 h-96 bg-[#5B4DFF]/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-32 -right-32 w-96 h-96 bg-indigo-400/20 rounded-full blur-3xl pointer-events-none"></div>

      <!-- Background SVG Swoosh Lines Decor -->
      <svg class="absolute left-0 top-1/4 w-3/5 h-3/5 opacity-25 pointer-events-none" viewBox="0 0 600 600" fill="none">
        <path d="M-100,200 C180,100 250,450 550,300" stroke="#5B4DFF" stroke-width="2" stroke-dasharray="6 6" />
        <path d="M-50,150 C230,80 300,400 600,250" stroke="#818CF8" stroke-width="1.5" />
        <path d="M0,100 C280,60 350,350 650,200" stroke="#C7D2FE" stroke-width="1" />
      </svg>

      <!-- ================================================== -->
      <!-- MAIN CONTAINER (GRID 12 COLS - RATIO 60:40)        -->
      <!-- ================================================== -->
      <main class="relative z-10 grid grid-cols-12 gap-10 xl:gap-16 min-h-screen items-center p-6 lg:p-12 pt-24 lg:pt-20 pb-12 max-w-[1400px] mx-auto w-full">

        <!-- ================================================== -->
        <!-- LEFT COLUMN (HERO PANEL - 7 COLS / 60%)           -->
        <!-- ================================================== -->
        <div class="hidden lg:flex col-span-12 lg:col-span-7 xl:col-span-7 flex-col justify-center space-y-6 relative">
          
          <!-- Dot Pattern Top Left -->
          <div class="absolute -top-10 -left-6 opacity-30 pointer-events-none grid grid-cols-5 gap-2 text-indigo-400 text-xs">
            <span>•</span><span>•</span><span>•</span><span>•</span><span>•</span>
            <span>•</span><span>•</span><span>•</span><span>•</span><span>•</span>
            <span>•</span><span>•</span><span>•</span><span>•</span><span>•</span>
          </div>

          <!-- 1. Step Badge -->
          <div>
            <span class="inline-flex items-center px-4 py-1.5 rounded-full text-xs font-semibold bg-purple-100/80 text-[#5B4DFF] border border-purple-200/50">
              Bước 2/3
            </span>
          </div>

          <!-- 2. Dual-line Heading & Underline Bar -->
          <div>
            <h1 class="text-4xl lg:text-[46px] xl:text-5xl font-extrabold text-gray-900 leading-[1.18] tracking-tight">
              Xác thực <span class="text-[#5B4DFF]">email</span><br />
              để hoàn tất đăng ký
            </h1>
            <div class="w-16 h-[3.5px] bg-[#5B4DFF] rounded-full mt-2 mb-4"></div>
          </div>

          <!-- 3. Subtitle -->
          <p class="text-gray-500 text-sm sm:text-base leading-relaxed max-w-lg">
            Chúng tôi vừa gửi mã xác thực đến email của bạn.<br />
            Vui lòng nhập mã để xác minh và kích hoạt tài khoản.
          </p>

          <!-- 4. 3 Feature Cards Row (Matching image_bec6f8.jpg) -->
          <div class="grid grid-cols-3 gap-4 w-full mt-6">
            
            <!-- Card 1: Shield -->
            <div class="bg-white/95 backdrop-blur-sm p-4 lg:p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col items-start gap-3 hover:shadow-md transition-all group">
              <div class="w-10 h-10 bg-purple-100/80 rounded-xl flex items-center justify-center text-[#5B4DFF] group-hover:bg-[#5B4DFF] group-hover:text-white transition-all shrink-0">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"/></svg>
              </div>
              <div>
                <h4 class="text-xs sm:text-sm font-bold text-gray-900">Bảo mật tuyệt đối</h4>
                <p class="text-[11px] text-gray-400 mt-1 leading-snug">Mã xác thực giúp đảm bảo tài khoản của bạn an toàn.</p>
              </div>
            </div>

            <!-- Card 2: Lightning -->
            <div class="bg-white/95 backdrop-blur-sm p-4 lg:p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col items-start gap-3 hover:shadow-md transition-all group">
              <div class="w-10 h-10 bg-purple-100/80 rounded-xl flex items-center justify-center text-[#5B4DFF] group-hover:bg-[#5B4DFF] group-hover:text-white transition-all shrink-0">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/></svg>
              </div>
              <div>
                <h4 class="text-xs sm:text-sm font-bold text-gray-900">Xác thực nhanh chóng</h4>
                <p class="text-[11px] text-gray-400 mt-1 leading-snug">Chỉ mất vài giây để xác minh và hoàn tất đăng ký.</p>
              </div>
            </div>

            <!-- Card 3: Check Circle -->
            <div class="bg-white/95 backdrop-blur-sm p-4 lg:p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col items-start gap-3 hover:shadow-md transition-all group">
              <div class="w-10 h-10 bg-purple-100/80 rounded-xl flex items-center justify-center text-[#5B4DFF] group-hover:bg-[#5B4DFF] group-hover:text-white transition-all shrink-0">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
              </div>
              <div>
                <h4 class="text-xs sm:text-sm font-bold text-gray-900">Trải nghiệm liền mạch</h4>
                <p class="text-[11px] text-gray-400 mt-1 leading-snug">Kích hoạt tài khoản để bắt đầu hành trình học tập hiệu quả.</p>
              </div>
            </div>

          </div>

          <!-- Dot Pattern Bottom Left -->
          <div class="absolute -bottom-8 -left-6 opacity-30 pointer-events-none grid grid-cols-5 gap-2 text-indigo-400 text-xs">
            <span>•</span><span>•</span><span>•</span><span>•</span><span>•</span>
            <span>•</span><span>•</span><span>•</span><span>•</span><span>•</span>
            <span>•</span><span>•</span><span>•</span><span>•</span><span>•</span>
          </div>

        </div>

        <!-- ================================================== -->
        <!-- RIGHT COLUMN (OTP FORM CARD - 5 COLS / 40%)        -->
        <!-- ================================================== -->
        <div class="col-span-12 lg:col-span-5 xl:col-span-5 flex justify-center lg:justify-end w-full my-auto">
          <div class="bg-white p-7 lg:p-9 rounded-[36px] shadow-2xl shadow-indigo-100/60 border border-gray-50 text-center max-w-md w-full relative">
            
            <!-- Envelope 3D Style Graphic Illustration -->
            <div class="w-32 h-24 mx-auto relative flex items-center justify-center mb-2">
              <svg class="w-full h-full drop-shadow-md" viewBox="0 0 160 120" fill="none">
                <!-- Background ambient glow -->
                <ellipse cx="80" cy="95" rx="55" ry="12" fill="#EEF0FF" />
                <!-- Floating passcode card -->
                <rect x="42" y="10" width="76" height="45" rx="12" fill="#FFFFFF" stroke="#E0E7FF" stroke-width="1.5" />
                <circle cx="56" cy="24" r="3" fill="#818CF8"/>
                <circle cx="68" cy="24" r="3" fill="#818CF8"/>
                <circle cx="80" cy="24" r="3" fill="#818CF8"/>
                <circle cx="92" cy="24" r="3" fill="#818CF8"/>
                <circle cx="104" cy="24" r="3" fill="#818CF8"/>
                <!-- Envelope body -->
                <path d="M28 40 L132 40 L132 95 C132 102 126 107 119 107 L41 107 C34 107 28 102 28 95 Z" fill="#7C3AED" />
                <!-- Inner fold -->
                <path d="M28 40 L80 75 L132 40 L80 92 Z" fill="#6D28D9" opacity="0.95" />
                <!-- Front flap -->
                <path d="M28 40 L80 75 L132 40 Z" fill="#8B5CF6" />
                <!-- Green checkmark badge -->
                <circle cx="125" cy="85" r="15" fill="#10B981" stroke="#FFFFFF" stroke-width="2.5" />
                <path d="M118 85 L123 90 L132 80" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>

            <!-- Title & Subtitle -->
            <div>
              <h2 class="text-2xl sm:text-3xl font-extrabold text-gray-900 tracking-tight">
                Nhập mã <span class="text-[#5B4DFF]">xác thực</span>
              </h2>
              <p class="text-xs text-gray-400 mt-1 mb-4">Mã xác thực đã được gửi đến</p>
            </div>

            <!-- Email Pill Banner -->
            <div class="bg-[#F4F5FF] rounded-2xl px-4 py-2.5 flex items-center justify-between text-xs mb-5 border border-purple-100/50">
              <div class="flex items-center gap-2 text-gray-700 font-medium overflow-hidden">
                <i class="pi pi-envelope text-[#5B4DFF] text-sm shrink-0"></i>
                <span class="truncate max-w-[190px] font-semibold text-gray-900">{{ email || 'admin@studyhub.com' }}</span>
              </div>
              <button type="button" (click)="onChangeEmail()" class="text-[#5B4DFF] font-bold hover:underline shrink-0 cursor-pointer">
                Đổi email
              </button>
            </div>

            <!-- Error Alert Banner -->
            <div *ngIf="errorMessage" class="bg-rose-50 border border-rose-200 text-rose-600 p-3 rounded-2xl text-xs flex items-center justify-center gap-2 mb-4">
              <i class="pi pi-exclamation-triangle text-sm text-rose-500"></i>
              <span>{{ errorMessage }}</span>
            </div>

            <!-- Success Alert Banner -->
            <div *ngIf="successMessage" class="bg-emerald-50 border border-emerald-200 text-emerald-700 p-3 rounded-2xl text-xs flex items-center justify-center gap-2 mb-4">
              <i class="pi pi-check-circle text-sm text-emerald-500"></i>
              <span>{{ successMessage }}</span>
            </div>

            <!-- Form Inputs -->
            <form (ngSubmit)="onVerify()" class="space-y-4">
              
              <!-- 6 OTP Input Boxes -->
              <div>
                <label class="text-xs font-bold text-gray-700 block text-left mb-2">
                  Mã xác thực
                </label>
                <div class="flex justify-between items-center gap-2" (paste)="onPaste($event)">
                  <input
                    *ngFor="let digit of otpDigits; let i = index"
                    #otpInput
                    type="text"
                    maxLength="1"
                    pattern="[0-9]*"
                    inputmode="numeric"
                    [(ngModel)]="otpDigits[i]"
                    [name]="'otp' + i"
                    (input)="onInput(i, $event)"
                    (keydown)="onKeyDown(i, $event)"
                    class="w-11 h-13 sm:w-12 sm:h-14 text-center font-extrabold text-2xl text-gray-900 bg-white border border-gray-200 rounded-2xl focus:border-[#5B4DFF] focus:ring-4 focus:ring-indigo-100 outline-none transition-all"
                  />
                </div>
              </div>

              <!-- Expiration Notice -->
              <p class="text-xs text-gray-400 text-center py-1">
                Mã có hiệu lực trong <span class="font-bold text-[#5B4DFF]">{{ formatSeconds(validityCountdown) }}</span>
              </p>

              <!-- Primary Submit Button -->
              <button
                type="submit"
                [disabled]="loading || getOtpString().length < 6"
                class="w-full py-3.5 bg-gradient-to-r from-[#5B4DFF] to-[#6366F1] hover:opacity-95 text-white font-bold rounded-2xl shadow-lg shadow-indigo-200 hover:shadow-indigo-300 transition-all text-sm flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 mt-2"
              >
                <i *ngIf="loading" class="pi pi-spin pi-spinner text-sm"></i>
                <span>{{ loading ? 'Đang xác thực...' : 'Xác thực ngay' }}</span>
                <i *ngIf="!loading" class="pi pi-arrow-right text-xs"></i>
              </button>

            </form>

            <!-- Divider -->
            <div class="relative flex py-3 items-center text-xs text-gray-400 font-medium">
              <div class="flex-grow border-t border-gray-100"></div>
              <span class="flex-shrink mx-4 text-gray-400">Hoặc</span>
              <div class="flex-grow border-t border-gray-100"></div>
            </div>

            <!-- Resend Button -->
            <button
              type="button"
              [disabled]="resendCountdown > 0 || resending"
              (click)="onResendOtp()"
              class="w-full py-3 bg-white border border-gray-200 hover:bg-gray-50 text-gray-700 font-semibold rounded-2xl flex items-center justify-center gap-2 text-xs transition-all shadow-xs cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed"
            >
              <i class="pi pi-refresh text-xs text-[#5B4DFF]"></i>
              <span>{{ resendCountdown > 0 ? 'Gửi lại mã (' + resendCountdown + 's)' : 'Gửi lại mã OTP' }}</span>
            </button>

            <!-- Back Link -->
            <div class="pt-3">
              <button
                type="button"
                (click)="onBack()"
                class="text-xs text-gray-500 hover:text-[#5B4DFF] font-semibold flex items-center justify-center gap-1.5 mx-auto transition-colors cursor-pointer"
              >
                <i class="pi pi-arrow-left text-[10px]"></i>
                <span>Quay lại</span>
              </button>
            </div>

          </div>
        </div>

      </main>
    </div>
  `
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  @ViewChildren('otpInput') otpInputs!: QueryList<ElementRef>;

  email: string = '';
  otpDigits: string[] = ['', '', '', '', '', ''];
  loading: boolean = false;
  resending: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  validityCountdown: number = 600; // 10 minutes total validity timer
  resendCountdown: number = 60; // 60 seconds resend timer
  timerInterval: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
    });

    this.startTimers();
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  startTimers(): void {
    this.resendCountdown = 60;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
    this.timerInterval = setInterval(() => {
      if (this.resendCountdown > 0) {
        this.resendCountdown--;
      }
      if (this.validityCountdown > 0) {
        this.validityCountdown--;
      }
    }, 1000);
  }

  formatSeconds(totalSec: number): string {
    const min = Math.floor(totalSec / 60);
    const sec = totalSec % 60;
    return `${min.toString().padStart(2, '0')}:${sec.toString().padStart(2, '0')}`;
  }

  getOtpString(): string {
    return this.otpDigits.join('');
  }

  onInput(index: number, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    const rawVal = inputEl.value || '';
    const digitsOnly = rawVal.replace(/\D/g, '');
    const lastChar = digitsOnly ? digitsOnly.slice(-1) : '';

    // Synchronize both model and DOM element
    this.otpDigits[index] = lastChar;
    inputEl.value = lastChar;

    // Defer focus shift to microtask queue to prevent keystroke event bleeding
    if (lastChar && index < 5) {
      setTimeout(() => {
        const inputsArray = this.otpInputs.toArray();
        if (inputsArray[index + 1]) {
          inputsArray[index + 1].nativeElement.focus();
        }
      }, 0);
    }

    if (this.getOtpString().length === 6) {
      this.onVerify();
    }
  }

  onKeyDown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace') {
      if (!this.otpDigits[index] && index > 0) {
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

  onPaste(event: ClipboardEvent): void {
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
        if (inputsArray[focusIndex]) {
          inputsArray[focusIndex].nativeElement.focus();
        }
      }, 0);
    }

    if (cleanDigits.length === 6) {
      this.onVerify();
    }
  }

  onChangeEmail(): void {
    this.router.navigate(['/register']);
  }

  onBack(): void {
    this.router.navigate(['/register']);
  }

  onVerify(): void {
    const code = this.getOtpString();
    if (code.length < 6) {
      this.errorMessage = 'Vui lòng nhập đủ 6 chữ số mã xác thực.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.verifyOtp(this.email, code, 'Register').subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Xác thực OTP thành công! Đang chuyển đến Đăng nhập...';
        setTimeout(() => {
          this.router.navigate(['/login'], { queryParams: { registered: 'true' } });
        }, 1200);
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

  onResendOtp(): void {
    if (!this.email) {
      this.errorMessage = 'Không tìm thấy địa chỉ email để gửi lại mã.';
      return;
    }

    this.resending = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.resendOtp(this.email).subscribe({
      next: () => {
        this.resending = false;
        this.successMessage = 'Mã xác thực mới đã được gửi tới email của bạn.';
        this.errorMessage = '';

        // Reset otpDigits array and clear native DOM values
        this.otpDigits = ['', '', '', '', '', ''];
        const inputsArray = this.otpInputs.toArray();
        inputsArray.forEach(input => {
          if (input?.nativeElement) {
            input.nativeElement.value = '';
          }
        });

        // Focus first input box (index 0)
        setTimeout(() => {
          if (inputsArray[0]?.nativeElement) {
            inputsArray[0].nativeElement.focus();
          }
        }, 0);

        this.validityCountdown = 600;
        this.startTimers();
      },
      error: (err) => {
        this.resending = false;
        if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else {
          this.errorMessage = 'Không thể gửi lại mã. Vui lòng thử lại sau.';
        }
      }
    });
  }
}
