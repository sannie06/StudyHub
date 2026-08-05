import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CalendarEventDto {
  id: string;
  sourceId: number;
  title: string;
  description: string;
  start: string;
  end: string;
  eventType: 'ClassSchedule' | 'ExamSchedule' | 'TaskDeadline' | 'PersonalEvent';
  color: string;
  location: string;
  reminderMinutes?: number;
  status: number;
  isEditable: boolean;
}

export interface CreateCalendarEventRequest {
  tieuDe: string;
  moTa?: string;
  thoiGianBatDau: string;
  thoiGianKetThuc: string;
  diaDiem?: string;
  mauSac?: string;
  nhacTruoc?: number;
}

export interface UpdateCalendarEventRequest {
  tieuDe: string;
  moTa?: string;
  thoiGianBatDau: string;
  thoiGianKetThuc: string;
  diaDiem?: string;
  mauSac?: string;
  nhacTruoc?: number;
  trangThai?: number;
}

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private apiUrl = 'http://localhost:5186/api/v1/calendar';

  constructor(private http: HttpClient) {}

  getCalendarEvents(start: string, end: string, types?: string[]): Observable<CalendarEventDto[]> {
    let url = `${this.apiUrl}?start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`;
    if (types && types.length > 0) {
      types.forEach(t => url += `&types=${encodeURIComponent(t)}`);
    }
    return this.http.get<CalendarEventDto[]>(url);
  }

  createEvent(request: CreateCalendarEventRequest): Observable<CalendarEventDto> {
    return this.http.post<CalendarEventDto>(`${this.apiUrl}/events`, request);
  }

  updateEvent(id: number, request: UpdateCalendarEventRequest): Observable<CalendarEventDto> {
    return this.http.put<CalendarEventDto>(`${this.apiUrl}/events/${id}`, request);
  }

  deleteEvent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${id}`);
  }
}
