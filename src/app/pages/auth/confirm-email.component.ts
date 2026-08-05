import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen w-full bg-gradient-to-br from-[#F4F5FF] via-[#EEF0FF] to-[#EAEFFF] relative overflow-hidden flex flex-col justify-between selection:bg-[#5B4DFF] selection:text-white">

      <!-- Top Header -->
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

      <!-- Decorative Glows -->
      <div class="absolute -top-32 -left-32 w-96 h-96 bg-[#5B4DFF]/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-32 -right-32 w-96 h-96 bg-indigo-400/20 rounded-full blur-3xl pointer-events-none"></div>

      <!-- Main Center Content -->
      <main class="relative z-10 min-h-screen flex items-center justify-center p-6 lg:p-12 max-w-lg mx-auto w-full">
        <div class="bg-white p-8 lg:p-10 rounded-[32px] shadow-2xl shadow-indigo-100/60 border border-gray-50 text-center w-full my-auto">

          <!-- State 1: Loading -->
          <div *ngIf="loading" class="py-8 space-y-4">
            <div class="w-16 h-16 bg-purple-100 text-[#5B4DFF] rounded-full flex items-center justify-center mx-auto animate-pulse">
              <i class="pi pi-spin pi-spinner text-3xl"></i>
            </div>
            <h2 class="text-2xl font-extrabold text-gray-900">Đang xác thực email...</h2>
            <p class="text-sm text-gray-500">Vui lòng chờ trong giây lát, hệ thống đang kiểm tra liên kết xác thực của bạn.</p>
          </div>

          <!-- State 2: Success -->
          <div *ngIf="!loading && success" class="py-6 space-y-5">
            <div class="w-20 h-20 bg-emerald-100 text-emerald-600 rounded-full flex items-center justify-center mx-auto shadow-lg shadow-emerald-100">
              <i class="pi pi-check text-4xl"></i>
            </div>
            <div>
              <h2 class="text-3xl font-extrabold text-gray-900 tracking-tight">
                Xác thực <span class="text-emerald-600">thành công!</span>
              </h2>
              <p class="text-sm text-gray-500 mt-2 leading-relaxed">
                Tài khoản <strong>{{ email }}</strong> của bạn đã được kích hoạt thành công. Bạn có thể đăng nhập và trải nghiệm StudyHub ngay bây giờ!
              </p>
            </div>
            <div class="pt-4">
              <a 
                routerLink="/login" 
                class="inline-flex items-center justify-center gap-2 w-full py-3.5 bg-gradient-to-r from-[#5B4DFF] to-[#6366F1] hover:opacity-95 text-white font-bold rounded-2xl shadow-lg shadow-indigo-200 hover:shadow-indigo-300 transition-all text-sm cursor-pointer"
              >
                <span>Đăng nhập ngay</span>
                <i class="pi pi-arrow-right text-xs"></i>
              </a>
            </div>
          </div>

          <!-- State 3: Error -->
          <div *ngIf="!loading && !success" class="py-6 space-y-5">
            <div class="w-20 h-20 bg-rose-100 text-rose-600 rounded-full flex items-center justify-center mx-auto shadow-lg shadow-rose-100">
              <i class="pi pi-times text-4xl"></i>
            </div>
            <div>
              <h2 class="text-3xl font-extrabold text-gray-900 tracking-tight">
                Xác thực <span class="text-rose-600">thất bại</span>
              </h2>
              <p class="text-sm text-rose-500 mt-2 leading-relaxed bg-rose-50 p-3.5 rounded-2xl border border-rose-200">
                {{ errorMessage }}
              </p>
            </div>
            <div class="pt-4 space-y-3">
              <a 
                routerLink="/register" 
                class="inline-flex items-center justify-center gap-2 w-full py-3.5 bg-gradient-to-r from-[#5B4DFF] to-[#6366F1] hover:opacity-95 text-white font-bold rounded-2xl shadow-lg shadow-indigo-200 hover:shadow-indigo-300 transition-all text-sm cursor-pointer"
              >
                <span>Tạo tài khoản mới</span>
              </a>
              <a 
                routerLink="/login" 
                class="block text-xs text-gray-500 hover:text-[#5B4DFF] font-semibold transition-colors"
              >
                Quay lại trang Đăng nhập
              </a>
            </div>
          </div>

        </div>
      </main>
    </div>
  `
})
export class ConfirmEmailComponent implements OnInit {
  loading = true;
  success = false;
  errorMessage = '';
  email = '';
  token = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';

      if (!this.email || !this.token) {
        this.loading = false;
        this.success = false;
        this.errorMessage = 'Liên kết xác thực không hợp lệ hoặc thiếu thông tin.';
        return;
      }

      this.verifyEmail();
    });
  }

  verifyEmail(): void {
    this.loading = true;
    this.authService.confirmEmail(this.email, this.token).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;
      },
      error: (err) => {
        this.loading = false;
        this.success = false;
        if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else if (err.error && typeof err.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = 'Mã xác thực email không hợp lệ hoặc đã hết hạn.';
        }
      }
    });
  }
}
