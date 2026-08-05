import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { DashboardService, DashboardData } from '../../services/dashboard.service';

export interface DashboardTask {
  id: number;
  title: string;
  subject: string;
  priority: 'Cao' | 'Trung bình' | 'Thấp';
  completed: boolean;
  status?: 'Cần thực hiện' | 'Đang thực hiện' | 'Hoàn thành';
  colorClass?: string;
}

export interface ClassSchedule {
  time: string;
  subject: string;
  room: string;
  colorClass: string;
  dotColor: string;
}

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './student-dashboard.component.html'
})
export class StudentDashboardComponent implements OnInit, OnDestroy {
  userName = 'Trần San San';
  userRole = 'Sinh viên';
  currentDate = '';

  loading: boolean = false;
  errorMessage: string = '';

  // Stats Card
  stats = {
    totalTasks: 15,
    completedTasks: 10,
    completionRate: 66.7,
    upcomingDeadlines: 3,
    studyHoursToday: 4.5
  };

  tasks: DashboardTask[] = [];
  schedule: ClassSchedule[] = [];

  performanceData = [
    { day: 'T2', hours: 3.0, h: 50 },
    { day: 'T3', hours: 4.5, h: 75 },
    { day: 'T4', hours: 5.0, h: 83 },
    { day: 'T5', hours: 6.0, h: 100 },
    { day: 'T6', hours: 4.5, h: 75 }
  ];

  groups: { name: string; unread: number; iconBg: string }[] = [];
  notifications: { text: string; time: string }[] = [];

  // Pomodoro timer state
  timerState: 'work' | 'shortBreak' | 'longBreak' = 'work';
  timeLeft: number = 25 * 60; // 25 mins in seconds
  totalWorkTime: number = 25 * 60;
  isRunning: boolean = false;
  isTimerRunning: boolean = false;
  pomoCompletedToday: number = 3;
  pomoGoal: number = 4;
  timerInterval: any = null;

  showAddTaskModal: boolean = false;
  newTaskTitle: string = '';
  newTaskSubject = 'Lập trình Java';
  newTaskPriority: 'Cao' | 'Trung bình' | 'Thấp' = 'Trung bình';

  private dashboardSub?: Subscription;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.formatCurrentDate();
    this.loadUserSession();
    this.loadDashboardData();
  }

  private loadUserSession() {
    const userStr = localStorage.getItem('sh_user');
    if (userStr) {
      try {
        const u = JSON.parse(userStr);
        if (u && u.hoTen) this.userName = u.hoTen;
        if (u && u.vaiTro) this.userRole = u.vaiTro === 'Admin' || u.vaiTro === '1' ? 'Quản trị viên' : 'Sinh viên';
      } catch (e) {}
    }
  }

  ngOnDestroy() {
    this.stopTimer();
    this.dashboardSub?.unsubscribe();
  }

  formatCurrentDate() {
    const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
    const now = new Date();
    const dayName = days[now.getDay()];
    const dateStr = `${now.getDate().toString().padStart(2, '0')}/${(now.getMonth() + 1).toString().padStart(2, '0')}/${now.getFullYear()}`;
    this.currentDate = `${dayName}, ${dateStr}`;
  }

  loadDashboardData() {
    this.loading = true;
    this.errorMessage = '';

    this.dashboardService.getDashboardData().subscribe({
      next: (data: DashboardData) => {
        this.loading = false;
        if (data) {
          if (data.userProfile) {
            this.userName = data.userProfile.hoTen || this.userName;
            this.userRole = data.userProfile.vaiTro || this.userRole;
          }

          if (data.statistics) {
            this.stats.totalTasks = data.statistics.tongSoCongViec || 0;
            this.stats.completedTasks = data.statistics.congViecHoanThanh || 0;
            this.stats.upcomingDeadlines = data.statistics.deadlineHomNay || 0;
            this.stats.completionRate = data.statistics.tongSoCongViec > 0
              ? Number(((data.statistics.congViecHoanThanh / data.statistics.tongSoCongViec) * 100).toFixed(1))
              : 0;
          }

          if (data.todayTasks && data.todayTasks.length > 0) {
            this.tasks = data.todayTasks.map(t => ({
              id: t.maCongViec,
              title: t.tieuDe,
              subject: t.tenMonHoc || 'Môn học',
              priority: t.doUuTien === 2 ? 'Cao' : t.doUuTien === 1 ? 'Trung bình' : 'Thấp',
              completed: t.trangThai === 3
            }));
          } else {
            this.loadFallbackTasks();
          }

          if (data.todayClassSchedules && data.todayClassSchedules.length > 0) {
            this.schedule = data.todayClassSchedules.map((s, idx) => {
              const start = new Date(s.ngayBatDau);
              const end = new Date(s.ngayKetThuc);
              const startStr = `${start.getHours().toString().padStart(2, '0')}:${start.getMinutes().toString().padStart(2, '0')}`;
              const endStr = `${end.getHours().toString().padStart(2, '0')}:${end.getMinutes().toString().padStart(2, '0')}`;

              const colors = [
                { colorClass: 'border-l-4 border-purple-500 bg-purple-50/40 text-purple-700', dotColor: 'bg-purple-500' },
                { colorClass: 'border-l-4 border-emerald-500 bg-emerald-50/40 text-emerald-700', dotColor: 'bg-emerald-500' },
                { colorClass: 'border-l-4 border-orange-500 bg-orange-50/40 text-orange-700', dotColor: 'bg-orange-500' }
              ];
              const c = colors[idx % colors.length];

              return {
                time: `${startStr} - ${endStr}`,
                subject: s.tenMonHoc,
                room: s.phongHoc || 'Phòng A101',
                colorClass: c.colorClass,
                dotColor: c.dotColor
              };
            });
          } else {
            this.loadFallbackSchedule();
          }

          if (data.weeklyProgress && data.weeklyProgress.length > 0) {
            this.performanceData = data.weeklyProgress.map(w => ({
              day: w.dayName,
              hours: w.completedCount,
              h: Math.min(100, Math.max(20, w.completedCount * 25))
            }));
          }

          if (data.recentStudyGroups && data.recentStudyGroups.length > 0) {
            const bgs = ['bg-purple-100 text-purple-600', 'bg-blue-100 text-blue-600', 'bg-emerald-100 text-emerald-600'];
            this.groups = data.recentStudyGroups.map((g, idx) => ({
              name: g.tenNhom,
              unread: g.soThanhVien,
              iconBg: bgs[idx % bgs.length]
            }));
          } else {
            this.loadFallbackGroups();
          }

          if (data.latestNotifications && data.latestNotifications.length > 0) {
            this.notifications = data.latestNotifications.map(n => ({
              text: n.tieuDe,
              time: 'Mới cập nhật'
            }));
          } else {
            this.loadFallbackNotifications();
          }
        }
      },
      error: (err) => {
        this.loading = false;
        console.error('Error loading Dashboard data from API:', err);
        if (err.status === 401) {
          this.errorMessage = 'Bạn cần đăng nhập để xem thông số Dashboard.';
        }
        this.loadFallbackTasks();
        this.loadFallbackSchedule();
        this.loadFallbackGroups();
        this.loadFallbackNotifications();
      }
    });
  }

  private loadFallbackTasks() {
    this.tasks = [
      { id: 1, title: 'Hoàn thành bài tập lớn Java', subject: 'Java Programming', priority: 'Cao', completed: false },
      { id: 2, title: 'Thiết kế sơ đồ ERD', subject: 'Cơ sở dữ liệu', priority: 'Cao', completed: false },
      { id: 3, title: 'Đọc tài liệu chương 3', subject: 'Lịch sử', priority: 'Trung bình', completed: true },
      { id: 4, title: 'Chuẩn bị slide báo cáo', subject: 'Thiết kế HTTT', priority: 'Thấp', completed: false }
    ];
  }

  private loadFallbackSchedule() {
    this.schedule = [
      { time: '08:00 - 10:00', subject: 'Java Programming', room: 'Phòng 402-A5', colorClass: 'border-l-4 border-purple-500 bg-purple-50/40 text-purple-700', dotColor: 'bg-purple-500' },
      { time: '10:15 - 12:15', subject: 'Cơ sở dữ liệu', room: 'Phòng 301-B1', colorClass: 'border-l-4 border-emerald-500 bg-emerald-50/40 text-emerald-700', dotColor: 'bg-emerald-500' },
      { time: '14:00 - 16:00', subject: 'Thiết kế HTTT', room: 'Phòng 205-C2', colorClass: 'border-l-4 border-orange-500 bg-orange-50/40 text-orange-700', dotColor: 'bg-orange-500' }
    ];
  }

  private loadFallbackGroups() {
    this.groups = [
      { name: 'Nhóm Đồ án tốt nghiệp', unread: 2, iconBg: 'bg-purple-100 text-purple-600' },
      { name: 'Nhóm Java Programming', unread: 5, iconBg: 'bg-blue-100 text-blue-600' },
      { name: 'Nhóm tự học Pomodoro', unread: 0, iconBg: 'bg-gray-100 text-gray-600' }
    ];
  }

  private loadFallbackNotifications() {
    this.notifications = [
      { text: 'Lịch học Java ngày mai thay đổi phòng sang 501-A5', time: '10 phút trước' },
      { text: 'Deadline báo cáo CSDL sắp hết hạn (12 giờ nữa)', time: '1 giờ trước' },
      { text: 'Bạn đã hoàn thành 90% mục tiêu học tập tuần này!', time: '3 giờ trước' }
    ];
  }

  updateStats() {
    const completed = this.tasks.filter(t => t.completed).length;
    this.stats.completedTasks = completed;
    this.stats.totalTasks = this.tasks.length;
    this.stats.completionRate = this.tasks.length > 0 ? Number(((completed / this.tasks.length) * 100).toFixed(1)) : 0;
  }

  toggleTask(task: DashboardTask) {
    task.completed = !task.completed;
    this.updateStats();
  }

  addTask() {
    if (!this.newTaskTitle.trim()) return;
    const newTask: DashboardTask = {
      id: Date.now(),
      title: this.newTaskTitle.trim(),
      subject: this.newTaskSubject,
      priority: this.newTaskPriority,
      completed: false
    };
    this.tasks.push(newTask);
    this.newTaskTitle = '';
    this.showAddTaskModal = false;
    this.updateStats();
  }

  deleteTask(id: number) {
    this.tasks = this.tasks.filter(t => t.id !== id);
    this.updateStats();
  }

  toggleTimer() {
    if (this.isTimerRunning) {
      this.stopTimer();
    } else {
      this.startTimer();
    }
  }

  startTimer() {
    this.isTimerRunning = true;
    this.timerInterval = setInterval(() => {
      if (this.timeLeft > 0) {
        this.timeLeft--;
      } else {
        this.stopTimer();
        this.timeLeft = 1500;
        this.pomoCompletedToday++;
      }
    }, 1000);
  }

  stopTimer() {
    this.isTimerRunning = false;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  resetTimer() {
    this.stopTimer();
    this.timeLeft = 1500;
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }
}
