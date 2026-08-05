import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PomodoroService, PomodoroSessionDto } from '../../services/pomodoro.service';
import { TaskService, TaskDto } from '../../services/task.service';

export interface PomodoroSession {
  id: number;
  type: 'pomodoro' | 'short_break' | 'long_break';
  name: string;
  duration: string;
  timeRange: string;
  icon: string;
  iconColor: string;
}

@Component({
  selector: 'app-pomodoro',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './pomodoro.component.html',
  styles: [`
    .timer-ring {
      transition: stroke-dashoffset 0.5s linear;
    }
  `]
})
export class PomodoroComponent implements OnInit, OnDestroy {
  activeMode: 'pomodoro' | 'short_break' | 'long_break' = 'pomodoro';

  // Timer State
  totalSeconds: number = 25 * 60; // 25 minutes default
  timeLeft: number = 25 * 60;
  isRunning: boolean = false;
  timerInterval: any = null;

  // Session API Tracking State
  activeSessionId: number | null = null;
  pauseCount: number = 0;
  pauseStartTime: number | null = null;
  totalPauseSeconds: number = 0;

  // Toggle Switches State
  toggleMuteNotifications: boolean = true;
  toggleBlockWebsites: boolean = true;
  toggleBackgroundSound: boolean = true;
  toggleFullscreen: boolean = false;

  // Tasks Integration
  allUnfinishedTasks: TaskDto[] = [];
  focusedTask: TaskDto | null = null;

  // Modal State
  isTasksModalOpen: boolean = false;
  modalSearchQuery: string = '';

  // Completed sessions history
  sessionHistory: PomodoroSession[] = [
    { id: 5, type: 'pomodoro', name: 'Pomodoro 4', duration: '25 phút', timeRange: '10:30 - 10:55', icon: '🍅', iconColor: 'text-red-500' },
    { id: 4, type: 'pomodoro', name: 'Pomodoro 3', duration: '25 phút', timeRange: '09:55 - 10:20', icon: '🍅', iconColor: 'text-red-500' },
    { id: 3, type: 'short_break', name: 'Short Break', duration: '5 phút', timeRange: '09:50 - 09:55', icon: '☕', iconColor: 'text-blue-500' },
    { id: 2, type: 'pomodoro', name: 'Pomodoro 2', duration: '25 phút', timeRange: '09:15 - 09:40', icon: '🍅', iconColor: 'text-red-500' },
    { id: 1, type: 'pomodoro', name: 'Pomodoro 1', duration: '25 phút', timeRange: '08:40 - 09:05', icon: '🍅', iconColor: 'text-red-500' },
  ];

  constructor(
    private pomodoroService: PomodoroService,
    private taskService: TaskService
  ) {}

  ngOnInit() {
    this.setMode('pomodoro');
    this.loadTasks();
    this.checkActiveSession();
  }

  ngOnDestroy() {
    this.stopTimer();
  }

  // Get all unfinished tasks excluding the focused task
  get otherTasks(): TaskDto[] {
    if (!this.focusedTask) return this.allUnfinishedTasks;
    return this.allUnfinishedTasks.filter(t => t.maCongViec !== this.focusedTask?.maCongViec);
  }

  // Get max 5 tasks for the main screen "Danh sách công việc khác"
  get displayOtherTasks(): TaskDto[] {
    return this.otherTasks.slice(0, 5);
  }

  // Get filtered tasks for Modal search (toàn bộ danh sách công việc chưa hoàn thành)
  get filteredModalTasks(): TaskDto[] {
    const pool = this.allUnfinishedTasks;
    const query = this.modalSearchQuery.trim().toLowerCase();
    if (!query) return pool;
    return pool.filter(t =>
      t.tieuDe.toLowerCase().includes(query) ||
      (t.tenMonHoc && t.tenMonHoc.toLowerCase().includes(query)) ||
      (t.moTa && t.moTa.toLowerCase().includes(query))
    );
  }

  loadTasks() {
    this.taskService.getTasks({ pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.items && res.items.length > 0) {
          // Filter only unfinished tasks (trangThai !== 3)
          const unfinished = res.items.filter(t => t.trangThai !== 3);
          if (unfinished.length >= 2) {
            this.allUnfinishedTasks = unfinished;
          } else {
            const mocks = this.getMockTasks();
            const existingTitles = new Set(unfinished.map(u => u.tieuDe));
            const extraMocks = mocks.filter(m => !existingTitles.has(m.tieuDe));
            this.allUnfinishedTasks = [...unfinished, ...extraMocks];
          }
        } else {
          this.allUnfinishedTasks = this.getMockTasks();
        }

        if (!this.focusedTask && this.allUnfinishedTasks.length > 0) {
          this.focusedTask = this.allUnfinishedTasks[0];
        }
      },
      error: (err) => {
        console.error('Error loading tasks for Pomodoro:', err);
        this.allUnfinishedTasks = this.getMockTasks();
        if (!this.focusedTask && this.allUnfinishedTasks.length > 0) {
          this.focusedTask = this.allUnfinishedTasks[0];
        }
      }
    });
  }

  private getMockTasks(): TaskDto[] {
    return [
      { maCongViec: 101, tieuDe: 'Làm báo cáo đồ án tốt nghiệp', tenMonHoc: 'Đồ án tốt nghiệp', moTa: 'Hoàn thiện chương 3: Thiết kế hệ thống và triển khai chức năng.', doUuTien: 2, trangThai: 1, hanHoanThanh: '2024-06-30', tiLeHoanThanh: 60, danhDauQuanTrong: true, danhDauYeuThich: false, maNguoiDung: 1 },
      { maCongViec: 102, tieuDe: 'Ôn tập môn Cơ sở dữ liệu', tenMonHoc: 'Cơ sở dữ liệu', moTa: 'Ôn lại các câu lệnh SQL nâng cao và tối ưu chỉ mục.', doUuTien: 1, trangThai: 0, hanHoanThanh: '2024-06-25', tiLeHoanThanh: 30, danhDauQuanTrong: false, danhDauYeuThich: false, maNguoiDung: 1 },
      { maCongViec: 103, tieuDe: 'Chuẩn bị bài thuyết trình Tiếng Anh', tenMonHoc: 'Tiếng Anh chuyên ngành', moTa: 'Làm slide 10 trang giới thiệu về kiến trúc Microservices.', doUuTien: 0, trangThai: 0, hanHoanThanh: '2024-06-28', tiLeHoanThanh: 10, danhDauQuanTrong: false, danhDauYeuThich: false, maNguoiDung: 1 },
      { maCongViec: 104, tieuDe: 'Luyện thuật toán LeetCode 5 bài', tenMonHoc: 'Cấu trúc dữ liệu', moTa: 'Giải các bài tập chủ đề Binary Tree & Dynamic Programming.', doUuTien: 2, trangThai: 1, hanHoanThanh: '2024-06-29', tiLeHoanThanh: 40, danhDauQuanTrong: false, danhDauYeuThich: false, maNguoiDung: 1 },
      { maCongViec: 105, tieuDe: 'Thiết kế giao diện Figma cho ứng dụng', tenMonHoc: 'Thiết kế UI/UX', moTa: 'Vẽ mockup các màn hình chính và thành phần UI Kit.', doUuTien: 1, trangThai: 0, hanHoanThanh: '2024-07-02', tiLeHoanThanh: 20, danhDauQuanTrong: false, danhDauYeuThich: false, maNguoiDung: 1 },
      { maCongViec: 106, tieuDe: 'Đọc tài liệu Spring Boot Security', tenMonHoc: 'Phát triển Web', moTa: 'Tìm hiểu JWT Authentication và OAuth2 integration.', doUuTien: 0, trangThai: 0, hanHoanThanh: '2024-07-05', tiLeHoanThanh: 0, danhDauQuanTrong: false, danhDauYeuThich: false, maNguoiDung: 1 },
      { maCongViec: 107, tieuDe: 'Viết Unit Test cho Auth Controller', tenMonHoc: 'Kiểm thử phần mềm', moTa: 'Tạo test case cho Login, Register, Refresh Token.', doUuTien: 1, trangThai: 0, hanHoanThanh: '2024-07-08', tiLeHoanThanh: 15, danhDauQuanTrong: false, danhDauYeuThich: false, maNguoiDung: 1 }
    ];
  }

  // Detail Modal State
  selectedDetailTask: TaskDto | null = null;
  isDetailModalOpen: boolean = false;

  selectTask(task: TaskDto) {
    this.focusedTask = task;
    this.isTasksModalOpen = false;
  }

  openTasksModal() {
    this.modalSearchQuery = '';
    this.isTasksModalOpen = true;
  }

  closeTasksModal() {
    this.isTasksModalOpen = false;
  }

  viewTaskDetail(task?: TaskDto | null) {
    const targetTask = task || this.focusedTask;
    if (!targetTask) return;
    this.selectedDetailTask = targetTask;
    this.isDetailModalOpen = true;
  }

  closeDetailModal() {
    this.isDetailModalOpen = false;
    this.selectedDetailTask = null;
  }

  selectFocusFromDetail() {
    if (this.selectedDetailTask) {
      this.focusedTask = this.selectedDetailTask;
    }
    this.closeDetailModal();
  }

  checkActiveSession() {
    this.pomodoroService.getActiveSession().subscribe({
      next: (session: PomodoroSessionDto) => {
        if (session) {
          this.activeSessionId = session.maSession;
          this.totalPauseSeconds = session.tongThoiGianTamDung || 0;
          this.pauseCount = session.soLanTamDung || 0;

          // Map loaiSession byte to mode
          if (session.loaiSession === 1) this.activeMode = 'short_break';
          else if (session.loaiSession === 2) this.activeMode = 'long_break';
          else this.activeMode = 'pomodoro';

          this.totalSeconds = session.thoiLuong * 60;

          // Calculate remaining time
          const startMs = new Date(session.thoiGianBatDau).getTime();
          const elapsedSec = Math.floor((Date.now() - startMs) / 1000) - this.totalPauseSeconds;
          const remainingSec = this.totalSeconds - elapsedSec;

          this.timeLeft = remainingSec > 0 ? remainingSec : 0;

          if (session.trangThai === 2) { // Running
            this.startTimer();
          }
        }
      },
      error: (err) => {
        // 404 means no active session, which is normal
        if (err.status !== 404) {
          console.error('Error checking active Pomodoro session:', err);
        }
      }
    });
  }

  setMode(mode: 'pomodoro' | 'short_break' | 'long_break') {
    this.activeMode = mode;
    this.stopTimer();
    if (mode === 'pomodoro') {
      this.totalSeconds = 25 * 60;
    } else if (mode === 'short_break') {
      this.totalSeconds = 5 * 60;
    } else if (mode === 'long_break') {
      this.totalSeconds = 15 * 60;
    }
    this.timeLeft = this.totalSeconds;
  }

  togglePlay() {
    if (this.isRunning) {
      this.pauseTimer();
    } else {
      this.startTimer();
    }
  }

  startTimer() {
    if (this.isRunning) return;

    // Accumulate pause time if resuming
    if (this.pauseStartTime) {
      const pauseDuration = Math.floor((Date.now() - this.pauseStartTime) / 1000);
      this.totalPauseSeconds += pauseDuration;
      this.pauseStartTime = null;
    }

    // Call API to start session if not started yet
    if (!this.activeSessionId) {
      const loaiSessionCode = this.activeMode === 'short_break' ? 1 : this.activeMode === 'long_break' ? 2 : 0;
      const minutesDuration = Math.round(this.totalSeconds / 60);

      this.pomodoroService.startSession({
        loaiSession: loaiSessionCode,
        thoiLuong: minutesDuration,
        tieuDe: this.focusedTask ? this.focusedTask.tieuDe : `Phiên ${this.activeMode}`,
        maCongViec: this.focusedTask ? this.focusedTask.maCongViec : undefined
      }).subscribe({
        next: (session: PomodoroSessionDto) => {
          this.activeSessionId = session.maSession;
        },
        error: (err) => console.error('Error starting Pomodoro session API:', err)
      });
    }

    this.isRunning = true;
    this.timerInterval = setInterval(() => {
      if (this.timeLeft > 0) {
        this.timeLeft--;
      } else {
        this.onTimerComplete();
      }
    }, 1000);
  }

  pauseTimer() {
    this.isRunning = false;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }

    this.pauseCount++;
    this.pauseStartTime = Date.now();

    if (this.activeSessionId) {
      this.pomodoroService.pauseSession(this.activeSessionId, {
        tongThoiGianTamDung: this.totalPauseSeconds
      }).subscribe({
        error: (err) => console.error('Error pausing Pomodoro session API:', err)
      });
    }
  }

  stopTimer() {
    this.isRunning = false;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  resetTimer() {
    if (this.activeSessionId) {
      this.pomodoroService.cancelSession(this.activeSessionId).subscribe({
        next: () => {
          this.activeSessionId = null;
        },
        error: (err) => console.error('Error canceling Pomodoro session API:', err)
      });
    }

    this.stopTimer();
    this.activeSessionId = null;
    this.pauseCount = 0;
    this.pauseStartTime = null;
    this.totalPauseSeconds = 0;
    this.timeLeft = this.totalSeconds;
  }

  onTimerComplete() {
    this.stopTimer();

    const now = new Date();
    const startTime = new Date(now.getTime() - (this.totalSeconds * 1000));
    const startStr = `${startTime.getHours().toString().padStart(2, '0')}:${startTime.getMinutes().toString().padStart(2, '0')}`;
    const endStr = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;

    if (this.activeSessionId) {
      this.pomodoroService.finishSession(this.activeSessionId, {
        tongThoiGianTamDung: this.totalPauseSeconds,
        soLanTamDung: this.pauseCount
      }).subscribe({
        next: () => {
          this.addHistoryItem(startStr, endStr);
          this.activeSessionId = null;
          this.pauseCount = 0;
          this.totalPauseSeconds = 0;
        },
        error: (err) => console.error('Error finishing Pomodoro session API:', err)
      });
    } else {
      this.addHistoryItem(startStr, endStr);
    }

    alert('🎉 Phiên tập trung đã hoàn thành! Hãy nghỉ giải lao nhé.');
  }

  private addHistoryItem(startStr: string, endStr: string) {
    const isPomo = this.activeMode === 'pomodoro';
    const isShort = this.activeMode === 'short_break';
    const minutes = Math.round(this.totalSeconds / 60);

    const newHistory: PomodoroSession = {
      id: Date.now(),
      type: this.activeMode,
      name: isPomo ? `Pomodoro ${this.sessionHistory.length + 1}` : isShort ? 'Short Break' : 'Long Break',
      duration: `${minutes} phút`,
      timeRange: `${startStr} - ${endStr}`,
      icon: isPomo ? '🍅' : '☕',
      iconColor: isPomo ? 'text-red-500' : 'text-blue-500'
    };

    this.sessionHistory.unshift(newHistory);
  }

  selectOtherTask(task: TaskDto) {
    this.selectTask(task);
  }

  get formattedTime(): string {
    const minutes = Math.floor(this.timeLeft / 60);
    const seconds = this.timeLeft % 60;
    const mStr = minutes < 10 ? '0' + minutes : '' + minutes;
    const sStr = seconds < 10 ? '0' + seconds : '' + seconds;
    return `${mStr}:${sStr}`;
  }

  // SVG Ring Progress offset calculation
  // Radius = 140, circumference = 2 * PI * 140 ≈ 879.64
  readonly circumference = 2 * Math.PI * 140;

  get strokeDashoffset(): number {
    const progress = (this.totalSeconds - this.timeLeft) / this.totalSeconds;
    return this.circumference * (1 - progress);
  }

  // Dial ticks for clock marks (12 hours / 60 mins ticks)
  get clockTicks() {
    const ticks = [];
    for (let i = 0; i < 60; i++) {
      const angle = (i * 6) * (Math.PI / 180);
      const isMajor = i % 5 === 0;
      const innerR = isMajor ? 122 : 126;
      const outerR = 130;
      const x1 = 150 + innerR * Math.sin(angle);
      const y1 = 150 - innerR * Math.cos(angle);
      const x2 = 150 + outerR * Math.sin(angle);
      const y2 = 150 - outerR * Math.cos(angle);
      ticks.push({ x1, y1, x2, y2, isMajor });
    }
    return ticks;
  }
}
