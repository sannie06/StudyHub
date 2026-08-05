import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

export interface UserStats {
  congViecHoanThanh: number;
  tongCongViec: number;
  tyLeHoanThanh: number;
  tongPomodoro: number;
  tongPhutHoc: number;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule
  ],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  activeTab = 'personal'; // 'personal', 'password', 'settings', 'security'
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  loading = false;
  uploading = false;
  successMessage = '';
  errorMessage = '';

  // Password Visibility Toggle variables
  showOldPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  // Account Settings State variables
  settings = {
    emailNotify: true,
    systemNotify: true,
    darkMode: false,
    language: 'vi'
  };

  // Mock-friendly statistics
  stats: UserStats = {
    congViecHoanThanh: 25,
    tongCongViec: 30,
    tyLeHoanThanh: 83,
    tongPomodoro: 12,
    tongPhutHoc: 300
  };

  // Default Mock User Profile Data
  profileData: any = {
    hoTen: 'Nguyễn Minh Anh',
    email: 'minhanh2210@example.com',
    soDienThoai: '0123 456 789',
    ngaySinh: '2003-10-22',
    gioiTinh: 0, // Nữ
    truongKhoa: 'Đại học Bách Khoa Hà Nội',
    chuyenNganh: 'Khoa học máy tính',
    anhDaiDien: '',
    vaiTro: 'Sinh viên'
  };

  genderOptions = [
    { label: 'Nữ', value: 0 },
    { label: 'Nam', value: 1 },
    { label: 'Khác', value: 2 }
  ];

  majorOptions = [
    'Khoa học máy tính',
    'Công nghệ thông tin',
    'Kỹ thuật máy tính',
    'An toàn thông tin',
    'Hệ thống thông tin'
  ];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient
  ) {}

  ngOnInit() {
    this.initForms();
    this.loadSettings();
    this.loadProfile();
    this.loadStats();
  }

  initForms() {
    this.profileForm = this.fb.group({
      hoTen: [this.profileData.hoTen, [Validators.required, Validators.maxLength(100)]],
      soDienThoai: [this.profileData.soDienThoai, [Validators.maxLength(15)]],
      ngaySinh: [this.profileData.ngaySinh],
      gioiTinh: [this.profileData.gioiTinh],
      truongKhoa: [this.profileData.truongKhoa],
      chuyenNganh: [this.profileData.chuyenNganh]
    });

    this.passwordForm = this.fb.group({
      oldPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmNewPassword')?.value
      ? null : { mismatch: true };
  }

  loadProfile() {
    this.loading = true;
    this.http.get<any>('http://localhost:5186/api/v1/users/profile').subscribe({
      next: (profile) => {
        // Load extra fields from local storage
        const localExt = this.loadLocalProfileExt();
        
        this.profileData = {
          ...profile,
          truongKhoa: localExt.truongKhoa || 'Đại học Bách Khoa Hà Nội',
          chuyenNganh: localExt.chuyenNganh || 'Khoa học máy tính',
          vaiTro: profile.vaiTro || 'Sinh viên'
        };

        this.profileForm.patchValue({
          hoTen: this.profileData.hoTen,
          soDienThoai: this.profileData.soDienThoai,
          ngaySinh: this.profileData.ngaySinh ? this.profileData.ngaySinh.split('T')[0] : null,
          gioiTinh: this.profileData.gioiTinh,
          truongKhoa: this.profileData.truongKhoa,
          chuyenNganh: this.profileData.chuyenNganh
        });
        this.loading = false;
      },
      error: (err) => {
        console.warn('Backend API offline, falling back to localStorage / Mock profile data.', err);
        // Load everything from local storage
        const savedProfile = localStorage.getItem('sh_profile_data');
        if (savedProfile) {
          this.profileData = JSON.parse(savedProfile);
        } else {
          // Initialize mock local storage
          this.saveProfileLocal(this.profileData);
        }
        
        this.profileForm.patchValue({
          hoTen: this.profileData.hoTen,
          soDienThoai: this.profileData.soDienThoai,
          ngaySinh: this.profileData.ngaySinh,
          gioiTinh: this.profileData.gioiTinh,
          truongKhoa: this.profileData.truongKhoa,
          chuyenNganh: this.profileData.chuyenNganh
        });
        this.loading = false;
      }
    });
  }

  loadStats() {
    this.http.get<UserStats>('http://localhost:5186/api/v1/users/statistics').subscribe({
      next: (data) => {
        this.stats = data;
      },
      error: (err) => {
        console.warn('Backend API statistics endpoint not reachable, loading mock values.', err);
      }
    });
  }

  loadSettings() {
    const saved = localStorage.getItem('sh_settings');
    if (saved) {
      this.settings = JSON.parse(saved);
    }
  }

  saveSettings() {
    localStorage.setItem('sh_settings', JSON.stringify(this.settings));
    this.successMessage = 'Lưu cài đặt tài khoản thành công.';
    this.errorMessage = '';
    setTimeout(() => this.successMessage = '', 3000);
  }

  changeTab(tab: string) {
    this.activeTab = tab;
    // Smooth scroll to the corresponding element
    const element = document.getElementById(tab);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  onUpdateProfile() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.successMessage = '';
    this.errorMessage = '';

    const extData = {
      truongKhoa: this.profileForm.value.truongKhoa,
      chuyenNganh: this.profileForm.value.chuyenNganh
    };
    this.saveLocalProfileExt(extData);

    const updateRequest = {
      hoTen: this.profileForm.value.hoTen,
      soDienThoai: this.profileForm.value.soDienThoai,
      ngaySinh: this.profileForm.value.ngaySinh ? new Date(this.profileForm.value.ngaySinh).toISOString() : null,
      gioiTinh: Number(this.profileForm.value.gioiTinh),
      diaChi: this.profileForm.value.truongKhoa // Map Trường/Khoa to Address field in backend DTO
    };

    this.http.put('http://localhost:5186/api/v1/users/profile', updateRequest).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.profileData = {
          ...this.profileData,
          ...res,
          ...extData
        };
        this.saveProfileLocal(this.profileData);
        this.successMessage = 'Cập nhật thông tin hồ sơ thành công.';
        setTimeout(() => this.successMessage = '', 3000);
        
        // Sync local auth service user
        const storedUser = localStorage.getItem('sh_user');
        if (storedUser) {
          const userObj = JSON.parse(storedUser);
          userObj.hoTen = res.hoTen;
          localStorage.setItem('sh_user', JSON.stringify(userObj));
        }
      },
      error: (err) => {
        console.warn('Backend API update failed, updating locally via localStorage.', err);
        this.loading = false;
        this.profileData = {
          ...this.profileData,
          hoTen: this.profileForm.value.hoTen,
          soDienThoai: this.profileForm.value.soDienThoai,
          ngaySinh: this.profileForm.value.ngaySinh,
          gioiTinh: Number(this.profileForm.value.gioiTinh),
          ...extData
        };
        this.saveProfileLocal(this.profileData);
        this.successMessage = 'Cập nhật thông tin hồ sơ (Local) thành công.';
        setTimeout(() => this.successMessage = '', 3000);
      }
    });
  }

  onChangePassword() {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.successMessage = '';
    this.errorMessage = '';

    const data = {
      oldPassword: this.passwordForm.value.oldPassword,
      newPassword: this.passwordForm.value.newPassword
    };

    this.http.put('http://localhost:5186/api/v1/users/change-password', data).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Đổi mật khẩu tài khoản thành công.';
        this.passwordForm.reset();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.title || 'Mật khẩu cũ không chính xác hoặc có lỗi xảy ra.';
        setTimeout(() => this.errorMessage = '', 5000);
      }
    });
  }

  onAvatarChange(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    this.uploading = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.http.post<any>('http://localhost:5186/api/v1/users/avatar', formData).subscribe({
      next: (res) => {
        this.uploading = false;
        this.profileData.anhDaiDien = res.avatarUrl;
        this.saveProfileLocal(this.profileData);
        this.successMessage = 'Cập nhật ảnh đại diện thành công.';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (err) => {
        this.uploading = false;
        console.warn('Backend API upload failed, loading local preview.', err);
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.profileData.anhDaiDien = e.target.result;
          this.saveProfileLocal(this.profileData);
        };
        reader.readAsDataURL(file);
        this.successMessage = 'Cập nhật ảnh đại diện (Preview) thành công.';
        setTimeout(() => this.successMessage = '', 3000);
      }
    });
  }

  // Local Storage helper functions for extra fields
  private loadLocalProfileExt(): any {
    const data = localStorage.getItem('sh_profile_ext');
    return data ? JSON.parse(data) : {};
  }

  private saveLocalProfileExt(data: any) {
    localStorage.setItem('sh_profile_ext', JSON.stringify(data));
  }

  private saveProfileLocal(data: any) {
    localStorage.setItem('sh_profile_data', JSON.stringify(data));
  }

  logout() {
    localStorage.removeItem('sh_token');
    localStorage.removeItem('sh_user');
    window.location.reload();
  }
}
