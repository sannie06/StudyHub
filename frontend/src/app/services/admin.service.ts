import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MonthlyUserGrowth {
  monthLabel: string;
  newUsers: number;
  totalUsers: number;
}

export interface SystemStats {
  totalUsers: number;
  activeStudents: number;
  blockedUsers: number;
  totalTasks: number;
  totalStudyGroups: number;
  activeStudyGroups?: number;
  inactiveStudyGroups?: number;
  newStudyGroupsThisWeek?: number;
  totalDocuments: number;
  userGrowth?: MonthlyUserGrowth[];
  tasksCreatedToday?: number;
  pomodoroSessionsToday?: number;
  groupMessagesToday?: number;
  groupsCreatedToday?: number;
  totalAiUsage?: number;
  aiSummariesCount?: number;
  aiPlannerCount?: number;
  aiQnaCount?: number;
  recentUsers?: UserManagement[];
}

export interface UserManagement {
  maNguoiDung: number;
  hoTen: string;
  email: string;
  soDienThoai?: string;
  maVaiTro: number;
  tenVaiTro: string;
  trangThai: number; // 1: Active, 0: Blocked
  anhDaiDien?: string;
  ngayTao: string;
  lanDangNhapCuoi?: string;
}

export interface GroupMember {
  maNguoiDung: number;
  hoTen: string;
  email: string;
  anhDaiDien?: string;
  vaiTro: string;
  ngayThamGia: string;
}

export interface GroupManagement {
  maNhom: number;
  tenNhom: string;
  moTa?: string;
  anhDaiDien?: string;
  maThamGia: string;
  maNguoiTao: number;
  tenNguoiTao: string;
  emailNguoiTao: string;
  maMonHoc?: number;
  tenMonHoc?: string;
  soLuongThanhVien: number;
  soLuongToiDa: number;
  trangThai: number; // 1: Active, 0: Locked/Dissolved
  ngayTao: string;
  thanhVien: GroupMember[];
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = 'http://localhost:5186/api/v1/admin';

  constructor(private http: HttpClient) {}

  getStats(): Observable<SystemStats> {
    return this.http.get<SystemStats>(`${this.apiUrl}/stats`);
  }

  getUsers(search?: string, roleId?: number, status?: number): Observable<UserManagement[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (roleId) params = params.set('roleId', roleId.toString());
    if (status !== undefined && status !== null) params = params.set('status', status.toString());

    return this.http.get<UserManagement[]>(`${this.apiUrl}/users`, { params });
  }

  toggleStatus(userId: number, newStatus: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${userId}/status`, { trangThai: newStatus });
  }

  updateRole(userId: number, newRoleId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${userId}/role`, { maVaiTro: newRoleId });
  }

  getGroups(search?: string, status?: number): Observable<GroupManagement[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status !== undefined && status !== null) params = params.set('status', status.toString());

    return this.http.get<GroupManagement[]>(`${this.apiUrl}/groups`, { params });
  }

  toggleGroupStatus(groupId: number, newStatus: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/groups/${groupId}/status`, { trangThai: newStatus });
  }
}
