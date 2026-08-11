import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

export interface DashboardUserProfile {
  maNguoiDung: number;
  hoTen: string;
  email: string;
  avatar?: string;
  vaiTro: string;
}

export interface DashboardStatistics {
  tongSoMonHoc: number;
  tongSoCongViec: number;
  congViecHoanThanh: number;
  congViecChuaHoanThanh: number;
  deadlineHomNay: number;
  thongBaoChuaDoc: number;
}

export interface WeeklyProgress {
  dayName: string;
  completedCount: number;
  createdCount: number;
}

export interface DashboardTaskItem {
  maCongViec: number;
  tieuDe: string;
  doUuTien: number;
  trangThai: number;
  tiLeHoanThanh: number;
  hanHoanThanh?: string;
  tenMonHoc?: string;
}

export interface DashboardClassScheduleItem {
  maLichHoc: number;
  tenMonHoc: string;
  phongHoc: string;
  giangVien: string;
  ngayBatDau: string;
  ngayKetThuc: string;
  mauSac: string;
}

export interface DashboardExamScheduleItem {
  maLichThi: number;
  tenMonHoc: string;
  hinhThucThi: string;
  ngayThi: string;
  thoiLuong?: number;
  phongThi: string;
}

export interface DashboardStudyGroupItem {
  maNhom: number;
  tenNhom: string;
  moTa?: string;
  soThanhVien: number;
  anhBia?: string;
}

export interface DashboardDocumentItem {
  maTaiLieu: number;
  tenTaiLieu: string;
  tenMonHoc?: string;
  loaiFile: string;
  ngayTải: string;
}

export interface DashboardNotificationItem {
  maThongBao: number;
  tieuDe: string;
  noiDung: string;
  icon: string;
  ngayGui: string;
  daDoc: boolean;
}

export interface DashboardData {
  userProfile: DashboardUserProfile;
  statistics: DashboardStatistics;
  weeklyProgress: WeeklyProgress[];
  todayTasks: DashboardTaskItem[];
  upcomingDeadlines: DashboardTaskItem[];
  todayClassSchedules: DashboardClassScheduleItem[];
  nearestExamSchedules: DashboardExamScheduleItem[];
  recentStudyGroups: DashboardStudyGroupItem[];
  latestDocuments: DashboardDocumentItem[];
  latestNotifications: DashboardNotificationItem[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = 'http://localhost:5186/api/v1/dashboard';

  constructor(private http: HttpClient) {}

  getDashboardData(): Observable<DashboardData> {
    return this.http.get<DashboardData>(this.apiUrl);
  }

  clearCache() {
    // No-op for backward compatibility
  }
}
