import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService, User } from '../../services/auth.service';
import { NotificationService, ThongBaoDto } from '../../services/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit, OnDestroy {
  userName = '';
  userRole = '';
  userAvatar = '';
  unreadCount = 0;

  activeToast: ThongBaoDto | null = null;
  private userSub?: Subscription;
  private notifySub?: Subscription;
  private toastSub?: Subscription;
  private toastTimeout: any = null;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.loadUserInfo();
    if (this.authService.token) {
      this.fetchProfile();
      this.notificationService.getUnreadCount().subscribe({ error: () => {} });
    }
    this.userSub = this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.updateUserDisplay(user);
      }
    });

    this.notifySub = this.notificationService.unreadCount$.subscribe(count => {
      this.unreadCount = count;
    });

    // Listen to live notifications for Toast floating popup
    this.toastSub = this.notificationService.latestNotification$.subscribe(notification => {
      if (notification) {
        this.showToast(notification);
      }
    });
  }

  showToast(notification: ThongBaoDto) {
    this.activeToast = notification;
    if (this.toastTimeout) clearTimeout(this.toastTimeout);
    this.toastTimeout = setTimeout(() => {
      this.activeToast = null;
    }, 5000);
  }

  ngOnDestroy() {
    this.userSub?.unsubscribe();
    this.notifySub?.unsubscribe();
    this.toastSub?.unsubscribe();
  }

  fetchProfile() {
    this.authService.getProfile().subscribe({
      next: (user) => {
        if (user) {
          this.updateUserDisplay(user);
        }
      },
      error: (err) => {
        console.warn('Header: Unable to fetch profile from API', err);
      }
    });
  }

  loadUserInfo() {
    const userStr = localStorage.getItem('sh_user');
    if (userStr) {
      try {
        const user: User = JSON.parse(userStr);
        this.updateUserDisplay(user);
        return;
      } catch (e) {}
    }

    const dataStr = localStorage.getItem('sh_profile_data');
    if (dataStr) {
      try {
        const data = JSON.parse(dataStr);
        this.userName = data.hoTen || '';
        this.userRole = data.vaiTro || 'Sinh viên';
        this.userAvatar = data.anhDaiDien || '';
      } catch (e) {}
    }
  }

  private updateUserDisplay(user: User) {
    this.userName = user.hoTen || '';
    if (user.anhDaiDien) {
      this.userAvatar = user.anhDaiDien;
    }
    const role = (user.vaiTro || '').toLowerCase();
    if (role.includes('admin') || role === '1' || role.includes('quản trị')) {
      this.userRole = 'Quản trị viên';
    } else if (user.vaiTro) {
      this.userRole = user.vaiTro;
    } else {
      this.userRole = 'Sinh viên';
    }
  }
}
