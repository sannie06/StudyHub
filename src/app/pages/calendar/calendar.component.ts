import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { CalendarService, CalendarEventDto, CreateCalendarEventRequest } from '../../services/calendar.service';
import { TaskService, TaskDto } from '../../services/task.service';
import { SubjectService, SubjectTag } from '../../services/subject.service';

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

  subjectTags: SubjectTag[] = [];

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
    const found = this.subjectTags.find(s => s.name.toLowerCase() === name.toLowerCase());
    return found ? found.color : '#6366F1';
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
  allTasks: TaskDto[] = [];

  constructor(
    private calendarService: CalendarService,
    private taskService: TaskService,
    private subjectService: SubjectService,
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
    this.loadSubjects();
    this.loadCalendarEvents();
    this.loadTasks();
  }

  loadSubjects() {
    this.subjectService.getSubjectTags().subscribe({
      next: (tags) => {
        if (tags && tags.length > 0) {
          this.subjectTags = tags;
        } else {
          this.extractSubjectsFromEvents();
        }
      },
      error: (err) => {
        console.warn('Could not load subject tags from API, extracting from events:', err);
        this.extractSubjectsFromEvents();
      }
    });
  }

  extractSubjectsFromEvents() {
    const subjectMap = new Map<string, string>();
    (this.events || []).forEach(e => {
      const name = e.tenMonHoc || e.title;
      if (name && !subjectMap.has(name) && e.type !== 'Hoạt động') {
        subjectMap.set(name, e.dotColor || '#6366F1');
      }
    });
    if (subjectMap.size > 0) {
      this.subjectTags = Array.from(subjectMap.entries()).map(([name, color], idx) => ({
        id: idx + 1,
        name,
        color
      }));
    }
  }

  loadTasks() {
    this.taskService.getTasks({ pageSize: 100 }).subscribe({
      next: (res) => {
        if (res && res.items) {
          this.allTasks = res.items;
        }
      },
      error: (err) => console.warn('Could not load tasks for AI advisor:', err)
    });
  }

  // ═══════════════════════════════════════════════
  // SMART AI ADVISOR REAL-TIME CALCULATIONS
  // ═══════════════════════════════════════════════

  get realUpcomingExams(): CalendarEvent[] {
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    return (this.events || [])
      .filter(e => e.type === 'Lịch thi' && e.startDateObj && e.startDateObj.getTime() >= now.getTime())
      .sort((a, b) => (a.startDateObj?.getTime() || 0) - (b.startDateObj?.getTime() || 0));
  }

  get nearestExam(): CalendarEvent | null {
    return this.realUpcomingExams.length > 0 ? this.realUpcomingExams[0] : null;
  }

  get daysUntilNearestExam(): number {
    if (!this.nearestExam || !this.nearestExam.startDateObj) return 0;
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    const diffMs = this.nearestExam.startDateObj.getTime() - now.getTime();
    return Math.max(0, Math.ceil(diffMs / (1000 * 3600 * 24)));
  }

  get realUpcomingClasses(): CalendarEvent[] {
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    return (this.events || [])
      .filter(e => e.type === 'Lịch học' && e.startDateObj && e.startDateObj.getTime() >= now.getTime())
      .sort((a, b) => (a.startDateObj?.getTime() || 0) - (b.startDateObj?.getTime() || 0));
  }

  get nearestClass(): CalendarEvent | null {
    return this.realUpcomingClasses.length > 0 ? this.realUpcomingClasses[0] : null;
  }

  get targetSubjectName(): string {
    if (this.nearestExam) {
      return this.nearestExam.tenMonHoc || this.nearestExam.title;
    }
    if (this.nearestClass) {
      return this.nearestClass.tenMonHoc || this.nearestClass.title;
    }
    return '';
  }

  get aiAlertText(): string {
    if (this.currentViewTab === 'day') {
      return `Hôm nay bạn có ${this.dayTotalCount} lịch trình. Hãy chuẩn bị bài vở chu đáo nhé!`;
    }
    if (this.nearestExam) {
      const examCount = this.realUpcomingExams.length;
      const days = this.daysUntilNearestExam;
      const examName = this.nearestExam.tenMonHoc || this.nearestExam.title;
      if (days === 0) {
        return `Hôm nay bạn có lịch thi môn ${examName}. Chúc bạn làm bài thật tốt!`;
      }
      if (days <= 7) {
        return `Bạn có ${examCount} lịch thi trong ${days} ngày tới. Môn ${examName} cần ưu tiên ôn tập ngay!`;
      }
      return `Bạn có ${examCount} lịch thi sắp tới (Môn ${examName} còn ${days} ngày). Hãy lên kế hoạch ôn tập sớm!`;
    }
    if (this.realUpcomingClasses.length > 0) {
      const classCount = this.realUpcomingClasses.length;
      return `Sắp tới bạn có ${classCount} buổi học. Hãy chú ý đi học đúng giờ và xem trước bài giảng nhé!`;
    }
    return `Không có lịch thi hay lịch học gấp. Bạn có thể tự do ôn tập hoặc nghỉ ngơi thư giãn!`;
  }

  get smartTaskSuggestions(): TaskDto[] {
    const uncompleted = (this.allTasks || []).filter(t => t.trangThai !== 3);
    if (uncompleted.length === 0) return [];

    const sortTasks = (list: TaskDto[]) => {
      return [...list].sort((a, b) => {
        if (b.doUuTien !== a.doUuTien) {
          return (b.doUuTien || 0) - (a.doUuTien || 0);
        }
        if (b.danhDauQuanTrong !== a.danhDauQuanTrong) {
          return (b.danhDauQuanTrong ? 1 : 0) - (a.danhDauQuanTrong ? 1 : 0);
        }
        if (a.hanHoanThanh && b.hanHoanThanh) {
          return new Date(a.hanHoanThanh).getTime() - new Date(b.hanHoanThanh).getTime();
        }
        return a.hanHoanThanh ? -1 : 1;
      });
    };

    const targetSubjLower = (this.targetSubjectName || '').trim().toLowerCase();

    let targetSubjectTasks: TaskDto[] = [];
    let otherSubjectTasks: TaskDto[] = [];

    if (targetSubjLower) {
      targetSubjectTasks = uncompleted.filter(t => {
        const tSubj = (t.tenMonHoc || '').toLowerCase();
        const tTitle = (t.tieuDe || '').toLowerCase();
        return tSubj.includes(targetSubjLower) || targetSubjLower.includes(tSubj) || tTitle.includes(targetSubjLower);
      });
      otherSubjectTasks = uncompleted.filter(t => !targetSubjectTasks.includes(t));
    } else {
      otherSubjectTasks = [...uncompleted];
    }

    const sortedTargetTasks = sortTasks(targetSubjectTasks);
    const sortedOtherTasks = sortTasks(otherSubjectTasks);

    const result: TaskDto[] = [];
    if (sortedTargetTasks.length >= 2) {
      result.push(sortedTargetTasks[0], sortedTargetTasks[1]);
    } else if (sortedTargetTasks.length === 1) {
      result.push(sortedTargetTasks[0]);
      if (sortedOtherTasks.length > 0) {
        result.push(sortedOtherTasks[0]);
      }
    } else {
      if (sortedOtherTasks.length > 0) result.push(sortedOtherTasks[0]);
      if (sortedOtherTasks.length > 1) result.push(sortedOtherTasks[1]);
    }

    return result;
  }

  formatDateShort(d?: Date): string {
    if (!d) return '';
    const day = d.getDate().toString().padStart(2, '0');
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    return `${day}/${month}`;
  }

  formatTaskDeadline(hanHoanThanh?: string): string {
    if (!hanHoanThanh) return '';
    try {
      const d = new Date(hanHoanThanh);
      if (isNaN(d.getTime())) return '';
      const day = d.getDate().toString().padStart(2, '0');
      const month = (d.getMonth() + 1).toString().padStart(2, '0');
      return `Hạn: ${day}/${month}`;
    } catch {
      return '';
    }
  }

  getPriorityBadge(doUuTien: number): { label: string, bg: string, text: string } {
    switch (doUuTien) {
      case 3: return { label: 'Khẩn cấp', bg: 'bg-red-50', text: 'text-red-700' };
      case 2: return { label: 'Ưu tiên cao', bg: 'bg-rose-50', text: 'text-rose-700' };
      case 1: return { label: 'Trung bình', bg: 'bg-amber-50', text: 'text-amber-700' };
      default: return { label: 'Thấp', bg: 'bg-slate-50', text: 'text-slate-600' };
    }
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
        if (!this.matchesEventSubject(ev, this.selectedSubjectFilter)) {
          return false;
        }

        // Apply Type filter
        if (!this.matchesEventType(ev, this.selectedTypeFilter)) {
          return false;
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
    this.setupMonthGrid();
  }

  selectType(typeValue: string) {
    this.selectedTypeFilter = typeValue;
    this.isTypeDropdownOpen = false;
    this.setupMonthGrid();
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
    this.setupMonthGrid();
  }

  pinnedEvent: CalendarEvent | null = null;
  hoveredEvent: CalendarEvent | null = null;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-filter-dropdown')) {
      this.isSubjectDropdownOpen = false;
      this.isTypeDropdownOpen = false;
    }
    if (!target.closest('.calendar-event-card') && !target.closest('.event-popover-container')) {
      this.pinnedEvent = null;
      this.hoveredEvent = null;
      this.selectedEvent = null;
    }
  }

  matchesEventSubject(ev: CalendarEvent, filter: string): boolean {
    if (!filter || filter === 'all') return true;
    const filterLower = filter.trim().toLowerCase();
    const eventSubjectLower = (ev.tenMonHoc || '').trim().toLowerCase();
    const eventTitleLower = (ev.title || '').trim().toLowerCase();

    if (eventSubjectLower && (
      eventSubjectLower === filterLower ||
      eventSubjectLower.includes(filterLower) ||
      filterLower.includes(eventSubjectLower)
    )) {
      return true;
    }

    if (eventTitleLower && (
      eventTitleLower === filterLower ||
      eventTitleLower.includes(filterLower) ||
      filterLower.includes(eventTitleLower)
    )) {
      return true;
    }

    if (ev.maMonHoc !== undefined && String(ev.maMonHoc) === filterLower) {
      return true;
    }

    return false;
  }

  matchesEventType(ev: CalendarEvent, typeFilter: string): boolean {
    if (!typeFilter || typeFilter === 'all') return true;
    if (typeFilter === 'ExamSchedule' && ev.type !== 'Lịch thi') return false;
    if (typeFilter === 'ClassSchedule' && ev.type !== 'Lịch học') return false;
    if (typeFilter === 'PersonalEvent' && ev.type !== 'Hoạt động' && ev.type !== 'Sắp tới') return false;
    return true;
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
      if (!this.matchesEventSubject(ev, this.selectedSubjectFilter)) {
        return false;
      }

      // 3. Schedule type filter
      if (!this.matchesEventType(ev, this.selectedTypeFilter)) {
        return false;
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
          if (this.subjectTags.length === 0) {
            this.extractSubjectsFromEvents();
          }
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

  isPopoverOpen(event: CalendarEvent): boolean {
    if (!event || !this.pinnedEvent) return false;
    return this.pinnedEvent === event;
  }

  selectEvent(event: CalendarEvent, e?: MouseEvent) {
    if (e) {
      e.stopPropagation();
    }
    if (this.pinnedEvent === event) {
      this.pinnedEvent = null;
      this.selectedEvent = null;
    } else {
      this.pinnedEvent = event;
      this.selectedEvent = event;
    }
  }

  closePopover(e?: MouseEvent) {
    if (e) {
      e.stopPropagation();
    }
    this.pinnedEvent = null;
    this.selectedEvent = null;
  }

  closeDetailCard() {
    this.pinnedEvent = null;
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

  editEvent(event: CalendarEvent, e?: MouseEvent) {
    if (e) {
      e.stopPropagation();
    }
    const targetId = event.stringId ? String(event.stringId) : String(event.id);
    this.router.navigate(['/calendar/create'], { queryParams: { editId: targetId } });
  }

  deleteEvent(event: CalendarEvent, e?: MouseEvent) {
    if (e) {
      e.stopPropagation();
    }
    const titleStr = event.title;
    if (confirm(`Bạn có chắc chắn muốn xóa lịch "${titleStr}" này không?`)) {
      const idToDelete = event.id;
      const eventType = event.stringId?.split('_')[0] || (event.type === 'Lịch học' ? 'ClassSchedule' : (event.type === 'Lịch thi' ? 'ExamSchedule' : 'PersonalEvent'));
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

  editCurrentEvent() {
    if (!this.selectedEvent) return;
    this.editEvent(this.selectedEvent);
  }

  deleteCurrentEvent() {
    if (!this.selectedEvent) return;
    this.deleteEvent(this.selectedEvent);
  }

  isShortEvent(event: CalendarEvent): boolean {
    return (event.durationHours || 1) < 0.75;
  }

  isMediumEvent(event: CalendarEvent): boolean {
    const d = event.durationHours || 1;
    return d >= 0.75 && d < 1.25;
  }

  getStartTime(timeStr: string): string {
    if (!timeStr) return '';
    return timeStr.split(' - ')[0] || timeStr;
  }

  isTooltipBelow(event: CalendarEvent): boolean {
    return (event.startHour || 0) < 4.5;
  }

  getTooltipPosClass(colIndex?: number): string {
    if (this.currentViewTab === 'day') {
      return 'left-2 sm:left-4 translate-x-0';
    }
    if (colIndex !== undefined) {
      if (colIndex <= 1) {
        return 'left-0 translate-x-0';
      }
      if (colIndex >= 5) {
        return 'right-0 left-auto translate-x-0';
      }
    }
    return 'left-1/2 -translate-x-1/2';
  }

  getTooltipArrowClass(colIndex?: number): string {
    if (this.currentViewTab === 'day') {
      return 'left-8';
    }
    if (colIndex !== undefined) {
      if (colIndex <= 1) {
        return 'left-6';
      }
      if (colIndex >= 5) {
        return 'right-6';
      }
    }
    return 'left-1/2 -translate-x-1/2';
  }

  isMonthTooltipBelow(cellIndex: number): boolean {
    // Only the first top row (Row 0: index 0 to 6) opens below
    // All rows from row 1 onwards (index >= 7) open ABOVE to avoid being cut off at the bottom
    return cellIndex < 7;
  }

  getMonthTooltipPosClass(cellIndex: number): string {
    const col = cellIndex % 7;
    if (col <= 1) return 'left-0 left-auto translate-x-0';
    if (col >= 5) return 'right-0 left-auto translate-x-0';
    return 'left-1/2 -translate-x-1/2';
  }

  getMonthTooltipArrowClass(cellIndex: number): string {
    const col = cellIndex % 7;
    if (col <= 1) return 'left-6';
    if (col >= 5) return 'right-6';
    return 'left-1/2 -translate-x-1/2';
  }

  isCellHasActivePopover(cell: MonthDayCell): boolean {
    const active = this.hoveredEvent || this.pinnedEvent;
    if (!active || !cell.events) return false;
    return cell.events.some(e => e === active);
  }

  getEventDurationStr(event: CalendarEvent): string {
    const dur = event.durationHours || 1;
    const hours = Math.floor(dur);
    const mins = Math.round((dur - hours) * 60);
    if (hours > 0 && mins > 0) {
      return `${hours} giờ ${mins}p`;
    } else if (hours > 0) {
      return `${hours} giờ`;
    } else {
      return `${mins} phút`;
    }
  }

  getEventStyle(event: CalendarEvent) {
    const isDayView = this.currentViewTab === 'day';
    const slotHeight = isDayView ? 56 : 52;
    const topPx = event.startHour * slotHeight;
    const isShort = this.isShortEvent(event);
    const maxDur = Math.min(event.durationHours, Math.max(24 - event.startHour, 0.25));
    // For short events (< 45 mins), min-height 28px
    // For 1-hour events, height 48px
    const heightPx = isShort 
      ? Math.max(maxDur * slotHeight - 2, 28) 
      : Math.max(maxDur * slotHeight - 4, 48);
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
