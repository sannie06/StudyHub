import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdminService, GroupManagement, SystemStats } from '../../services/admin.service';

@Component({
  selector: 'app-admin-groups',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './admin-groups.component.html',
  styles: []
})
export class AdminGroupsComponent implements OnInit {
  stats: SystemStats | null = null;
  groups: GroupManagement[] = [];
  filteredGroups: GroupManagement[] = [];

  loadingStats = false;
  loadingGroups = false;
  successMessage = '';
  errorMessage = '';

  searchTerm = '';
  selectedStatusFilter: string = 'all';

  // Group Details Modal State
  selectedGroup: GroupManagement | null = null;
  showDetailsModal = false;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadStats();
    this.loadGroups();
  }

  loadStats(): void {
    this.loadingStats = true;
    this.adminService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.loadingStats = false;
      },
      error: (err) => {
        console.error('Error loading stats:', err);
        this.loadingStats = false;
      }
    });
  }

  loadGroups(): void {
    this.loadingGroups = true;
    this.adminService.getGroups().subscribe({
      next: (data) => {
        this.groups = data;
        this.applyFilters();
        this.loadingGroups = false;
      },
      error: (err) => {
        console.error('Error loading groups:', err);
        this.loadingGroups = false;
      }
    });
  }

  applyFilters(): void {
    let result = [...this.groups];

    if (this.searchTerm.trim()) {
      const q = this.searchTerm.trim().toLowerCase();
      result = result.filter(g =>
        g.tenNhom.toLowerCase().includes(q) ||
        (g.moTa && g.moTa.toLowerCase().includes(q)) ||
        g.tenNguoiTao.toLowerCase().includes(q) ||
        g.maThamGia.toLowerCase().includes(q)
      );
    }

    if (this.selectedStatusFilter !== 'all') {
      const statusVal = parseInt(this.selectedStatusFilter, 10);
      result = result.filter(g => g.trangThai === statusVal);
    }

    this.filteredGroups = result;
  }

  get totalMembersCount(): number {
    return this.groups.reduce((acc, g) => acc + g.soLuongThanhVien, 0);
  }

  get activeGroupsCount(): number {
    return this.groups.filter(g => g.trangThai === 1).length;
  }

  toggleGroupStatus(group: GroupManagement): void {
    const newStatus = group.trangThai === 1 ? 0 : 1;
    const actionName = newStatus === 0 ? 'Khóa/Giải tán' : 'Mở khóa';

    if (!confirm(`Bạn có chắc chắn muốn ${actionName} nhóm "${group.tenNhom}"?`)) {
      return;
    }

    this.adminService.toggleGroupStatus(group.maNhom, newStatus).subscribe({
      next: () => {
        group.trangThai = newStatus;
        this.successMessage = `Đã ${actionName.toLowerCase()} nhóm ${group.tenNhom} thành công.`;
        this.loadStats();
        setTimeout(() => this.successMessage = '', 4000);
      },
      error: () => {
        this.errorMessage = 'Cập nhật trạng thái nhóm thất bại.';
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  viewGroupDetails(group: GroupManagement): void {
    this.selectedGroup = group;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.selectedGroup = null;
    this.showDetailsModal = false;
  }
}
