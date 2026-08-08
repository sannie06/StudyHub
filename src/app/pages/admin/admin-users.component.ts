import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdminService, SystemStats, UserManagement } from '../../services/admin.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './admin-users.component.html',
  styles: []
})
export class AdminUsersComponent implements OnInit {
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

  ngOnInit(): void {
    this.loadStats();
    this.loadUsers();
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
