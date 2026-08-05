import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationService, ThongBaoDto } from '../../services/notification.service';

export interface NotificationItem {
  id: number;
  title: string;
  category: 'all' | 'task' | 'schedule' | 'group' | 'ai' | 'system';
  badge?: string;
  badgeClass?: string;
  content: string;
  timeAgo: string;
  isRead: boolean;
  icon: string;
  iconBg: string;
  iconColor: string;

  // Metadata for detail view
  taskName?: string;
  dueDate?: string;
  groupName?: string;
  priority?: string;
  priorityClass?: string;
  status?: string;
  statusClass?: string;
  fullMessage?: string;
  targetUrl?: string;
  actionButtonText?: string;
}

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './notifications.component.html',
  styles: [`
    .notify-list-scroll::-webkit-scrollbar { width: 4px; }
    .notify-list-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 9999px; }
  `]
})
export class NotificationsComponent implements OnInit, OnDestroy {
  selectedCategory: 'all' | 'task' | 'schedule' | 'group' | 'ai' | 'system' = 'all';
  selectedNotificationId: number = 0;

  loading: boolean = false;
  errorMessage: string = '';

  notifications: NotificationItem[] = [];
  private notifySub?: Subscription;

  constructor(
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
    this.notifySub = this.notificationService.latestNotification$.subscribe((dto: ThongBaoDto) => {
      const newItem = this.mapDtoToItem(dto);
      // Prepend live incoming notification
      this.notifications = [newItem, ...this.notifications.filter(n => n.id !== newItem.id)];
      if (this.selectedNotificationId === 0) {
        this.selectedNotificationId = newItem.id;
      }
    });
  }

  ngOnDestroy(): void {
    this.notifySub?.unsubscribe();
  }

  loadNotifications(): void {
    this.loading = true;
    this.errorMessage = '';

    this.notificationService.getMyNotifications(false, 1, 50).subscribe({
      next: (dtos: ThongBaoDto[]) => {
        this.loading = false;
        this.notifications = dtos.map(dto => this.mapDtoToItem(dto));

        if (this.notifications.length > 0 && this.selectedNotificationId === 0) {
          this.selectedNotificationId = this.notifications[0].id;
        }
      },
      error: (err) => {
        this.loading = false;
        console.error('Error loading notifications:', err);
        if (err?.status === 401) {
          this.errorMessage = 'Bạn cần đăng nhập để xem thông báo.';
        } else {
          this.errorMessage = err?.error?.message || 'Không thể tải danh sách thông báo.';
        }
      }
    });
  }

  private mapDtoToItem(dto: ThongBaoDto): NotificationItem {
    const catMap: Record<number, 'task' | 'schedule' | 'group' | 'ai' | 'system'> = {
      1: 'task',
      2: 'schedule',
      3: 'group',
      4: 'ai',
      5: 'system'
    };

    let category: 'all' | 'task' | 'schedule' | 'group' | 'ai' | 'system' = catMap[dto.maLoaiThongBao] || 'system';

    const tieuDeLower = (dto.tieuDe || '').toLowerCase();
    const noiDungLower = (dto.noiDung || '').toLowerCase();
    const tenLoai = (dto.tenLoaiThongBao || '').toLowerCase();

    // Distinct icon assignment matching the design
    let icon = 'pi-bell';
    let iconBg = 'bg-gray-100';
    let iconColor = 'text-gray-500';

    if (tieuDeLower.includes('deadline') || tieuDeLower.includes('báo cáo') || tieuDeLower.includes('hạn nộp')) {
      category = 'task';
      icon = 'pi-calendar';
      iconBg = 'bg-rose-50';
      iconColor = 'text-rose-500';
    } else if (tieuDeLower.includes('tài liệu') || noiDungLower.includes('tải lên file') || noiDungLower.includes('.docx')) {
      category = 'group';
      icon = 'pi-users';
      iconBg = 'bg-emerald-50';
      iconColor = 'text-emerald-500';
    } else if (tieuDeLower.includes('lịch học') || tieuDeLower.includes('diễn ra')) {
      category = 'schedule';
      icon = 'pi-book';
      iconBg = 'bg-blue-50';
      iconColor = 'text-blue-500';
    } else if (tieuDeLower.includes('ai assistant') || tenLoai.includes('ai') || tieuDeLower.includes('đề xuất')) {
      category = 'ai';
      icon = 'pi-android';
      iconBg = 'bg-amber-50';
      iconColor = 'text-amber-500';
    } else if (tieuDeLower.includes('quá hạn')) {
      category = 'task';
      icon = 'pi-history';
      iconBg = 'bg-purple-50';
      iconColor = 'text-[#5B4DFF]';
    } else if (tieuDeLower.includes('lịch thi') || tieuDeLower.includes('kỳ thi')) {
      category = 'schedule';
      icon = 'pi-calendar-plus';
      iconBg = 'bg-emerald-50';
      iconColor = 'text-emerald-600';
    } else if (tieuDeLower.includes('tham gia nhóm') || noiDungLower.includes('gia nhập')) {
      category = 'group';
      icon = 'pi-user-plus';
      iconBg = 'bg-rose-50';
      iconColor = 'text-rose-500';
    } else {
      category = catMap[dto.maLoaiThongBao] || 'system';
      if (category === 'task') {
        icon = 'pi-check-square';
        iconBg = 'bg-rose-50';
        iconColor = 'text-rose-500';
      } else if (category === 'schedule') {
        icon = 'pi-calendar';
        iconBg = 'bg-blue-50';
        iconColor = 'text-blue-500';
      } else if (category === 'group') {
        icon = 'pi-users';
        iconBg = 'bg-emerald-50';
        iconColor = 'text-emerald-500';
      } else if (category === 'ai') {
        icon = 'pi-android';
        iconBg = 'bg-amber-50';
        iconColor = 'text-amber-500';
      } else {
        icon = 'pi-bell';
        iconBg = 'bg-gray-100';
        iconColor = 'text-gray-500';
      }
    }

    let badge: string | undefined;
    let badgeClass: string | undefined;
    let priorityText = 'Bình thường';
    let priorityClass = 'font-bold text-gray-700';

    if (dto.mucDo === 2 || tieuDeLower.includes('deadline') || tieuDeLower.includes('quá hạn')) {
      badge = 'Cao';
      badgeClass = 'bg-rose-100 text-rose-600';
      priorityText = 'Cao';
      priorityClass = 'font-bold text-rose-600';
    } else if (dto.mucDo === 1) {
      badge = 'Ưu tiên';
      badgeClass = 'bg-amber-100 text-amber-600';
      priorityText = 'Ưu tiên';
      priorityClass = 'font-bold text-amber-600';
    }

    // Action button label based on category
    let actionButtonText = '→ Đi tới công việc';
    if (category === 'group') actionButtonText = '→ Đi tới nhóm';
    else if (category === 'schedule') actionButtonText = '→ Đi tới lịch học';
    else if (category === 'ai') actionButtonText = '→ Mở AI Assistant';
    else if (category === 'system') actionButtonText = '→ Xem chi tiết';

    const cleanTitle = this.stripEmoji(dto.tieuDe);
    const cleanContent = this.stripEmoji(dto.noiDung);

    // Extract task name if title has format "Title: TaskName"
    let taskName = cleanTitle;
    if (cleanTitle.includes(':')) {
      taskName = cleanTitle.split(':')[1].trim();
    }

    return {
      id: dto.maThongBao,
      title: cleanTitle,
      category,
      badge,
      badgeClass,
      content: cleanContent,
      timeAgo: this.formatTimeAgo(dto.ngayGui),
      isRead: dto.daDoc,
      icon,
      iconBg,
      iconColor,
      fullMessage: cleanContent,
      targetUrl: dto.duongDan,
      taskName: taskName,
      dueDate: '26/06/2024 (23:59)',
      groupName: 'DATN Nhóm 1',
      priority: priorityText,
      priorityClass: priorityClass,
      status: 'Đang thực hiện',
      statusClass: 'font-bold text-amber-600',
      actionButtonText: actionButtonText
    };
  }

  private stripEmoji(text: string | null | undefined): string {
    if (!text) return '';
    return text.replace(/[💭📝🗓⏰🔴⚠️🎉📚🏆📁💬👑💡📊⏱🔐🛠]/g, '').trim();
  }

  navigateToTarget(n?: NotificationItem): void {
    if (!n) return;
    const url = n.targetUrl && n.targetUrl.trim() !== '' ? n.targetUrl : '/groups';
    this.router.navigateByUrl(url);
  }

  private formatTimeAgo(dateStr: string): string {
    if (!dateStr) return 'Vừa xong';
    const diffMs = new Date().getTime() - new Date(dateStr).getTime();
    if (diffMs < 0) return 'Vừa xong';
    const diffMin = Math.floor(diffMs / (1000 * 60));
    if (diffMin < 1) return 'Vừa xong';
    if (diffMin < 60) return `${diffMin} phút trước`;
    const diffHours = Math.floor(diffMin / 60);
    if (diffHours < 24) return `${diffHours} giờ trước`;
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} ngày trước`;
  }

  get filteredNotifications(): NotificationItem[] {
    if (this.selectedCategory === 'all') return this.notifications;
    return this.notifications.filter(n => n.category === this.selectedCategory);
  }

  get selectedNotification(): NotificationItem | undefined {
    return this.notifications.find(n => n.id === this.selectedNotificationId);
  }

  selectNotification(id: number): void {
    this.selectedNotificationId = id;
    const found = this.notifications.find(n => n.id === id);
    if (found && !found.isRead) {
      found.isRead = true;
      this.notificationService.markAsRead(id).subscribe({
        error: (err) => console.error('Error marking as read:', err)
      });
    }
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
      },
      error: (err) => console.error('Error marking all as read:', err)
    });
  }

  toggleReadStatus(n: NotificationItem): void {
    if (!n.isRead) {
      n.isRead = true;
      this.notificationService.markAsRead(n.id).subscribe({
        error: (err) => console.error('Error marking as read:', err)
      });
    } else {
      n.isRead = false;
    }
  }

  deleteNotification(id: number): void {
    this.notificationService.deleteNotification(id).subscribe({
      next: () => {
        this.notifications = this.notifications.filter(n => n.id !== id);
        if (this.selectedNotificationId === id) {
          this.selectedNotificationId = this.notifications.length > 0 ? this.notifications[0].id : 0;
        }
      },
      error: (err) => console.error('Error deleting notification:', err)
    });
  }

  getCategoryCount(cat: string): number {
    if (cat === 'all') return this.notifications.length;
    return this.notifications.filter(n => n.category === cat).length;
  }
}
