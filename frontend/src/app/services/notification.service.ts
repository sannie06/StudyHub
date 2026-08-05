import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject, tap } from 'rxjs';
import { SignalRService } from './signalr.service';

export interface ThongBaoDto {
  maThongBao: number;
  maNguoiDung: number;
  maLoaiThongBao: number;
  tenLoaiThongBao: string;
  icon: string;
  mauSac: string;
  tieuDe: string;
  noiDung: string;
  duongDan: string;
  daDoc: boolean;
  mucDo: number; // 0: Low, 1: Medium, 2: High
  ngayGui: string;
  ngayDoc?: string;
}

export interface NotificationCountDto {
  unreadCount: number;
}

export interface CreateNotificationRequest {
  maNguoiDung: number;
  maLoaiThongBao?: number;
  tieuDe: string;
  noiDung: string;
  duongDan?: string;
  mucDo?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = 'http://localhost:5186/api/v1/notifications';
  public unreadCount$ = new BehaviorSubject<number>(0);
  public latestNotification$ = new Subject<ThongBaoDto>();

  constructor(private http: HttpClient, private signalRService: SignalRService) {
    this.signalRService.startConnection();
    this.listenToSignalRNotifications();
  }

  private listenToSignalRNotifications(): void {
    this.signalRService.notification$.subscribe((notification: ThongBaoDto) => {
      if (notification) {
        this.unreadCount$.next(this.unreadCount$.value + 1);
        this.latestNotification$.next(notification);
      }
    });
  }

  getMyNotifications(unreadOnly = false, page = 1, pageSize = 20): Observable<ThongBaoDto[]> {
    return this.http.get<ThongBaoDto[]>(`${this.apiUrl}?unreadOnly=${unreadOnly}&page=${page}&pageSize=${pageSize}`);
  }

  getUnreadCount(): Observable<NotificationCountDto> {
    return this.http.get<NotificationCountDto>(`${this.apiUrl}/unread-count`).pipe(
      tap(res => this.unreadCount$.next(res.unreadCount))
    );
  }

  markAsRead(id: number): Observable<ThongBaoDto> {
    return this.http.put<ThongBaoDto>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => {
        const current = this.unreadCount$.value;
        if (current > 0) this.unreadCount$.next(current - 1);
      })
    );
  }

  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => this.unreadCount$.next(0))
    );
  }

  deleteNotification(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
