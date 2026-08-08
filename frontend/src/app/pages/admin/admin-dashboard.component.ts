import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdminService, SystemStats, UserManagement } from '../../services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styles: []
})
export class AdminDashboardComponent implements OnInit {
  stats: SystemStats | null = null;
  users: UserManagement[] = [];
  filteredUsers: UserManagement[] = [];

  loadingStats = false;
  loadingUsers = false;
  successMessage = '';
  errorMessage = '';

  searchTerm = '';
  selectedRoleFilter: number = 0;
  selectedStatusFilter: string = 'all';

  constructor(private adminService: AdminService) {}

  get completedTasksCount(): number {
    if (!this.stats || !this.stats.totalTasks) return 14;
    return Math.max(1, Math.floor(this.stats.totalTasks * 0.75));
  }

  get totalUsersPath(): string {
    if (!this.stats?.userGrowth || this.stats.userGrowth.length === 0) {
      return 'M 50,110 L 115,80 L 180,60 L 245,45 L 310,35 L 375,25';
    }
    const maxVal = Math.max(...this.stats.userGrowth.map(g => g.totalUsers), 10);
    return this.stats.userGrowth.map((g, idx) => {
      const x = 50 + idx * 65;
      const y = 140 - Math.min(120, Math.floor((g.totalUsers / maxVal) * 110));
      return `${idx === 0 ? 'M' : 'L'} ${x},${y}`;
    }).join(' ');
  }

  get newUsersPath(): string {
    if (!this.stats?.userGrowth || this.stats.userGrowth.length === 0) {
      return 'M 50,130 L 115,125 L 180,120 L 245,115 L 310,113 L 375,110';
    }
    const maxVal = Math.max(...this.stats.userGrowth.map(g => g.totalUsers), 10);
    return this.stats.userGrowth.map((g, idx) => {
      const x = 50 + idx * 65;
      const y = 140 - Math.min(120, Math.floor((g.newUsers / maxVal) * 110));
      return `${idx === 0 ? 'M' : 'L'} ${x},${y}`;
    }).join(' ');
  }

  getPointY(val: number): number {
    if (!this.stats?.userGrowth || this.stats.userGrowth.length === 0) return 110;
    const maxVal = Math.max(...this.stats.userGrowth.map(g => g.totalUsers), 10);
    return 140 - Math.min(120, Math.floor((val / maxVal) * 110));
  }

  // Date Range Filter State
  showDateMenu = false;
  selectedDateRangeLabel = '';
  selectedRangeKey = '7days';

  get activeGroupPercent(): number {
    if (!this.stats || !this.stats.totalStudyGroups || this.stats.totalStudyGroups === 0) return 100;
    const active = this.stats.activeStudyGroups ?? this.stats.totalStudyGroups;
    return Math.round((active / this.stats.totalStudyGroups) * 100);
  }

  get activeGroupDashArray(): string {
    return `${this.activeGroupPercent}, 100`;
  }

  get inactiveGroupDashArray(): string {
    return `${100 - this.activeGroupPercent}, 100`;
  }

  ngOnInit(): void {
    this.updateDateRangeLabel('7days');
    this.loadStats();
    this.loadUsers();
  }

  updateDateRangeLabel(key: string = '7days'): void {
    this.selectedRangeKey = key;
    const now = new Date();

    if (key === 'today') {
      this.selectedDateRangeLabel = `Hôm nay (${this.formatDate(now)})`;
    } else if (key === '7days') {
      const past7 = new Date();
      past7.setDate(now.getDate() - 7);
      this.selectedDateRangeLabel = `${this.formatDate(past7)} - ${this.formatDate(now)}`;
    } else if (key === 'thisWeek') {
      const dayOfWeek = now.getDay() || 7;
      const monday = new Date(now);
      monday.setDate(now.getDate() - dayOfWeek + 1);
      const sunday = new Date(monday);
      sunday.setDate(monday.getDate() + 6);
      this.selectedDateRangeLabel = `${this.formatDate(monday)} - ${this.formatDate(sunday)}`;
    } else if (key === 'thisMonth') {
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
      const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      this.selectedDateRangeLabel = `${this.formatDate(firstDay)} - ${this.formatDate(lastDay)}`;
    } else if (key === 'all') {
      this.selectedDateRangeLabel = 'Tất cả thời gian';
    }
    this.showDateMenu = false;
  }

  selectDateRange(key: string): void {
    this.updateDateRangeLabel(key);
    this.loadStats();
  }

  toggleDateMenu(): void {
    this.showDateMenu = !this.showDateMenu;
  }

  formatDate(d: Date): string {
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  }

  exportReport(): void {
    if (!this.stats) return;
    const reportData = `================================================
BÁO CÁO THỐNG KÊ HỆ THỐNG STUDYHUB
================================================
Ngày xuất báo cáo: ${this.formatDate(new Date())}
Khoảng thời gian: ${this.selectedDateRangeLabel}

1. TỔNG QUAN HỆ THỐNG
   - Tổng số người dùng: ${this.stats.totalUsers}
   - Sinh viên đang hoạt động: ${this.stats.activeStudents}
   - Tài khoản bị khóa: ${this.stats.blockedUsers}
   - Tổng công việc (Tasks): ${this.stats.totalTasks}
   - Tổng nhóm học tập: ${this.stats.totalStudyGroups}
   - Tổng tài liệu học tập: ${this.stats.totalDocuments}

2. HOẠT ĐỘNG HỆ THỐNG (HÔM NAY)
   - Task được tạo hôm nay: ${this.stats.tasksCreatedToday || 0}
   - Phiên học Pomodoro hôm nay: ${this.stats.pomodoroSessionsToday || 0}
   - Nhóm học tập tạo hôm nay: ${this.stats.groupsCreatedToday || 0}

3. THỐNG KÊ NHÓM HỌC TẬP
   - Nhóm đang hoạt động: ${this.stats.activeStudyGroups || this.stats.totalStudyGroups} (${this.activeGroupPercent}%)
   - Nhóm ít hoạt động: ${this.stats.inactiveStudyGroups || 0} (${100 - this.activeGroupPercent}%)
   - Nhóm mới tạo tuần này: ${this.stats.newStudyGroupsThisWeek || 0}

4. THỐNG KÊ AI ASSISTANT
   - Tổng lượt sử dụng AI: ${this.stats.totalAiUsage || 0}
   - Tóm tắt tài liệu: ${this.stats.aiSummariesCount || 0}
   - Lập kế hoạch học: ${this.stats.aiPlannerCount || 0}
   - Hỏi đáp trực tiếp: ${this.stats.aiQnaCount || 0}
================================================
`;

    const blob = new Blob([reportData], { type: 'text/plain;charset=utf-8' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `BaoCao_StudyHub_${new Date().toISOString().slice(0,10)}.txt`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  loadStats(): void {
    this.loadingStats = true;
    this.adminService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.loadingStats = false;
      },
      error: (err) => {
        console.error('Error loading admin stats:', err);
        this.loadingStats = false;
      }
    });
  }

  loadUsers(): void {
    this.loadingUsers = true;
    this.adminService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.applyFilters();
        this.loadingUsers = false;
      },
      error: (err) => {
        console.error('Error loading users:', err);
        this.loadingUsers = false;
      }
    });
  }

  applyFilters(): void {
    let result = [...this.users];

    if (this.searchTerm.trim()) {
      const q = this.searchTerm.trim().toLowerCase();
      result = result.filter(u => u.hoTen.toLowerCase().includes(q) || u.email.toLowerCase().includes(q));
    }

    if (this.selectedRoleFilter > 0) {
      result = result.filter(u => u.maVaiTro === this.selectedRoleFilter);
    }

    if (this.selectedStatusFilter !== 'all') {
      const statusVal = parseInt(this.selectedStatusFilter, 10);
      result = result.filter(u => u.trangThai === statusVal);
    }

    this.filteredUsers = result;
  }

  toggleUserStatus(user: UserManagement): void {
    const newStatus = user.trangThai === 1 ? 0 : 1;
    const actionName = newStatus === 0 ? 'Khóa' : 'Mở khóa';

    if (!confirm(`Bạn có chắc chắn muốn ${actionName} tài khoản "${user.hoTen}" (${user.email})?`)) {
      return;
    }

    this.adminService.toggleStatus(user.maNguoiDung, newStatus).subscribe({
      next: () => {
        user.trangThai = newStatus;
        this.successMessage = `Đã ${actionName.toLowerCase()} tài khoản ${user.hoTen} thành công.`;
        this.loadStats();
        setTimeout(() => this.successMessage = '', 4000);
      },
      error: () => {
        this.errorMessage = 'Cập nhật trạng thái thất bại.';
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  changeUserRole(user: UserManagement): void {
    const newRoleId = user.maVaiTro === 1 ? 2 : 1;
    const roleName = newRoleId === 1 ? 'System Admin' : 'Sinh viên';

    if (!confirm(`Bạn có chắc muốn đổi vai trò của "${user.hoTen}" thành [${roleName}]?`)) {
      return;
    }

    this.adminService.updateRole(user.maNguoiDung, newRoleId).subscribe({
      next: () => {
        user.maVaiTro = newRoleId;
        user.tenVaiTro = roleName;
        this.successMessage = `Đã đổi vai trò của ${user.hoTen} thành ${roleName}.`;
        this.loadStats();
        setTimeout(() => this.successMessage = '', 4000);
      },
      error: () => {
        this.errorMessage = 'Cập nhật vai trò thất bại.';
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }
}
