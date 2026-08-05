import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AnalyticsService, AnalyticsDto } from '../../services/analytics.service';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './analytics-dashboard.component.html',
  styles: [`
    .donut-svg { transform: rotate(-90deg); }
  `]
})
export class AnalyticsDashboardComponent implements OnInit {
  activeTab: 'day' | 'week' | 'month' = 'week';
  userName = 'Nguyễn Minh Anh';
  userRole = 'Sinh viên';

  loading: boolean = false;
  errorMessage: string = '';

  kpiCards = [
    { title: 'Tổng Task', value: '35', icon: 'pi-calendar-plus', bg: '#F3F0FF', iconColor: '#5B4DFF', valueColor: '#1a1a2e', sub: 'Cập nhật từ CSDL', subColor: '#5B4DFF' },
    { title: 'Hoàn thành', value: '28', icon: 'pi-check-circle', bg: '#E6F9F0', iconColor: '#10b981', valueColor: '#10b981', sub: 'Tỷ lệ cao', subColor: '#10b981' },
    { title: 'Quá hạn', value: '2', icon: 'pi-exclamation-circle', bg: '#FFEFEF', iconColor: '#ef4444', valueColor: '#ef4444', sub: 'Cần xử lý', subColor: '#ef4444' },
    { title: 'Hiệu suất học tập', value: '85%', icon: 'pi-chart-line', bg: '#EBF3FF', iconColor: '#3b82f6', valueColor: '#3b82f6', sub: 'Tiến độ tốt', subColor: '#3b82f6' },
    { title: 'Tổng giờ học', value: '52.4 giờ', icon: 'pi-clock', bg: '#FFF6E6', iconColor: '#f97316', valueColor: '#f97316', sub: 'Tổng tích lũy', subColor: '#f97316' },
  ];

  barData = [
    { day: 'T2', date: '26/05', h: 3.5, pct: 35, active: false },
    { day: 'T3', date: '27/05', h: 6.2, pct: 62, active: false },
    { day: 'T4', date: '28/05', h: 5.0, pct: 50, active: false },
    { day: 'T5', date: '29/05', h: 8.2, pct: 82, active: true },
    { day: 'T6', date: '30/05', h: 6.4, pct: 64, active: false },
    { day: 'T7', date: '31/05', h: 3.2, pct: 32, active: false },
    { day: 'CN', date: '01/06', h: 2.1, pct: 21, active: false },
  ];

  subjects = [
    { name: 'Java Programming', pct: 80, color: '#5B4DFF', dot: 'bg-[#5B4DFF]' },
    { name: 'Cơ sở dữ liệu', pct: 65, color: '#10b981', dot: 'bg-emerald-500' },
    { name: 'Thiết kế HTTT', pct: 90, color: '#f59e0b', dot: 'bg-amber-500' },
    { name: 'Cấu trúc dữ liệu', pct: 75, color: '#ef4444', dot: 'bg-rose-500' },
    { name: 'Các môn khác', pct: 60, color: '#94a3b8', dot: 'bg-slate-400' },
  ];

  deadlines = [
    { title: 'Báo cáo đồ án tốt nghiệp', sub: 'DATN Nhóm 1', due: '2 ngày nữa', date: '03/06/2024', priority: 'Cao', pClass: 'bg-red-100 text-red-600', iconBg: 'bg-red-100', iconColor: 'text-red-500' },
    { title: 'Bài tập lớn Java', sub: 'Java Programming', due: '1 ngày nữa', date: '02/06/2024', priority: 'Trung bình', pClass: 'bg-orange-100 text-orange-600', iconBg: 'bg-orange-100', iconColor: 'text-orange-500' },
    { title: 'Báo cáo CSDL', sub: 'Cơ sở dữ liệu', due: '4 ngày nữa', date: '05/06/2024', priority: 'Thấp', pClass: 'bg-emerald-100 text-emerald-600', iconBg: 'bg-emerald-100', iconColor: 'text-emerald-500' }
  ];

  pomoStats = [
    { label: 'Tổng phiên', value: '120 phiên', iconBg: 'bg-red-50', iconColor: 'text-red-500', icon: 'pi-circle-fill' },
    { label: 'Tổng thời gian', value: '52 giờ 24 phút', iconBg: 'bg-purple-50', iconColor: 'text-purple-600', icon: 'pi-clock' },
    { label: 'Thời gian tập trung TB', value: '26 phút / phiên', iconBg: 'bg-pink-50', iconColor: 'text-pink-500', icon: 'pi-sparkles' },
    { label: 'Tỉ lệ hoàn thành', value: '88%', iconBg: 'bg-emerald-50', iconColor: 'text-emerald-600', icon: 'pi-check-circle' },
  ];

  linePoints = [
    { x: 18, y: 72 },
    { x: 55, y: 52 },
    { x: 92, y: 66 },
    { x: 129, y: 20 },
    { x: 166, y: 42 },
    { x: 203, y: 76 },
    { x: 240, y: 68 },
  ];
  linePolyline = this.linePoints.map(p => `${p.x},${p.y}`).join(' ');
  lineDayLabels = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

  aiInsights = [
    { bg: '#E6F9F0', border: '#bbf7d0', icon: 'pi-arrow-up-right', iconBg: 'bg-emerald-100', iconColor: 'text-emerald-600', text: 'Hiệu suất học tập của bạn tăng 15% so với tuần trước. Hãy tiếp tục phát huy!' },
    { bg: '#FFF6E6', border: '#fed7aa', icon: 'pi-exclamation-triangle', iconBg: 'bg-amber-100', iconColor: 'text-amber-600', text: 'Môn Java Programming đang có nhiều task quá hạn. Bạn nên dành thêm thời gian ôn tập.' },
    { bg: '#F0F4FF', border: '#bfdbfe', icon: 'pi-clock', iconBg: 'bg-blue-100', iconColor: 'text-blue-600', text: 'Bạn thường tập trung tốt nhất vào khung giờ 19:00 - 22:00. Hãy ưu tiên học vào thời gian này.' },
  ];

  goals = [
    { title: 'Hoàn thành 10 task', val: '8/10', pct: 80, barColor: '#5B4DFF', iconBg: 'bg-purple-100', iconColor: 'text-purple-600', icon: 'pi-shopping-bag' },
    { title: 'Học tối thiểu 20 giờ', val: '18.5/20 giờ', pct: 92, barColor: '#10b981', iconBg: 'bg-emerald-100', iconColor: 'text-emerald-600', icon: 'pi-check-circle' },
    { title: 'Đạt hiệu suất 80%', val: '85/100%', pct: 85, barColor: '#f59e0b', iconBg: 'bg-amber-100', iconColor: 'text-amber-600', icon: 'pi-bookmark' },
    { title: 'Hoàn thành 5 phiên Pomodoro/ngày', val: '5/7 ngày', pct: 71, barColor: '#3b82f6', iconBg: 'bg-blue-100', iconColor: 'text-blue-600', icon: 'pi-file' },
  ];

  constructor(
    private router: Router,
    private analyticsService: AnalyticsService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit() {
    this.loadAnalyticsData();
  }

  loadAnalyticsData() {
    this.loading = true;
    this.errorMessage = '';

    this.analyticsService.getAnalytics().subscribe({
      next: (data: AnalyticsDto) => {
        this.loading = false;
        if (data) {
          this.kpiCards[0].value = data.totalTasks.toString();
          this.kpiCards[1].value = data.completedTasks.toString();
          this.kpiCards[2].value = data.overdueTasks.toString();
          this.kpiCards[3].value = `${Math.round(data.taskCompletionRate)}%`;

          const hours = (data.totalFocusMinutes / 60).toFixed(1);
          this.kpiCards[4].value = `${hours} giờ`;

          if (data.subjectProgress && data.subjectProgress.length > 0) {
            this.subjects = data.subjectProgress.map((s, idx) => {
              const dots = ['bg-[#5B4DFF]', 'bg-emerald-500', 'bg-amber-500', 'bg-rose-500', 'bg-slate-400'];
              return {
                name: s.tenMonHoc,
                pct: Math.round(s.progress),
                color: s.mauSac || '#5B4DFF',
                dot: dots[idx % dots.length]
              };
            });
          }

          if (data.weeklyActivity && data.weeklyActivity.length > 0) {
            this.barData = data.weeklyActivity.map((w, idx) => {
              const h = (w.focusMinutes / 60);
              return {
                day: w.dayName || `T${idx + 2}`,
                date: w.date ? new Date(w.date).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' }) : '',
                h: Number(h.toFixed(1)),
                pct: Math.min(100, Math.max(15, Math.round((w.focusMinutes / 300) * 100))),
                active: idx === 3
              };
            });
          }

          const hoursTotal = Math.floor(data.totalFocusMinutes / 60);
          const minsTotal = data.totalFocusMinutes % 60;
          this.pomoStats[0].value = `${data.totalPomodoros} phiên`;
          this.pomoStats[1].value = `${hoursTotal} giờ ${minsTotal} phút`;
          this.pomoStats[2].value = data.totalPomodoros > 0 ? `${Math.round(data.totalFocusMinutes / data.totalPomodoros)} phút / phiên` : '25 phút / phiên';
          this.pomoStats[3].value = `${Math.round(data.taskCompletionRate)}%`;
        }
      },
      error: (err) => {
        this.loading = false;
        console.error('Error loading Analytics data from API:', err);
        if (err.status === 401) {
          this.errorMessage = 'Bạn cần đăng nhập để xem thông số Analytics.';
        }
      }
    });
  }

  setActiveTab(t: 'day' | 'week' | 'month') { this.activeTab = t; }
}
