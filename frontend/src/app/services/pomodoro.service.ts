import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { DashboardService } from './dashboard.service';

export interface PomodoroSessionDto {
  maSession: number;
  maNguoiDung: number;
  maCongViec?: number;
  maMonHoc?: number;
  tieuDe: string;
  loaiSession: number; // 0: Focus, 1: Short Break, 2: Long Break
  thoiLuong: number;
  soLanTamDung: number;
  tongThoiGianTamDung: number;
  thoiGianBatDau: string;
  thoiGianKetThuc?: string;
  trangThai: number; // 0: Huy, 1: Hoan thanh, 2: Dang chay
}

@Injectable({
  providedIn: 'root'
})
export class PomodoroService {
  private apiUrl = 'http://localhost:5186/api/v1/pomodoro/sessions';
  private storageKey = 'studyhub_pomodoro_state';

  constructor(
    private http: HttpClient,
    private dashboardService: DashboardService
  ) {}

  startSession(request: { maCongViec?: number; maMonHoc?: number; tieuDe?: string; loaiSession: number; thoiLuong: number }): Observable<PomodoroSessionDto> {
    return this.http.post<PomodoroSessionDto>(this.apiUrl, request).pipe(
      tap(() => this.dashboardService.clearCache())
    );
  }

  pauseSession(id: number, request: { tongThoiGianTamDung: number }): Observable<PomodoroSessionDto> {
    return this.http.put<PomodoroSessionDto>(`${this.apiUrl}/${id}/pause`, request);
  }

  finishSession(id: number, request: { tongThoiGianTamDung: number; soLanTamDung: number }): Observable<PomodoroSessionDto> {
    return this.http.put<PomodoroSessionDto>(`${this.apiUrl}/${id}/finish`, request).pipe(
      tap(() => this.dashboardService.clearCache())
    );
  }

  cancelSession(id: number): Observable<PomodoroSessionDto> {
    return this.http.put<PomodoroSessionDto>(`${this.apiUrl}/${id}/cancel`, {}).pipe(
      tap(() => this.dashboardService.clearCache())
    );
  }

  getActiveSession(): Observable<PomodoroSessionDto> {
    return this.http.get<PomodoroSessionDto>(`${this.apiUrl}/active`);
  }

  // Local storage auto-save helpers for resilient Pomodoro sessions
  saveLocalState(state: any) {
    localStorage.setItem(this.storageKey, JSON.stringify(state));
  }

  getLocalState(): any | null {
    const raw = localStorage.getItem(this.storageKey);
    return raw ? JSON.parse(raw) : null;
  }

  clearLocalState() {
    localStorage.removeItem(this.storageKey);
  }
}
