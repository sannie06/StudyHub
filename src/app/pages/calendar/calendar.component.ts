import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
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
  bgColor?: string;
  borderColor?: string;
  type: string; // 'Lịch học' | 'Lịch thi' | 'Hoạt động'
  teacher?: string;
  note?: string;
  fullDate?: string;
  startDateObj?: Date;
  maMonHoc?: number;
  tenMonHoc?: string;
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

export interface MonthDayCell {
  dateNum: number;
  dateObj: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  events: CalendarEvent[];
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
export class CalendarComponent implements OnInit, OnDestroy {
  currentViewTab: 'week' | 'month' | 'day' = 'week';
  selectedSubjectFilter = 'all';
  selectedTypeFilter = 'all';
  selectedEvent: CalendarEvent | null = null;

  isSubjectDropdownOpen = false;
  isTypeDropdownOpen = false;

  typeOptions = [
    { value: 'all', label: 'Tất cả loại lịch', badgeBg: 'bg-slate-100', badgeText: 'text-slate-700', color: '#64748B' },
    { value: 'ClassSchedule', label: 'Lịch học', badgeBg: 'bg-blue-100', badgeText: 'text-blue-700', color: '#3B82F6' },
    { value: 'ExamSchedule', label: 'Lịch thi', badgeBg: 'bg-purple-100', badgeText: 'text-purple-700', color: '#6366F1' },
    { value: 'PersonalEvent', label: 'Sự kiện / Hoạt động', badgeBg: 'bg-amber-100', badgeText: 'text-amber-700', color: '#F59E0B' }
  ];

  subjectTags = [
    { name: 'Cơ sở dữ liệu', color: '#3B82F6' },
    { name: 'Cấu trúc dữ liệu và giải thuật', color: '#F59E0B' },
    { name: 'Lập trình Web', color: '#10B981' },
    { name: 'Tiếng Anh 2', color: '#8B5CF6' },
    { name: 'PTPM', color: '#6366F1' },
    { name: 'Java', color: '#EC4899' },
    { name: 'Kỹ năng mềm', color: '#14B8A6' },
    { name: 'Công nghệ phần mềm', color: '#EF4444' }
  ];

  getTagBadgeStyle(colorHex?: string) {
    if (!colorHex) return { 'background-color': '#F1F5F9', 'color': '#475569' };
    return {
      'background-color': `${colorHex}18`,
      'color': colorHex,
      'border': `1px solid ${colorHex}30`
    };
  }

  getSelectedTypeOption() {
    return this.typeOptions.find(t => t.value === this.selectedTypeFilter) || this.typeOptions[0];
  }

  getSubjectColor(name: string): string {
    const found = this.subjectTags.find(s => s.name === name);
    return found ? found.color : '#64748B';
  }

  loading: boolean = false;
  errorMessage: string = '';

  baseDate: Date = new Date();
  currentWeekMonday!: Date;
  currentWeekSunday!: Date;
  weekRangeLabel: string = '';

  currentTimeStr: string = '';
  currentTimeTopWeek: number = 0;
  currentTimeTopDay: number = 0;
  private timeIntervalId: any;

  subjectOptions: string[] = [
    'Cơ sở dữ liệu',
    'Cấu trúc dữ liệu và giải thuật',
    'Lập trình Web',
    'Tiếng Anh 2',
    'PTPM',
    'Java',
    'Kỹ năng mềm',
    'Công nghệ phần mềm'
  ];

  weekDays = [
    { day: 'T2', date: '', isToday: false },
    { day: 'T3', date: '', isToday: false },
    { day: 'T4', date: '', isToday: false },
    { day: 'T5', date: '', isToday: false },
    { day: 'T6', date: '', isToday: false },
    { day: 'T7', date: '', isToday: false },
    { day: 'CN', date: '', isToday: false }
  ];

  timeSlots = [
    '00:00', '01:00', '02:00', '03:00', '04:00', '05:00',
    '06:00', '07:00', '08:00', '09:00', '10:00', '11:00',
    '12:00', '13:00', '14:00', '15:00', '16:00', '17:00',
    '18:00', '19:00', '20:00', '21:00', '22:00', '23:00'
  ];

  dayTimeSlots = [
    '00:00', '01:00', '02:00', '03:00', '04:00', '05:00',
    '06:00', '07:00', '08:00', '09:00', '10:00', '11:00',
    '12:00', '13:00', '14:00', '15:00', '16:00', '17:00',
    '18:00', '19:00', '20:00', '21:00', '22:00', '23:00'
  ];

  getDayHeaderLabel() {
    const d = new Date(this.baseDate);
    const dayNames = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
    const dayName = dayNames[d.getDay()];
    const dayNum = d.getDate();
    const pad = (n: number) => n.toString().padStart(2, '0');
    const monthYear = `Tháng ${d.getMonth() + 1}, ${d.getFullYear()}`;
    const fullDateStr = `${dayName}, ${pad(dayNum)}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`;
    return { dayName, dayNum, monthYear, fullDateStr };
  }

  targetSelectedId: string | null = null;

  events: CalendarEvent[] = [];
  upcomingSchedules: UpcomingSchedule[] = [];
  upcomingExams: UpcomingExam[] = [];

  constructor(
    private calendarService: CalendarService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['selectedId']) {
        this.targetSelectedId = String(params['selectedId']);
      }
    });
    this.updateCurrentTime();
    this.timeIntervalId = setInterval(() => {
      this.updateCurrentTime();
    }, 10000);
    this.setupCurrentWeekDays();
    this.loadCalendarEvents();
  }

  ngOnDestroy() {
    if (this.timeIntervalId) {
      clearInterval(this.timeIntervalId);
    }
  }

  updateCurrentTime() {
    const now = new Date();
    const hours = now.getHours();
    const minutes = now.getMinutes();

    const pad = (n: number) => n.toString().padStart(2, '0');
    this.currentTimeStr = `${pad(hours)}:${pad(minutes)}`;

    const totalHours = hours + (minutes / 60);
    this.currentTimeTopWeek = Math.round(totalHours * 52);
    this.currentTimeTopDay = Math.round(totalHours * 56);
  }

  monthDaysMatrix: MonthDayCell[] = [];
  monthHeaderDays = ['THỨ 2', 'THỨ 3', 'THỨ 4', 'THỨ 5', 'THỨ 6', 'THỨ 7', 'CHỦ NHẬT'];

  setupCurrentWeekDays() {
    const current = new Date(this.baseDate);
    const currentJsDay = current.getDay(); // 0 = Sun, 1 = Mon ...
    const distanceToMon = currentJsDay === 0 ? -6 : 1 - currentJsDay;

    this.currentWeekMonday = new Date(current);
    this.currentWeekMonday.setDate(current.getDate() + distanceToMon);
    this.currentWeekMonday.setHours(0, 0, 0, 0);

    this.currentWeekSunday = new Date(this.currentWeekMonday);
    this.currentWeekSunday.setDate(this.currentWeekMonday.getDate() + 6);
    this.currentWeekSunday.setHours(23, 59, 59, 999);

    const pad = (n: number) => n.toString().padStart(2, '0');
    const formatShort = (d: Date) => `${pad(d.getDate())}/${pad(d.getMonth() + 1)}`;
    const formatFull = (d: Date) => `${formatShort(d)}/${d.getFullYear()}`;

    if (this.currentViewTab === 'month') {
      this.weekRangeLabel = `Tháng ${this.baseDate.getMonth() + 1}, ${this.baseDate.getFullYear()}`;
    } else if (this.currentViewTab === 'day') {
      this.weekRangeLabel = this.getDayHeaderLabel().fullDateStr;
    } else {
      this.weekRangeLabel = `${formatFull(this.currentWeekMonday)} - ${formatFull(this.currentWeekSunday)}`;
    }

    const todayRealStr = new Date().toDateString();
    const daysLabel = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
    this.weekDays = daysLabel.map((label, idx) => {
      const d = new Date(this.currentWeekMonday);
      d.setDate(this.currentWeekMonday.getDate() + idx);
      const isToday = d.toDateString() === todayRealStr;
      return { day: label, date: formatShort(d), isToday };
    });

    this.setupMonthGrid();
  }

  get dayEvents(): CalendarEvent[] {
    const targetStr = this.baseDate.toDateString();
    return this.filteredEvents.filter(ev => {
      if (!ev.startDateObj) return false;
      return ev.startDateObj.toDateString() === targetStr;
    });
  }

  get dayExamCount(): number {
    return this.dayEvents.filter(e => e.type === 'Lịch thi').length;
  }

  get dayClassCount(): number {
    return this.dayEvents.filter(e => e.type === 'Lịch học').length;
  }

  get dayOtherCount(): number {
    return this.dayEvents.filter(e => e.type !== 'Lịch thi' && e.type !== 'Lịch học').length;
  }

  get dayTotalCount(): number {
    return this.dayEvents.length;
  }

  get dayExamPercent(): number {
    if (this.dayTotalCount === 0) return 0;
    return Math.round((this.dayExamCount / this.dayTotalCount) * 100);
  }

  get dayClassPercent(): number {
    if (this.dayTotalCount === 0) return 0;
    return Math.round((this.dayClassCount / this.dayTotalCount) * 100);
  }

  get dayOtherPercent(): number {
    if (this.dayTotalCount === 0) return 0;
    return Math.round((this.dayOtherCount / this.dayTotalCount) * 100);
  }

  setupMonthGrid() {
    const year = this.baseDate.getFullYear();
    const month = this.baseDate.getMonth(); // 0-indexed

    const firstDayOfMonth = new Date(year, month, 1);
    const jsDayFirst = firstDayOfMonth.getDay();
    const distanceToMon = jsDayFirst === 0 ? 6 : jsDayFirst - 1;

    const gridStartDate = new Date(firstDayOfMonth);
    gridStartDate.setDate(firstDayOfMonth.getDate() - distanceToMon);
    gridStartDate.setHours(0, 0, 0, 0);

    const todayRealStr = new Date().toDateString();
    const cells: MonthDayCell[] = [];

    for (let i = 0; i < 35; i++) {
      const d = new Date(gridStartDate);
      d.setDate(gridStartDate.getDate() + i);

      const isCurrentMonth = d.getMonth() === month;
      const isToday = d.toDateString() === todayRealStr;

      const dateStart = new Date(d); dateStart.setHours(0, 0, 0, 0);
      const dateEnd = new Date(d); dateEnd.setHours(23, 59, 59, 999);

      const cellEvents = (this.events || []).filter(ev => {
        if (!ev.startDateObj) return false;
        if (ev.type === 'TaskDeadline' || ev.title?.startsWith('Deadline:')) return false;

        // Apply Subject filter
        if (this.selectedSubjectFilter !== 'all') {
          const filterLower = this.selectedSubjectFilter.toLowerCase();
          const eventSubjectLower = (ev.tenMonHoc || '').toLowerCase();
          const eventTitleLower = (ev.title || '').toLowerCase();

          const matchesSubject = (eventSubjectLower && (eventSubjectLower === filterLower || eventSubjectLower.includes(filterLower))) ||
                                 (eventTitleLower && eventTitleLower.includes(filterLower)) ||
                                 (ev.maMonHoc !== undefined && String(ev.maMonHoc) === filterLower);

          if (!matchesSubject) return false;
        }

        // Apply Type filter
        if (this.selectedTypeFilter !== 'all') {
          if (this.selectedTypeFilter === 'ExamSchedule' && ev.type !== 'Lịch thi') return false;
          if (this.selectedTypeFilter === 'ClassSchedule' && ev.type !== 'Lịch học') return false;
          if (this.selectedTypeFilter === 'PersonalEvent' && ev.type !== 'Hoạt động' && ev.type !== 'Sắp tới') return false;
        }

        const t = ev.startDateObj.getTime();
        return t >= dateStart.getTime() && t <= dateEnd.getTime();
      });

      cells.push({
        dateNum: d.getDate(),
        dateObj: d,
        isCurrentMonth,
        isToday,
        events: cellEvents
      });
    }

    this.monthDaysMatrix = cells;
  }

  switchViewTab(tab: 'week' | 'month' | 'day') {
    this.currentViewTab = tab;
    this.setupCurrentWeekDays();
    this.loadCalendarEvents();
  }

  previousWeek() {
    if (this.currentViewTab === 'month') {
      this.baseDate.setMonth(this.baseDate.getMonth() - 1);
    } else if (this.currentViewTab === 'day') {
      this.baseDate.setDate(this.baseDate.getDate() - 1);
    } else {
      this.baseDate.setDate(this.baseDate.getDate() - 7);
    }
    this.setupCurrentWeekDays();
    this.loadCalendarEvents();
  }

  nextWeek() {
    if (this.currentViewTab === 'month') {
      this.baseDate.setMonth(this.baseDate.getMonth() + 1);
    } else if (this.currentViewTab === 'day') {
      this.baseDate.setDate(this.baseDate.getDate() + 1);
    } else {
      this.baseDate.setDate(this.baseDate.getDate() + 7);
    }
    this.setupCurrentWeekDays();
    this.loadCalendarEvents();
  }

  goToCurrentWeek() {
    this.baseDate = new Date();
    this.setupCurrentWeekDays();
    this.loadCalendarEvents();
  }

  openDatePicker(inputEl: HTMLInputElement) {
    if (!inputEl) return;
    try {
      if (typeof inputEl.showPicker === 'function') {
        inputEl.showPicker();
      } else {
        inputEl.focus();
        inputEl.click();
      }
    } catch (e) {
      console.warn('Could not open date picker:', e);
    }
  }

  onDateSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input || !input.value) return;

    const parts = input.value.split('-');
    if (parts.length === 3) {
      const year = parseInt(parts[0], 10);
      const month = parseInt(parts[1], 10) - 1;
      const day = parseInt(parts[2], 10);

      if (!isNaN(year) && !isNaN(month) && !isNaN(day)) {
        this.baseDate = new Date(year, month, day);
        this.setupCurrentWeekDays();
        this.loadCalendarEvents();
      }
    }
  }

  toggleSubjectDropdown(event: Event) {
    event.stopPropagation();
    this.isSubjectDropdownOpen = !this.isSubjectDropdownOpen;
    this.isTypeDropdownOpen = false;
  }

  toggleTypeDropdown(event: Event) {
    event.stopPropagation();
    this.isTypeDropdownOpen = !this.isTypeDropdownOpen;
    this.isSubjectDropdownOpen = false;
  }

  selectSubject(subj: string) {
    this.selectedSubjectFilter = subj;
    this.isSubjectDropdownOpen = false;
  }

  selectType(typeValue: string) {
    this.selectedTypeFilter = typeValue;
    this.isTypeDropdownOpen = false;
  }

  getTypeLabel(value: string): string {
    const found = this.typeOptions.find(t => t.value === value);
    return found ? found.label : 'Tất cả loại lịch';
  }

  resetFilters() {
    this.selectedSubjectFilter = 'all';
    this.selectedTypeFilter = 'all';
    this.isSubjectDropdownOpen = false;
    this.isTypeDropdownOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-filter-dropdown')) {
      this.isSubjectDropdownOpen = false;
      this.isTypeDropdownOpen = false;
    }
  }

  get filteredEvents(): CalendarEvent[] {
    if (!this.events || !Array.isArray(this.events)) return [];

    return this.events.filter(ev => {
      // 0. Exclude Task Deadlines from grid
      if (ev.type === 'TaskDeadline' || ev.title?.startsWith('Deadline:')) return false;

      // 1. Date Range Check based on active view mode
      if (ev.startDateObj) {
        const evTime = ev.startDateObj.getTime();
        if (this.currentViewTab === 'week') {
          if (this.currentWeekMonday && this.currentWeekSunday) {
            const monTime = this.currentWeekMonday.getTime();
            const sunTime = this.currentWeekSunday.getTime();
            if (evTime < monTime || evTime > sunTime) {
              return false;
            }
          }
        } else if (this.currentViewTab === 'month') {
          const year = this.baseDate.getFullYear();
          const month = this.baseDate.getMonth();
          const startMonth = new Date(year, month - 1, 20).getTime();
          const endMonth = new Date(year, month + 2, 10).getTime();
          if (evTime < startMonth || evTime > endMonth) {
            return false;
          }
        }
      }

      // 2. Subject filter
      if (this.selectedSubjectFilter !== 'all') {
        const filterLower = this.selectedSubjectFilter.toLowerCase();
        const eventSubjectLower = (ev.tenMonHoc || '').toLowerCase();
        const eventTitleLower = (ev.title || '').toLowerCase();

        const matchesSubject = (eventSubjectLower && (eventSubjectLower === filterLower || eventSubjectLower.includes(filterLower))) ||
                               (eventTitleLower && eventTitleLower.includes(filterLower)) ||
                               (ev.maMonHoc !== undefined && String(ev.maMonHoc) === filterLower);

        if (!matchesSubject) return false;
      }

      // 3. Schedule type filter
      if (this.selectedTypeFilter !== 'all') {
        if (this.selectedTypeFilter === 'ExamSchedule' && ev.type !== 'Lịch thi') return false;
        if (this.selectedTypeFilter === 'ClassSchedule' && ev.type !== 'Lịch học') return false;
        if (this.selectedTypeFilter === 'PersonalEvent' && ev.type !== 'Hoạt động' && ev.type !== 'Sắp tới') return false;
      }

      return true;
    });
  }

  loadCalendarEvents() {
    this.loading = true;
    this.errorMessage = '';

    // Load local storage events initially for instant UI response
    const localDtos = this.calendarService.getLocalEvents().filter(dto => dto.eventType !== 'TaskDeadline' && !dto.title?.startsWith('Deadline:'));
    this.events = localDtos.map(dto => this.mapDtoToEvent(dto));
    this.buildUpcomingSidebars(localDtos);
    this.updateSelectedEventState();
    this.setupMonthGrid();

    const start = new Date(this.baseDate);
    start.setDate(start.getDate() - 30);
    const end = new Date(this.baseDate);
    end.setDate(end.getDate() + 30);

    this.calendarService.getCalendarEvents(start.toISOString(), end.toISOString()).subscribe({
      next: (dtos: CalendarEventDto[]) => {
        this.loading = false;
        if (dtos && Array.isArray(dtos)) {
          const activeDtos = dtos.filter(dto => dto.eventType !== 'TaskDeadline' && !dto.title?.startsWith('Deadline:'));
          this.events = activeDtos.map(dto => this.mapDtoToEvent(dto));
          this.buildUpcomingSidebars(activeDtos);
          this.updateSelectedEventState();
          this.setupMonthGrid();
        }
      },
      error: (err) => {
        this.loading = false;
        console.warn('API connection offline, displaying local events:', err);
      }
    });
  }

  private mapDtoToEvent(dto: CalendarEventDto): CalendarEvent {
    const startDate = new Date(dto.start);
    const endDate = new Date(dto.end);

    const jsDay = startDate.getDay();
    const dayIndex = jsDay === 0 ? 6 : jsDay - 1;

    const startHour = startDate.getHours() + (startDate.getMinutes() / 60);

    // Calculate intra-day duration hours (same day time span)
    const sameDayEnd = new Date(startDate);
    sameDayEnd.setHours(endDate.getHours(), endDate.getMinutes(), endDate.getSeconds());
    let rawDurationMs = sameDayEnd.getTime() - startDate.getTime();

    if (rawDurationMs <= 0 || rawDurationMs > 24 * 3600 * 1000) {
      rawDurationMs = 2 * 3600 * 1000; // Default 2 hours fallback
    }

    const durationHours = Math.min(Math.max(rawDurationMs / (1000 * 60 * 60), 0.5), 12);

    const startStr = `${startDate.getHours().toString().padStart(2, '0')}:${startDate.getMinutes().toString().padStart(2, '0')}`;
    const endStr = `${endDate.getHours().toString().padStart(2, '0')}:${endDate.getMinutes().toString().padStart(2, '0')}`;
    const time = `${startStr} - ${endStr}`;

    let typeStr = 'Hoạt động';

    const rawColor = (dto.color && dto.color.startsWith('#')) ? dto.color : (
      dto.eventType === 'ClassSchedule' ? '#0284c7' :
      dto.eventType === 'ExamSchedule' ? '#ea580c' : '#8b5cf6'
    );

    const dotColor = rawColor;
    const bgColor = `${rawColor}18`;
    const borderColor = `${rawColor}40`;
    const colorClass = 'border';

    if (dto.eventType === 'ClassSchedule') {
      typeStr = 'Lịch học';
    } else if (dto.eventType === 'ExamSchedule') {
      typeStr = 'Lịch thi';
    }

    let teacher = dto.giangVien || 'Giảng viên bộ môn';
    let note = dto.description || 'Không có ghi chú';

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
      bgColor,
      borderColor,
      type: typeStr,
      teacher,
      note,
      fullDate: `${time}, ${startDate.toLocaleDateString('vi-VN')}`,
      startDateObj: startDate,
      maMonHoc: dto.maMonHoc,
      tenMonHoc: dto.tenMonHoc
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

  private updateSelectedEventState() {
    if (this.targetSelectedId) {
      const foundTarget = this.events.find(e => String(e.stringId) === this.targetSelectedId || String(e.id) === this.targetSelectedId);
      if (foundTarget) {
        this.selectedEvent = foundTarget;
        return;
      }
    }
    if (this.selectedEvent) {
      const currentId = String(this.selectedEvent.stringId || this.selectedEvent.id);
      const foundCurrent = this.events.find(e => String(e.stringId) === currentId || String(e.id) === currentId);
      if (foundCurrent) {
        this.selectedEvent = foundCurrent;
        return;
      }
    }
    if (this.events.length > 0) {
      this.selectedEvent = this.events[0];
    } else {
      this.selectedEvent = null;
    }
  }

  selectEvent(event: CalendarEvent, e?: MouseEvent) {
    if (e) {
      e.stopPropagation();
    }
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

  editCurrentEvent() {
    if (!this.selectedEvent) return;
    const targetId = this.selectedEvent.stringId ? String(this.selectedEvent.stringId) : String(this.selectedEvent.id);
    this.router.navigate(['/calendar/create'], { queryParams: { editId: targetId } });
  }

  deleteCurrentEvent() {
    if (!this.selectedEvent) return;

    const titleStr = this.selectedEvent.title;
    if (confirm(`Bạn có chắc chắn muốn xóa lịch "${titleStr}" này không?`)) {
      const idToDelete = this.selectedEvent.id;
      const eventType = this.selectedEvent.stringId?.split('_')[0] || (this.selectedEvent.type === 'Lịch học' ? 'ClassSchedule' : (this.selectedEvent.type === 'Lịch thi' ? 'ExamSchedule' : 'PersonalEvent'));
      this.calendarService.deleteEvent(idToDelete, eventType).subscribe({
        next: () => {
          this.calendarService.deleteEventLocal(idToDelete);
          this.events = this.events.filter(e => e.id !== idToDelete);
          this.selectedEvent = null;
          this.loadCalendarEvents();
        },
        error: (err) => {
          console.error('Error deleting event:', err);
          alert('Lỗi xóa lịch: ' + (err?.error?.message || 'Vui lòng thử lại.'));
        }
      });
    }
  }

  getEventStyle(event: CalendarEvent) {
    const isDayView = this.currentViewTab === 'day';
    const slotHeight = isDayView ? 56 : 52;
    const topPx = event.startHour * slotHeight;
    const maxDur = Math.min(event.durationHours, Math.max(24 - event.startHour, 0.5));
    const heightPx = Math.max(maxDur * slotHeight - 4, 36);
    const hex = event.dotColor || '#6366F1';

    return {
      'top': `${topPx}px`,
      'height': `${heightPx}px`,
      'background-color': `${hex}15`,
      'border-color': `${hex}40`,
      'color': '#1e293b'
    };
  }
}
