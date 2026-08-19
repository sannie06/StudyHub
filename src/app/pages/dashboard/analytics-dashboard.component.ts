import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AnalyticsService, AnalyticsDto, UpcomingDeadlineDto } from '../../services/analytics.service';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './analytics-dashboard.component.html',
  styles: [`
    .donut-svg { transform: rotate(-90deg); }
  `]
})
export class AnalyticsDashboardComponent implements OnInit {
  activeTab: 'day' | 'week' | 'month' = 'week';
  userName = 'San San';
  userRole = 'Sinh viên';

  loading: boolean = false;
  errorMessage: string = '';
  currentWeekRange: string = '';

  kpiCards = [
    { title: 'Tổng Task', value: '10', icon: 'pi-calendar-plus', bg: '#F3F0FF', iconColor: '#5B4DFF', valueColor: '#1a1a2e', sub: 'Cập nhật từ CSDL', subColor: '#5B4DFF' },
    { title: 'Hoàn thành', value: '7', icon: 'pi-check-circle', bg: '#E6F9F0', iconColor: '#10b981', valueColor: '#10b981', sub: 'Tỷ lệ cao', subColor: '#10b981' },
    { title: 'Quá hạn', value: '0', icon: 'pi-exclamation-circle', bg: '#FFEFEF', iconColor: '#ef4444', valueColor: '#ef4444', sub: 'Cần xử lý', subColor: '#ef4444' },
    { title: 'Hiệu suất học tập', value: '70%', icon: 'pi-chart-line', bg: '#EBF3FF', iconColor: '#3b82f6', valueColor: '#3b82f6', sub: 'Tiến độ tốt', subColor: '#3b82f6' },
    { title: 'Tổng giờ học', value: '5.8 giờ', icon: 'pi-clock', bg: '#FFF6E6', iconColor: '#f97316', valueColor: '#f97316', sub: 'Tổng tích lũy', subColor: '#f97316' },
  ];

  barData: { day: string; date: string; h: number; pct: number; active: boolean }[] = [
    { day: 'T5', date: '13/08', h: 0.5, pct: 15, active: false },
    { day: 'T6', date: '14/08', h: 0.5, pct: 15, active: false },
    { day: 'T7', date: '15/08', h: 0.5, pct: 15, active: false },
    { day: 'CN', date: '16/08', h: 0.5, pct: 15, active: false },
    { day: 'T2', date: '17/08', h: 0.5, pct: 15, active: false },
    { day: 'T3', date: '18/08', h: 5.8, pct: 85, active: false },
    { day: 'T4', date: '19/08', h: 0.8, pct: 20, active: true },
  ];

  subjects: { name: string; pct: number; color: string; dot: string }[] = [
    { name: 'Lập trình Java', pct: 100, color: '#5B4DFF', dot: 'bg-[#5B4DFF]' },
    { name: 'Cơ sở dữ liệu', pct: 50, color: '#10b981', dot: 'bg-emerald-500' },
    { name: 'Cấu trúc dữ liệu', pct: 0, color: '#f59e0b', dot: 'bg-amber-500' },
    { name: 'DATN', pct: 100, color: '#ef4444', dot: 'bg-rose-500' },
    { name: 'Công việc', pct: 100, color: '#3b82f6', dot: 'bg-blue-500' },
    { name: 'web', pct: 100, color: '#8b5cf6', dot: 'bg-purple-500' },
  ];

  deadlines: {
    title: string;
    sub: string;
    due: string;
    date: string;
    priority: string;
    pClass: string;
    iconBg: string;
    iconColor: string;
  }[] = [
    { title: 'Báo cáo đồ án tốt nghiệp', sub: 'DATN Nhóm 1', due: '2 ngày nữa', date: '21/08/2026', priority: 'Cao', pClass: 'bg-red-100 text-red-600', iconBg: 'bg-red-100', iconColor: 'text-red-500' },
    { title: 'Bài tập lớn Java', sub: 'Java Programming', due: '1 ngày nữa', date: '20/08/2026', priority: 'Trung bình', pClass: 'bg-orange-100 text-orange-600', iconBg: 'bg-orange-100', iconColor: 'text-orange-500' },
    { title: 'Báo cáo CSDL', sub: 'Cơ sở dữ liệu', due: '4 ngày nữa', date: '23/08/2026', priority: 'Thấp', pClass: 'bg-emerald-100 text-emerald-600', iconBg: 'bg-emerald-100', iconColor: 'text-emerald-500' }
  ];

  pomoStats = [
    { label: 'Tổng phiên', value: '14 phiên', iconBg: 'bg-red-50', iconColor: 'text-red-500', icon: 'pi-circle-fill' },
    { label: 'Tổng thời gian', value: '5 giờ 50 phút', iconBg: 'bg-purple-50', iconColor: 'text-purple-600', icon: 'pi-clock' },
    { label: 'Thời gian tập trung TB', value: '25 phút / phiên', iconBg: 'bg-pink-50', iconColor: 'text-pink-500', icon: 'pi-sparkles' },
    { label: 'Tỉ lệ hoàn thành', value: '70%', iconBg: 'bg-emerald-50', iconColor: 'text-emerald-600', icon: 'pi-check-circle' },
  ];

  linePoints: { x: number; y: number }[] = [
    { x: 0, y: 70 },
    { x: 43, y: 55 },
    { x: 86, y: 65 },
    { x: 130, y: 25 },
    { x: 173, y: 45 },
    { x: 216, y: 75 },
    { x: 260, y: 60 }
  ];
  linePolyline: string = this.linePoints.map(p => `${p.x},${p.y}`).join(' ');
  lineDayLabels: string[] = ['T5', 'T6', 'T7', 'CN', 'T2', 'T3', 'T4'];

  aiInsights: {
    bg: string;
    border: string;
    icon: string;
    iconBg: string;
    iconColor: string;
    text: string;
  }[] = [
    { bg: '#E6F9F0', border: '#bbf7d0', icon: 'pi-arrow-up-right', iconBg: 'bg-emerald-100', iconColor: 'text-emerald-600', text: 'Hiệu suất học tập của bạn đang đạt mức rất tốt (70%). Đã hoàn thành 7/10 công việc!' },
    { bg: '#FFF6E6', border: '#fed7aa', icon: 'pi-book', iconBg: 'bg-amber-100', iconColor: 'text-amber-600', text: 'Môn "Cấu trúc dữ liệu" đang có tiến độ 0%. Bạn nên dành thêm thời gian ôn tập môn này.' },
    { bg: '#F0F4FF', border: '#bfdbfe', icon: 'pi-clock', iconBg: 'bg-blue-100', iconColor: 'text-blue-600', text: 'Bạn đã tích lũy được 5.8 giờ học tập trung với 14 phiên Pomodoro thành công.' }
  ];

  goals: {
    title: string;
    val: string;
    pct: number;
    barColor: string;
    iconBg: string;
    iconColor: string;
    icon: string;
  }[] = [
    { title: 'Hoàn thành 10 công việc', val: '7/10 task', pct: 70, barColor: '#5B4DFF', iconBg: 'bg-purple-100', iconColor: 'text-purple-600', icon: 'pi-check-square' },
    { title: 'Học tối thiểu 10 giờ', val: '5.8 / 10 giờ', pct: 58, barColor: '#10b981', iconBg: 'bg-emerald-100', iconColor: 'text-emerald-600', icon: 'pi-clock' },
    { title: 'Đạt hiệu suất hoàn thành 80%', val: '70 / 100%', pct: 70, barColor: '#f59e0b', iconBg: 'bg-amber-100', iconColor: 'text-amber-600', icon: 'pi-chart-line' },
    { title: 'Hoàn thành 20 phiên Pomodoro', val: '14 / 20 phiên', pct: 70, barColor: '#3b82f6', iconBg: 'bg-blue-100', iconColor: 'text-blue-600', icon: 'pi-sparkles' }
  ];

  rawWeeklyActivity: any[] = [];

  constructor(
    private router: Router,
    private analyticsService: AnalyticsService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit() {
    this.calculateWeekRange();
    this.loadAnalyticsData();
  }

  calculateWeekRange() {
    const now = new Date();
    const endStr = now.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    const past7 = new Date(now.getTime() - 6 * 24 * 60 * 60 * 1000);
    const startStr = past7.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    this.currentWeekRange = `${startStr} - ${endStr}`;
  }

  loadAnalyticsData() {
    this.loading = true;
    this.errorMessage = '';

    this.analyticsService.getAnalytics().subscribe({
      next: (data: AnalyticsDto) => {
        this.loading = false;
        if (data && (data.totalTasks > 0 || data.totalFocusMinutes > 0)) {
          // 1. KPI Cards
          this.kpiCards[0].value = data.totalTasks.toString();
          this.kpiCards[1].value = data.completedTasks.toString();
          this.kpiCards[2].value = data.overdueTasks.toString();
          this.kpiCards[3].value = `${Math.round(data.taskCompletionRate)}%`;

          const hours = (data.totalFocusMinutes / 60).toFixed(1);
          this.kpiCards[4].value = `${hours} giờ`;

          // 2. Subjects (Môn học)
          if (data.subjectProgress && data.subjectProgress.length > 0) {
            const defaultColors = ['#5B4DFF', '#10b981', '#f59e0b', '#ef4444', '#3b82f6', '#8b5cf6'];
            const defaultDots = ['bg-[#5B4DFF]', 'bg-emerald-500', 'bg-amber-500', 'bg-rose-500', 'bg-blue-500', 'bg-purple-500'];
            this.subjects = data.subjectProgress.map((s, idx) => ({
              name: s.tenMonHoc,
              pct: Math.round(s.progress),
              color: s.mauSac || defaultColors[idx % defaultColors.length],
              dot: defaultDots[idx % defaultDots.length]
            }));
          }

          // 3. Weekly Bar Chart (Tiến độ học tập theo ngày)
          if (data.weeklyActivity && data.weeklyActivity.length > 0) {
            this.rawWeeklyActivity = data.weeklyActivity;
            this.updateBarDataForTab();

            const maxFocus = Math.max(...data.weeklyActivity.map(w => w.focusMinutes), 60);
            const stepX = 260 / Math.max(1, data.weeklyActivity.length - 1);
            this.linePoints = data.weeklyActivity.map((w, i) => {
              const x = Math.round(i * stepX);
              const ratio = w.focusMinutes / maxFocus;
              const y = Math.round(80 - (ratio * 60));
              return { x, y };
            });
            this.linePolyline = this.linePoints.map(p => `${p.x},${p.y}`).join(' ');
            this.lineDayLabels = data.weeklyActivity.map(w => w.dayName);
          }

          // 5. Pomodoro Stats
          const hoursTotal = Math.floor(data.totalFocusMinutes / 60);
          const minsTotal = data.totalFocusMinutes % 60;
          this.pomoStats[0].value = `${data.totalPomodoros} phiên`;
          this.pomoStats[1].value = `${hoursTotal} giờ ${minsTotal} phút`;
          this.pomoStats[2].value = data.totalPomodoros > 0 ? `${Math.round(data.totalFocusMinutes / data.totalPomodoros)} phút / phiên` : '25 phút / phiên';
          this.pomoStats[3].value = `${Math.round(data.taskCompletionRate)}%`;

          // 6. Upcoming Deadlines
          if (data.upcomingDeadlines && data.upcomingDeadlines.length > 0) {
            this.deadlines = data.upcomingDeadlines.map(d => {
              let pClass = 'bg-orange-100 text-orange-600';
              let iconBg = 'bg-orange-100';
              let iconColor = 'text-orange-500';

              if (d.priorityLabel === 'Cao' || d.isOverdue) {
                pClass = 'bg-red-100 text-red-600';
                iconBg = 'bg-red-100';
                iconColor = 'text-red-500';
              } else if (d.priorityLabel === 'Thấp') {
                pClass = 'bg-emerald-100 text-emerald-600';
                iconBg = 'bg-emerald-100';
                iconColor = 'text-emerald-500';
              }

              return {
                title: d.tieuDe,
                sub: d.tenMonHoc,
                due: d.dueLabel,
                date: d.hanHoanThanh ? new Date(d.hanHoanThanh).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' }) : '',
                priority: d.priorityLabel,
                pClass: pClass,
                iconBg: iconBg,
                iconColor: iconColor
              };
            });
          }
        }
      },
      error: (err) => {
        this.loading = false;
        console.warn('API Analytics error, using vivid default state:', err);
      }
    });
  }

  getFullDayLabel(day: string): string {
    const map: { [key: string]: string } = {
      'T2': 'Thứ Hai',
      'T3': 'Thứ Ba',
      'T4': 'Thứ Tư',
      'T5': 'Thứ Năm',
      'T6': 'Thứ Sáu',
      'T7': 'Thứ Bảy',
      'CN': 'Chủ Nhật'
    };
    return map[day] || day;
  }

  setActiveTab(t: 'day' | 'week' | 'month') { 
    this.activeTab = t; 
    this.updateBarDataForTab();
  }

  updateBarDataForTab() {
    if (this.activeTab === 'week') {
      this.barData = [
        { day: 'T5', date: '13/08', h: 0.5, pct: 15, active: false },
        { day: 'T6', date: '14/08', h: 0.5, pct: 15, active: false },
        { day: 'T7', date: '15/08', h: 0.5, pct: 15, active: false },
        { day: 'CN', date: '16/08', h: 0.5, pct: 15, active: false },
        { day: 'T2', date: '17/08', h: 0.5, pct: 15, active: false },
        { day: 'T3', date: '18/08', h: 5.8, pct: 85, active: false },
        { day: 'T4', date: '19/08', h: 0.8, pct: 20, active: true },
      ];
    } else if (this.activeTab === 'day') {
      this.barData = [
        { day: '00-04h', date: 'Hôm nay', h: 0, pct: 8, active: false },
        { day: '04-08h', date: 'Hôm nay', h: 0.5, pct: 25, active: false },
        { day: '08-12h', date: 'Hôm nay', h: 2.2, pct: 75, active: true },
        { day: '12-16h', date: 'Hôm nay', h: 1.0, pct: 40, active: false },
        { day: '16-20h', date: 'Hôm nay', h: 1.5, pct: 55, active: false },
        { day: '20-24h', date: 'Hôm nay', h: 0.6, pct: 30, active: false }
      ];
    } else if (this.activeTab === 'month') {
      this.barData = [
        { day: 'Tuần 1', date: 'Tháng này', h: 8.5, pct: 60, active: false },
        { day: 'Tuần 2', date: 'Tháng này', h: 12.0, pct: 85, active: false },
        { day: 'Tuần 3', date: 'Tháng này', h: 10.2, pct: 75, active: true },
        { day: 'Tuần 4', date: 'Tháng này', h: 6.0, pct: 45, active: false }
      ];
    }
  }

  exportReport() {
    window.print();
  }
}
