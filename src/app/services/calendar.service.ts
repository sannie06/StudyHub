import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

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
  maMonHoc?: number;
  tenMonHoc?: string;
  giangVien?: string;
  hinhThucThi?: string;
}

export interface CreateCalendarEventRequest {
  tieuDe: string;
  moTa?: string;
  thoiGianBatDau: string;
  thoiGianKetThuc: string;
  diaDiem?: string;
  mauSac?: string;
  nhacTruoc?: number;
  eventType?: string;
  maMonHoc?: number;
  giangVien?: string;
  hinhThucThi?: string;
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
  eventType?: string;
  maMonHoc?: number;
  giangVien?: string;
  hinhThucThi?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private apiUrl = 'http://localhost:5186/api/v1/calendar';
  private storageKey = 'studyhub_calendar_events_v2';

  constructor(private http: HttpClient) {}

  // LocalStorage read helper
  getLocalEvents(): CalendarEventDto[] {
    try {
      const data = localStorage.getItem(this.storageKey);
      if (data) {
        return JSON.parse(data) as CalendarEventDto[];
      }
    } catch (e) {
      console.error('Error reading events from localStorage:', e);
    }
    return [];
  }

  saveLocalEvents(events: CalendarEventDto[]): void {
    try {
      localStorage.setItem(this.storageKey, JSON.stringify(events));
    } catch (e) {
      console.error('Error saving events to localStorage:', e);
    }
  }

  addEvent(req: CreateCalendarEventRequest): CalendarEventDto {
    const newDto: CalendarEventDto = {
      id: 'local_' + Date.now(),
      sourceId: Date.now(),
      title: req.tieuDe,
      description: req.moTa || '',
      start: req.thoiGianBatDau,
      end: req.thoiGianKetThuc,
      eventType: (req.eventType as any) || (req.tieuDe.toLowerCase().includes('thi') ? 'ExamSchedule' : 'ClassSchedule'),
      color: req.mauSac || '#6366F1',
      location: req.diaDiem || 'Phòng A101',
      status: 1,
      isEditable: true,
      maMonHoc: req.maMonHoc,
      giangVien: req.giangVien,
      hinhThucThi: req.hinhThucThi
    };

    const currentEvents = this.getLocalEvents();
    currentEvents.unshift(newDto);
    this.saveLocalEvents(currentEvents);

    return newDto;
  }

  deleteEventLocal(id: string | number): void {
    const idStr = String(id);
    const currentEvents = this.getLocalEvents().filter(e => e.id !== idStr && e.sourceId !== Number(id));
    this.saveLocalEvents(currentEvents);
  }

  updateEventLocal(id: string | number, req: CreateCalendarEventRequest): CalendarEventDto {
    const idStr = String(id);
    const numId = Number(id);
    const events = this.getLocalEvents();
    let targetIndex = events.findIndex(e => String(e.id) === idStr || (e.sourceId && e.sourceId === numId));

    if (targetIndex !== -1) {
      events[targetIndex].title = req.tieuDe;
      events[targetIndex].description = req.moTa || '';
      events[targetIndex].start = req.thoiGianBatDau;
      events[targetIndex].end = req.thoiGianKetThuc;
      events[targetIndex].location = req.diaDiem || 'Phòng A101';
      events[targetIndex].color = req.mauSac || '#6366F1';
      events[targetIndex].maMonHoc = req.maMonHoc;
      events[targetIndex].giangVien = req.giangVien;
      events[targetIndex].hinhThucThi = req.hinhThucThi;
      this.saveLocalEvents(events);
      return events[targetIndex];
    } else {
      const newDto: CalendarEventDto = {
        id: idStr,
        sourceId: numId || Date.now(),
        title: req.tieuDe,
        description: req.moTa || '',
        start: req.thoiGianBatDau,
        end: req.thoiGianKetThuc,
        eventType: (req.eventType as any) || (req.tieuDe.toLowerCase().includes('thi') ? 'ExamSchedule' : 'ClassSchedule'),
        color: req.mauSac || '#6366F1',
        location: req.diaDiem || 'Phòng A101',
        status: 1,
        isEditable: true,
        maMonHoc: req.maMonHoc,
        giangVien: req.giangVien,
        hinhThucThi: req.hinhThucThi
      };
      events.unshift(newDto);
      this.saveLocalEvents(events);
      return newDto;
    }
  }

  getCalendarEvents(start: string, end: string): Observable<CalendarEventDto[]> {
    return this.http.get<CalendarEventDto[]>(`${this.apiUrl}?start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`).pipe(
      tap(apiDtos => {
        if (apiDtos) {
          this.saveLocalEvents(apiDtos);
        }
      }),
      catchError(err => {
        console.warn('Backend API unreachable, using cached LocalStorage events:', err);
        return of(this.getLocalEvents());
      })
    );
  }

  createEvent(request: CreateCalendarEventRequest): Observable<CalendarEventDto> {
    return this.http.post<CalendarEventDto>(`${this.apiUrl}/events`, request);
  }

  updateEvent(id: number, request: UpdateCalendarEventRequest): Observable<CalendarEventDto> {
    return this.http.put<CalendarEventDto>(`${this.apiUrl}/events/${id}`, request);
  }

  deleteEvent(id: number, eventType?: string): Observable<void> {
    this.deleteEventLocal(id);
    const url = `${this.apiUrl}/events/${id}${eventType ? '?type=' + encodeURIComponent(eventType) : ''}`;
    return this.http.delete<void>(url);
  }
}
