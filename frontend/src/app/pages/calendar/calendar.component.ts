import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CalendarService, CalendarEventDto, CreateCalendarEventRequest } from '../../services/calendar.service';

export interface CalendarEvent {
  id: number;
  stringId?: string;
  title: string;
  room: string;
  time: string;
  dayIndex: number; // 0: T2, 1: T3, 2: T4, 3: T5, 4: T6, 5: T7, 6: CN
  startHour: number; // e.g. 8 for 08:00
  durationHours: number; // e.g. 2 for 2 hours
  colorClass: string;
  dotColor: string;
  type: string; // 'Lịch học' | 'Lịch thi' | 'Hoạt động'
  teacher?: string;
  note?: string;
  fullDate?: string;
}

export interface UpcomingSchedule {
  time: string;
  title: string;
  room: string;
  borderClass: string;
  dotClass: string;
}

export interface UpcomingExam {
  date: string;
  title: string;
  timeRoom: string;
  countdown: string;
  badgeClass: string;
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './calendar.component.html',
  styles: [`
    .donut-svg { transform: rotate(-90deg); }
    .timetable-grid {
      display: grid;
      grid-template-columns: 60px repeat(7, minmax(130px, 1fr));
    }
  `]
})
export class CalendarComponent implements OnInit {
  currentViewTab: 'week' | 'month' | 'day' = 'week';
  selectedSubjectFilter = 'all';
  selectedTypeFilter = 'all';
  selectedEvent: CalendarEvent | null = null;

  loading: boolean = false;
  errorMessage: string = '';

  weekDays = [
    { day: 'T2', date: '20/05', isToday: false },
    { day: 'T3', date: '21/05', isToday: false },
    { day: 'T4', date: '22/05', isToday: false },
    { day: 'T5', date: '23/05', isToday: true  },
    { day: 'T6', date: '24/05', isToday: false },
    { day: 'T7', date: '25/05', isToday: false },
    { day: 'CN', date: '26/05', isToday: false }
  ];

  timeSlots = [
    '06:00', '07:00', '08:00', '09:00', '10:00', '11:00',
    '12:00', '13:00', '14:00', '15:00', '16:00', '17:00',
    '18:00', '19:00'
  ];

  events: CalendarEvent[] = [];
  upcomingSchedules: UpcomingSchedule[] = [];
  upcomingExams: UpcomingExam[] = [];

  constructor(private calendarService: CalendarService) {}

  ngOnInit() {
    this.setupCurrentWeekDays();
    this.loadCalendarEvents();
  }

  setupCurrentWeekDays() {
    const now = new Date();
    const currentJsDay = now.getDay(); // 0 = Sun, 1 = Mon ...
    const distanceToMon = currentJsDay === 0 ? -6 : 1 - currentJsDay;
    const monday = new Date(now);
    monday.setDate(now.getDate() + distanceToMon);

    const daysLabel = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
    this.weekDays = daysLabel.map((label, idx) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + idx);
      const isToday = d.toDateString() === now.toDateString();
      const dateStr = `${d.getDate().toString().padStart(2, '0')}/${(d.getMonth() + 1).toString().padStart(2, '0')}`;
      return { day: label, date: dateStr, isToday };
    });
  }

  loadCalendarEvents() {
    this.loading = true;
    this.errorMessage = '';

    const start = new Date();
    start.setDate(start.getDate() - 30);
    const end = new Date();
    end.setDate(end.getDate() + 30);

    this.calendarService.getCalendarEvents(start.toISOString(), end.toISOString()).subscribe({
      next: (dtos: CalendarEventDto[]) => {
        this.loading = false;
        if (dtos && dtos.length > 0) {
          this.events = dtos.map(dto => this.mapDtoToEvent(dto));
          this.buildUpcomingSidebars(dtos);

          if (this.events.length > 0 && !this.selectedEvent) {
            this.selectedEvent = this.events[0];
          }
        } else {
          this.loadFallbackMockData();
        }
      },
      error: (err) => {
        this.loading = false;
        console.error('Error loading calendar events from API:', err);
        if (err.status === 401) {
          this.errorMessage = 'Bạn cần đăng nhập để xem lịch học.';
        }
        this.loadFallbackMockData();
      }
    });
  }

  private mapDtoToEvent(dto: CalendarEventDto): CalendarEvent {
    const startDate = new Date(dto.start);
    const endDate = new Date(dto.end);

    const jsDay = startDate.getDay();
    const dayIndex = jsDay === 0 ? 6 : jsDay - 1;

    const startHour = startDate.getHours() + (startDate.getMinutes() / 60);
    const durationMs = endDate.getTime() - startDate.getTime();
    const durationHours = durationMs > 0 ? durationMs / (1000 * 60 * 60) : 1.5;

    const startStr = `${startDate.getHours().toString().padStart(2, '0')}:${startDate.getMinutes().toString().padStart(2, '0')}`;
    const endStr = `${endDate.getHours().toString().padStart(2, '0')}:${endDate.getMinutes().toString().padStart(2, '0')}`;
    const time = `${startStr} - ${endStr}`;

    let typeStr = 'Hoạt động';
    let colorClass = 'bg-[#F3E8FF] border-purple-200 text-purple-900';
    let dotColor = dto.color || '#9333ea';

    if (dto.eventType === 'ClassSchedule') {
      typeStr = 'Lịch học';
      colorClass = 'bg-[#E0F2FE] border-sky-200 text-sky-900';
      dotColor = '#0284c7';
    } else if (dto.eventType === 'ExamSchedule') {
      typeStr = 'Lịch thi';
      colorClass = 'bg-[#FFEDD5] border-orange-200 text-orange-900';
      dotColor = '#ea580c';
    }

    const numericId = dto.sourceId > 0 ? dto.sourceId : (parseInt(dto.id.replace(/\D/g, '')) || Date.now());

    return {
      id: numericId,
      stringId: dto.id,
      title: dto.title,
      room: dto.location || 'Phòng A101',
      time,
      dayIndex,
      startHour,
      durationHours,
      colorClass,
      dotColor,
      type: typeStr,
      teacher: dto.description || 'Giảng viên bộ môn',
      note: dto.description || 'Không có ghi chú',
      fullDate: `${time}, ${startDate.toLocaleDateString('vi-VN')}`
    };
  }

  private buildUpcomingSidebars(dtos: CalendarEventDto[]) {
    this.upcomingSchedules = dtos
      .filter(d => d.eventType === 'ClassSchedule' || d.eventType === 'PersonalEvent')
      .slice(0, 3)
      .map(d => {
        const startDate = new Date(d.start);
        const endDate = new Date(d.end);
        const startStr = `${startDate.getHours().toString().padStart(2, '0')}:${startDate.getMinutes().toString().padStart(2, '0')}`;
        const endStr = `${endDate.getHours().toString().padStart(2, '0')}:${endDate.getMinutes().toString().padStart(2, '0')}`;
        return {
          time: `${startStr} - ${endStr}`,
          title: d.title,
          room: d.location || 'Phòng A101',
          borderClass: d.eventType === 'ClassSchedule' ? 'border-orange-500' : 'border-emerald-500',
          dotClass: d.eventType === 'ClassSchedule' ? 'bg-orange-500' : 'bg-emerald-500'
        };
      });

    this.upcomingExams = dtos
      .filter(d => d.eventType === 'ExamSchedule')
      .slice(0, 3)
      .map(d => {
        const startDate = new Date(d.start);
        const dateStr = `${startDate.getDate().toString().padStart(2, '0')}/${(startDate.getMonth() + 1).toString().padStart(2, '0')}/${startDate.getFullYear()}`;
        const diffDays = Math.ceil((startDate.getTime() - new Date().getTime()) / (1000 * 3600 * 24));
        const countdownStr = diffDays > 0 ? `${diffDays} ngày` : 'Sắp diễn ra';
        return {
          date: dateStr,
          title: d.title,
          timeRoom: `08:00 - 10:00 | ${d.location || 'Phòng A201'}`,
          countdown: countdownStr,
          badgeClass: 'bg-emerald-50 text-emerald-600'
        };
      });
  }

  private loadFallbackMockData() {
    this.events = [
      {
        id: 1, title: 'Cấu trúc dữ liệu', room: 'P.A101', time: '08:00 - 10:00',
        dayIndex: 0, startHour: 8, durationHours: 2,
        colorClass: 'bg-[#F3E8FF] border-purple-200 text-purple-900', dotColor: '#9333ea',
        type: 'Lịch học', teacher: 'Lê Hoàng Nam', note: 'Ôn lại con trỏ', fullDate: '08:00 - 10:00, Thứ 2, 20/05/2024'
      },
      {
        id: 2, title: 'Kỹ năng mềm', room: 'P.D401', time: '13:30 - 15:00',
        dayIndex: 0, startHour: 13.5, durationHours: 1.5,
        colorClass: 'bg-[#FCE7F3] border-pink-200 text-pink-900', dotColor: '#ec4899',
        type: 'Lịch học', teacher: 'Trần Thị Mỹ', note: 'Chuẩn bị bài thuyết trình', fullDate: '13:30 - 15:00, Thứ 2, 20/05/2024'
      },
      {
        id: 9, title: 'Lập trình Java', room: 'P.A101', time: '08:00 - 10:00',
        dayIndex: 3, startHour: 8, durationHours: 2,
        colorClass: 'bg-[#FFEDD5] border-orange-200 text-orange-900', dotColor: '#ea580c',
        type: 'Lịch học', teacher: 'Nguyễn Văn An', note: 'Mang theo laptop', fullDate: '08:00 - 10:00, Thứ 5, 23/05/2024'
      },
      {
        id: 10, title: 'Công nghệ phần mềm', room: 'P.A101', time: '13:30 - 15:30',
        dayIndex: 3, startHour: 13.5, durationHours: 2,
        colorClass: 'bg-[#DCFCE7] border-emerald-200 text-emerald-900', dotColor: '#16a34a',
        type: 'Lịch học', teacher: 'Vũ Thị Hải', note: 'Báo cáo tiến độ đồ án', fullDate: '13:30 - 15:30, Thứ 5, 23/05/2024'
      }
    ];

    this.upcomingSchedules = [
      { time: '08:00 - 10:00', title: 'Lập trình Java', room: 'P.A101', borderClass: 'border-orange-500', dotClass: 'bg-orange-500' },
      { time: '13:30 - 15:30', title: 'Công nghệ phần mềm', room: 'P.A101', borderClass: 'border-emerald-500', dotClass: 'bg-emerald-500' }
    ];

    this.upcomingExams = [
      { date: '29/05/2024', title: 'Cơ sở dữ liệu', timeRoom: '08:00 - 10:00 | Phòng thi A201', countdown: '6 ngày', badgeClass: 'bg-emerald-50 text-emerald-600' },
      { date: '03/06/2024', title: 'Cấu trúc dữ liệu', timeRoom: '13:30 - 15:30 | Phòng thi B101', countdown: '11 ngày', badgeClass: 'bg-amber-50 text-amber-600' }
    ];

    this.selectedEvent = this.events[2] || this.events[0];
  }

  selectEvent(event: CalendarEvent) {
    this.selectedEvent = event;
  }

  closeDetailCard() {
    this.selectedEvent = null;
  }

  promptCreateEvent() {
    const title = prompt('Nhập tên lịch học / sự kiện mới:');
    if (!title || !title.trim()) return;

    const location = prompt('Nhập phòng học / địa điểm (ví dụ: Phòng A101):', 'Phòng A101');

    const now = new Date();
    const startIso = now.toISOString();
    const endDate = new Date(now.getTime() + (2 * 60 * 60 * 1000));
    const endIso = endDate.toISOString();

    const req: CreateCalendarEventRequest = {
      tieuDe: title.trim(),
      moTa: 'Tạo từ Calendar Component',
      thoiGianBatDau: startIso,
      thoiGianKetThuc: endIso,
      diaDiem: location || 'Phòng A101',
      mauSac: '#4F46E5'
    };

    this.calendarService.createEvent(req).subscribe({
      next: () => {
        alert('✅ Đã tạo lịch học thành công!');
        this.loadCalendarEvents();
      },
      error: (err) => {
        console.error('Error creating event:', err);
        alert('Lỗi tạo lịch học: ' + (err?.error?.message || 'Vui lòng thử lại.'));
      }
    });
  }

  deleteCurrentEvent() {
    if (!this.selectedEvent) return;

    if (confirm(`Bạn có chắc chắn muốn xóa lịch "${this.selectedEvent.title}" không?`)) {
      const idToDelete = this.selectedEvent.id;
      this.calendarService.deleteEvent(idToDelete).subscribe({
        next: () => {
          this.events = this.events.filter(e => e.id !== idToDelete);
          this.selectedEvent = null;
          alert('🗑️ Đã xóa lịch thành công!');
        },
        error: (err) => {
          console.error('Error deleting event:', err);
          alert('Lỗi xóa lịch: ' + (err?.error?.message || 'Vui lòng thử lại.'));
        }
      });
    }
  }

  getEventStyle(event: CalendarEvent) {
    const startOffsetMinutes = (event.startHour - 6) * 60;
    const topPx = (startOffsetMinutes / 60) * 52;
    const heightPx = event.durationHours * 52 - 4;
    return {
      top: `${topPx}px`,
      height: `${heightPx}px`
    };
  }
}
