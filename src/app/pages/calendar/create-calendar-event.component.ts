import { Component, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { CalendarService, CreateCalendarEventRequest } from '../../services/calendar.service';
import { SubjectService, SubjectTag } from '../../services/subject.service';

export interface ColorOption {
  hex: string;
  name: string;
}

export interface ScheduleTypeOption {
  value: string; // 'ExamSchedule' | 'ClassSchedule' | 'PersonalEvent'
  label: string;
  badgeBg: string;
  badgeText: string;
  color: string;
}

@Component({
  selector: 'app-create-calendar-event',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './create-calendar-event.component.html'
})
export class CreateCalendarEventComponent implements OnInit {
  isSubmitting = false;
  isEditMode = false;
  editEventId: string | number | null = null;

  // Dropdown Open States
  isTypeDropdownOpen = false;
  isSubjectDropdownOpen = false;

  // Create Tag Modal State
  showTagModal = false;
  newTagName = '';
  newTagColor = '#6366F1';

  colorOptions: ColorOption[] = [
    { hex: '#6366F1', name: 'Tím Indigo' },
    { hex: '#3B82F6', name: 'Xanh dương' },
    { hex: '#10B981', name: 'Xanh lá' },
    { hex: '#F59E0B', name: 'Cam' },
    { hex: '#EC4899', name: 'Hồng' },
    { hex: '#14B8A6', name: 'Xanh ngọc' },
    { hex: '#EF4444', name: 'Đỏ' }
  ];

  typeOptions: ScheduleTypeOption[] = [
    { value: 'ExamSchedule', label: 'Lịch thi', badgeBg: 'bg-purple-100', badgeText: 'text-purple-700', color: '#6366F1' },
    { value: 'ClassSchedule', label: 'Lịch học', badgeBg: 'bg-blue-100', badgeText: 'text-blue-700', color: '#3B82F6' },
    { value: 'PersonalEvent', label: 'Sự kiện / Hoạt động', badgeBg: 'bg-amber-100', badgeText: 'text-amber-700', color: '#F59E0B' }
  ];

  subjectTags: SubjectTag[] = [
    { id: 1, name: 'Cơ sở dữ liệu', color: '#3B82F6' },
    { id: 2, name: 'Cấu trúc dữ liệu và giải thuật', color: '#F59E0B' },
    { id: 3, name: 'Lập trình Web', color: '#10B981' },
    { id: 4, name: 'Tiếng Anh 2', color: '#8B5CF6' },
    { id: 5, name: 'PTPM', color: '#6366F1' },
    { id: 6, name: 'Java', color: '#10B981' },
    { id: 7, name: 'Kỹ năng mềm', color: '#EC4899' },
    { id: 8, name: 'Công nghệ phần mềm', color: '#14B8A6' },
    { id: 9, name: 'Thiết kế', color: '#F97316' },
    { id: 10, name: 'Toán', color: '#14B8A6' }
  ];

  selectedType: ScheduleTypeOption = this.typeOptions[0];
  selectedSubjectTag: SubjectTag = this.subjectTags[0];

  showStartTimePicker = false;
  showEndTimePicker = false;

  timeOptions: { value: string; label: string }[] = this.generateTimeOptions();

  private generateTimeOptions(): { value: string; label: string }[] {
    const options: { value: string; label: string }[] = [];
    for (let hour = 0; hour < 24; hour++) {
      for (let min = 0; min < 60; min += 15) {
        const valStr = `${hour.toString().padStart(2, '0')}:${min.toString().padStart(2, '0')}`;
        const period = hour < 12 ? 'SA' : 'CH';
        const label = `${valStr} ${period}`;
        options.push({ value: valStr, label });
      }
    }
    return options;
  }

  ensureTimeOption(valStr: string) {
    if (!valStr) return;
    const exists = this.timeOptions.some(o => o.value === valStr);
    if (!exists) {
      const parts = valStr.split(':');
      const hour = parseInt(parts[0], 10) || 0;
      const period = hour < 12 ? 'SA' : 'CH';
      const label = `${valStr} ${period}`;
      this.timeOptions.push({ value: valStr, label });
      this.timeOptions.sort((a, b) => a.value.localeCompare(b.value));
    }
  }

  getTimeLabel(val: string): string {
    const found = this.timeOptions.find(o => o.value === val);
    if (found) return found.label;
    if (!val) return '09:00 SA';
    const parts = val.split(':');
    const hour = parseInt(parts[0], 10) || 0;
    const period = hour < 12 ? 'SA' : 'CH';
    return `${val} ${period}`;
  }

  toggleStartTimePicker(e: MouseEvent) {
    e.stopPropagation();
    this.showStartTimePicker = !this.showStartTimePicker;
    this.showEndTimePicker = false;
  }

  toggleEndTimePicker(e: MouseEvent) {
    e.stopPropagation();
    this.showEndTimePicker = !this.showEndTimePicker;
    this.showStartTimePicker = false;
  }

  selectStartTime(val: string, e: MouseEvent) {
    e.stopPropagation();
    this.eventForm.startTime = val;
    this.showStartTimePicker = false;
  }

  selectEndTime(val: string, e: MouseEvent) {
    e.stopPropagation();
    this.eventForm.endTime = val;
    this.showEndTimePicker = false;
  }

  @HostListener('document:click')
  closeTimePickers() {
    this.showStartTimePicker = false;
    this.showEndTimePicker = false;
  }

  eventForm = {
    title: '',
    type: 'ExamSchedule',
    typeLabel: 'Lịch thi',
    subject: 'Cơ sở dữ liệu',
    description: '',
    startDate: '',
    startTime: '09:00',
    endDate: '',
    endTime: '11:00',
    location: 'Phòng A101',
    teacher: 'Nguyễn Văn A',
    format: 'Trực tiếp', // 'Trực tiếp' | 'Online'
    color: '#6366F1'
  };

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private calendarService: CalendarService,
    private subjectService: SubjectService,
    private elementRef: ElementRef
  ) {}

  ngOnInit() {
    const today = new Date();
    const year = today.getFullYear();
    const month = (today.getMonth() + 1).toString().padStart(2, '0');
    const day = today.getDate().toString().padStart(2, '0');
    const dateStr = `${year}-${month}-${day}`;

    this.eventForm.startDate = dateStr;
    this.eventForm.endDate = dateStr;

    this.subjectService.getSubjectTags().subscribe({
      next: (tags) => {
        if (tags && tags.length > 0) {
          this.subjectTags = tags;
        }
      },
      error: (err) => console.warn('Could not load subject tags from SubjectService:', err)
    });

    this.route.queryParams.subscribe(params => {
      const editId = params['editId'];
      if (editId) {
        this.isEditMode = true;
        this.editEventId = editId;

        const allEvents = this.calendarService.getLocalEvents();
        const found = allEvents.find(e => String(e.id) === String(editId) || (e.sourceId && String(e.sourceId) === String(editId)));
        if (found) {
          this.eventForm.title = found.title;
          this.eventForm.type = found.eventType;
          const matchedType = this.typeOptions.find(t => t.value === found.eventType);
          if (matchedType) this.selectedType = matchedType;

          this.eventForm.location = found.location || 'Phòng A101';
          if (found.giangVien) this.eventForm.teacher = found.giangVien;
          if (found.hinhThucThi) this.eventForm.format = found.hinhThucThi;
          if (found.description) this.eventForm.description = found.description;

          if (found.maMonHoc) {
            const tag = this.subjectTags.find(t => t.id === found.maMonHoc);
            if (tag) {
              this.selectedSubjectTag = tag;
              this.eventForm.subject = tag.name;
            }
          }

          if (found.start) {
            const startD = new Date(found.start);
            this.eventForm.startDate = startD.toISOString().substring(0, 10);
            this.eventForm.startTime = `${startD.getHours().toString().padStart(2, '0')}:${startD.getMinutes().toString().padStart(2, '0')}`;
            this.ensureTimeOption(this.eventForm.startTime);
          }
          if (found.end) {
            const endD = new Date(found.end);
            this.eventForm.endDate = endD.toISOString().substring(0, 10);
            this.eventForm.endTime = `${endD.getHours().toString().padStart(2, '0')}:${endD.getMinutes().toString().padStart(2, '0')}`;
            this.ensureTimeOption(this.eventForm.endTime);
          }
        }
      }
    });
  }

  // Close dropdowns when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-dropdown-container')) {
      this.isTypeDropdownOpen = false;
      this.isSubjectDropdownOpen = false;
    }
  }

  // Type Dropdown Handling
  toggleTypeDropdown(event: Event) {
    event.stopPropagation();
    this.isTypeDropdownOpen = !this.isTypeDropdownOpen;
    this.isSubjectDropdownOpen = false;
  }

  selectType(option: ScheduleTypeOption, event: Event) {
    event.stopPropagation();
    this.selectedType = option;
    this.eventForm.type = option.value;
    this.eventForm.typeLabel = option.label;
    this.eventForm.color = option.color;
    this.isTypeDropdownOpen = false;
  }

  // Subject Dropdown Handling
  toggleSubjectDropdown(event: Event) {
    event.stopPropagation();
    this.isSubjectDropdownOpen = !this.isSubjectDropdownOpen;
    this.isTypeDropdownOpen = false;
  }

  selectSubjectTag(tag: SubjectTag, event: Event) {
    event.stopPropagation();
    this.selectedSubjectTag = tag;
    this.eventForm.subject = tag.name;
    this.eventForm.color = tag.color;
    this.isSubjectDropdownOpen = false;
  }

  // Tag Modal Handling
  openCreateTagModal(event: Event) {
    event.stopPropagation();
    this.isSubjectDropdownOpen = false;
    this.newTagName = '';
    this.newTagColor = '#6366F1';
    this.showTagModal = true;
  }

  closeTagModal() {
    this.showTagModal = false;
  }

  addNewTag() {
    if (!this.newTagName || !this.newTagName.trim()) {
      alert('Vui lòng nhập tên tag!');
      return;
    }
    const newTag = this.subjectService.addSubjectTag(this.newTagName.trim(), this.newTagColor);
    if (!this.subjectTags.some(t => t.name.toLowerCase() === newTag.name.toLowerCase())) {
      this.subjectTags.push(newTag);
    }
    this.selectedSubjectTag = newTag;
    this.eventForm.subject = newTag.name;
    this.eventForm.color = newTag.color;
    this.showTagModal = false;
  }

  selectColor(colorHex: string) {
    this.eventForm.color = colorHex;
  }

  getTagBadgeStyle(colorHex: string) {
    return {
      'background-color': `${colorHex}18`,
      'color': colorHex,
      'border': `1px solid ${colorHex}40`
    };
  }

  getCalculatedDuration(): string {
    if (!this.eventForm.startTime || !this.eventForm.endTime) {
      return '0 giờ';
    }
    const [startH, startM] = this.eventForm.startTime.split(':').map(Number);
    const [endH, endM] = this.eventForm.endTime.split(':').map(Number);

    let startMins = startH * 60 + startM;
    let endMins = endH * 60 + endM;

    if (endMins < startMins) {
      endMins += 24 * 60;
    }

    const diffMins = endMins - startMins;
    const hours = Math.floor(diffMins / 60);
    const mins = diffMins % 60;

    if (mins > 0) {
      return `${hours} giờ ${mins} phút`;
    }
    return `${hours} giờ`;
  }

  getFormattedDisplayDate(): string {
    if (!this.eventForm.startDate) return 'Chưa chọn ngày';
    const parts = this.eventForm.startDate.split('-');
    if (parts.length === 3) {
      return `${parts[2]}/${parts[1]}/${parts[0]}`;
    }
    return this.eventForm.startDate;
  }

  onSubmit() {
    if (!this.eventForm.title || !this.eventForm.title.trim()) {
      alert('Vui lòng nhập tiêu đề lịch!');
      return;
    }

    this.isSubmitting = true;

    const startDateTime = `${this.eventForm.startDate}T${this.eventForm.startTime}:00`;
    const endDateTime = `${this.eventForm.endDate}T${this.eventForm.endTime}:00`;

    const request: CreateCalendarEventRequest = {
      tieuDe: this.eventForm.title.trim(),
      moTa: this.eventForm.description ? this.eventForm.description.trim() : undefined,
      thoiGianBatDau: startDateTime,
      thoiGianKetThuc: endDateTime,
      diaDiem: this.eventForm.location || 'Phòng học',
      mauSac: this.eventForm.color,
      eventType: this.eventForm.type,
      maMonHoc: this.selectedSubjectTag?.id,
      giangVien: this.eventForm.teacher,
      hinhThucThi: this.eventForm.format
    };

    if (this.isEditMode && this.editEventId) {
      const numId = typeof this.editEventId === 'number' ? this.editEventId : (parseInt(String(this.editEventId).replace(/\D/g, ''), 10) || 0);
      this.calendarService.updateEvent(numId, request).subscribe({
        next: (apiDto) => {
          this.isSubmitting = false;
          this.router.navigate(['/calendar'], { queryParams: { selectedId: String(apiDto?.id || this.editEventId) } });
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Error updating event:', err);
          alert('Lỗi cập nhật lịch: ' + (err?.error?.message || 'Vui lòng kiểm tra lại thông tin.'));
        }
      });
    } else {
      this.calendarService.createEvent(request).subscribe({
        next: (apiDto) => {
          this.isSubmitting = false;
          this.router.navigate(['/calendar'], { queryParams: { selectedId: String(apiDto.id) } });
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Error creating event:', err);
          alert('Lỗi tạo lịch: ' + (err?.error?.message || 'Vui lòng kiểm tra lại thông tin.'));
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['/calendar']);
  }
}
