import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { GroupService, NhomHocTapDto, ThanhVienNhomDto, CreateStudyGroupRequest, GroupTaskBackendDto, CreateGroupTaskBackendRequest } from '../../services/group.service';
import { ChatService, TinNhanDto } from '../../services/chat.service';
import { ChatSignalRService } from '../../services/chat-signalr.service';

export interface GroupItem {
  id: number;
  name: string;
  code: string;
  iconBg: string;
  iconColor: string;
  isActive?: boolean;
  leader?: string;
  description?: string;
  membersCount?: number;
  createdDate?: string;
  leaderId?: number;
  leaderAvatar?: string;
  leaderName?: string;
  leaderEmail?: string;
  isOwner?: boolean;
}

export interface ChatMessage {
  id: number;
  senderName: string;
  senderAvatar: string;
  time: string;
  content: string;
  isMe: boolean;
  loaiTinNhan?: number;
  attachment?: {
    fileName: string;
    fileSize: string;
    fileUrl?: string;
    ext?: string;
  };
  reaction?: {
    emoji: string;
    count: number;
  };
}

// Keep for backward compat with overview tab
export interface TaskItemSummary {
  id: number; title: string; assigneeAvatar: string; assigneeName: string;
  dueDate?: string; priority?: string; priorityClass?: string; completed: boolean;
}

export interface KanbanLabel { id: number; text: string; colorClass: string; }
export interface ChecklistItem { id: number; text: string; done: boolean; }
export interface KanbanComment { id: number; text: string; author: string; avatar: string; time: string; }

export interface KanbanTask {
  id: number;
  title: string;
  description: string;
  subjectTag?: string;
  subjectTagClass?: string;
  labels: KanbanLabel[];
  checklist: ChecklistItem[];
  comments: KanbanComment[];
  assigneeName: string;
  assigneeAvatar: string;
  dueDate: string;
  priority: 'Cao' | 'Trung bình' | 'Thấp';
  priorityClass: string;
  completed: boolean;
  column: 'todo' | 'inProgress' | 'review' | 'done';
}

export interface KanbanColumn {
  id: 'todo' | 'inProgress' | 'review' | 'done';
  title: string; dotClass: string; bgClass: string; countClass: string; borderClass: string;
}

@Component({
  selector: 'app-groups',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './groups.component.html',
  styles: [`
    .chat-scroll::-webkit-scrollbar { width: 4px; }
    .chat-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 9999px; }
  `]
})
export class GroupsComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('chatScrollContainer') chatScrollContainer!: ElementRef;

  activeTab: 'overview' | 'chat' | 'docs' | 'tasks' | 'meetings' | 'members' | 'settings' = 'overview';
  activeGroupId: number = 0;
  newMessageText: string = '';
  shouldScrollToBottom: boolean = false;

  // Loading and Error States
  loadingGroups: boolean = false;
  loadingMessages: boolean = false;
  loadingMembers: boolean = false;
  errorMessage: string = '';

  // Dropdown select state
  showGroupDropdown: boolean = false;

  // Collapsible sidebar states
  isLeftSidebarCollapsed: boolean = false;
  isRightSidebarCollapsed: boolean = false;

  // Real Groups List from API
  groupsList: GroupItem[] = [];
  rawGroups: NhomHocTapDto[] = [];

  // Real Chat Messages from API + SignalR
  chatMessages: ChatMessage[] = [];

  // Real Members List from API
  membersList: { name: string; role: string; avatar: string; status: string; statusClass: string }[] = [];

  // Subscriptions
  private subscriptions: Subscription = new Subscription();

  // ── Full Kanban Engine ──
  kanbanTasks: KanbanTask[] = [];

  kanbanColumns: KanbanColumn[] = [
    { id: 'todo',       title: 'Cần làm',      dotClass: 'bg-slate-400',   bgClass: 'bg-slate-50',       countClass: 'bg-slate-100 text-slate-500',    borderClass: 'border-gray-200' },
    { id: 'inProgress', title: 'Đang làm',     dotClass: 'bg-amber-400',   bgClass: 'bg-amber-50/50',    countClass: 'bg-amber-100 text-amber-600',    borderClass: 'border-amber-100' },
    { id: 'review',     title: 'Xem xét',      dotClass: 'bg-purple-400',  bgClass: 'bg-purple-50/40',   countClass: 'bg-purple-100 text-purple-600',  borderClass: 'border-purple-100' },
    { id: 'done',       title: 'Hoàn thành',   dotClass: 'bg-emerald-400', bgClass: 'bg-emerald-50/40',  countClass: 'bg-emerald-100 text-emerald-600',borderClass: 'border-emerald-100' }
  ];

  availableLabels: KanbanLabel[] = [
    { id: 1, text: 'Bug',      colorClass: 'bg-rose-500' },
    { id: 2, text: 'Feature',  colorClass: 'bg-blue-500' },
    { id: 3, text: 'Design',   colorClass: 'bg-purple-500' },
    { id: 4, text: 'Backend',  colorClass: 'bg-emerald-500' },
    { id: 5, text: 'Frontend', colorClass: 'bg-amber-500' },
    { id: 6, text: 'Urgent',   colorClass: 'bg-pink-500' },
  ];

  // Card detail panel
  selectedCard: KanbanTask | null = null;
  editingCard: KanbanTask | null = null;
  showCardDetail: boolean = false;
  newChecklistText: string = '';
  newCommentText: string = '';
  showLabelPicker: boolean = false;

  // Filters
  taskFilter = { search: '', priority: '' as '' | 'Cao' | 'Trung bình' | 'Thấp', assignee: '' };
  showTaskFilter: boolean = false;

  // Quick add per column
  quickAddCol: string = '';
  quickAddTitle: string = '';

  // Add task modal
  showTaskModal: boolean = false;
  newTaskForm = {
    title: '', description: '', assigneeName: '',
    priority: 'Trung bình' as 'Cao' | 'Trung bình' | 'Thấp', dueDate: '',
    status: 'todo' as 'todo' | 'inProgress' | 'review' | 'done'
  };

  // Computed getters (backward compat + overview)
  get todoTasks(): KanbanTask[]       { return this.getFilteredTasks('todo'); }
  get inProgressTasks(): KanbanTask[] { return this.getFilteredTasks('inProgress'); }
  get doneTasks(): KanbanTask[]       { return this.getFilteredTasks('done'); }


  groupMeetings: any[] = [];
  filteredMeetings: any[] = [];
  currentDate: Date = new Date();
  currentMonth: number = this.currentDate.getMonth();
  currentYear: number = this.currentDate.getFullYear();
  calendarDays: { day: number; isCurrentMonth: boolean; hasMeeting: boolean; isToday: boolean; isSelected: boolean }[] = [];
  selectedDate: Date = new Date(this.currentYear, this.currentMonth, this.currentDate.getDate());

  generateCalendar(): void {
    this.calendarDays = [];
    const firstDay = new Date(this.currentYear, this.currentMonth, 1).getDay();
    const daysInMonth = new Date(this.currentYear, this.currentMonth + 1, 0).getDate();
    const daysInPrevMonth = new Date(this.currentYear, this.currentMonth, 0).getDate();
    
    // Adjust for Monday start (0=Sunday -> 6=Sunday, 0=Monday)
    let startDay = firstDay === 0 ? 6 : firstDay - 1;

    // Prev month days
    for (let i = startDay - 1; i >= 0; i--) {
      this.calendarDays.push({ day: daysInPrevMonth - i, isCurrentMonth: false, hasMeeting: false, isToday: false, isSelected: false });
    }

    // Current month days
    const today = new Date();
    for (let i = 1; i <= daysInMonth; i++) {
      const date = new Date(this.currentYear, this.currentMonth, i);
      const isToday = date.getDate() === today.getDate() && date.getMonth() === today.getMonth() && date.getFullYear() === today.getFullYear();
      const isSelected = this.selectedDate && date.getDate() === this.selectedDate.getDate() && date.getMonth() === this.selectedDate.getMonth() && date.getFullYear() === this.selectedDate.getFullYear();
      
      const hasMeeting = this.groupMeetings.some(m => {
        const mDate = new Date(m.thoiGianBatDau);
        return mDate.getDate() === i && mDate.getMonth() === this.currentMonth && mDate.getFullYear() === this.currentYear;
      });

      this.calendarDays.push({ day: i, isCurrentMonth: true, hasMeeting: hasMeeting, isToday: isToday, isSelected: isSelected });
    }

    // Next month days to fill 42 cells (6 rows)
    let nextMonthDay = 1;
    while (this.calendarDays.length < 42) {
      this.calendarDays.push({ day: nextMonthDay++, isCurrentMonth: false, hasMeeting: false, isToday: false, isSelected: false });
    }
  }

  prevMonth(): void {
    if (this.currentMonth === 0) {
      this.currentMonth = 11;
      this.currentYear--;
    } else {
      this.currentMonth--;
    }
    this.generateCalendar();
  }

  nextMonth(): void {
    if (this.currentMonth === 11) {
      this.currentMonth = 0;
      this.currentYear++;
    } else {
      this.currentMonth++;
    }
    this.generateCalendar();
  }

  selectDate(day: any): void {
    if (!day.isCurrentMonth) return;
    this.selectedDate = new Date(this.currentYear, this.currentMonth, day.day);
    this.generateCalendar(); // update selected state
    this.filterMeetingsByDate();
  }

  filterMeetingsByDate(): void {
    this.filteredMeetings = this.groupMeetings.filter(m => {
      const mDate = new Date(m.thoiGianBatDau);
      return mDate.getDate() === this.selectedDate.getDate() && 
             mDate.getMonth() === this.selectedDate.getMonth() && 
             mDate.getFullYear() === this.selectedDate.getFullYear();
    });
  }

  // Meeting modal
  showMeetingModal: boolean = false;
  meetingSubmitLoading: boolean = false;
  meetingModalError: string = '';
  editingMeetingId: number | null = null;
  meetingForm = {
    tieuDe: '',
    moTa: '',
    nenTang: 'Google Meet',
    duongDan: '',
    ngayHop: '',
    gioBatDau: '08:00',
    gioKetThuc: '09:00'
  };

  openMeetingModal(mt?: any): void {
    this.meetingModalError = '';
    this.meetingSubmitLoading = false;
    if (mt) {
      this.editingMeetingId = mt.maLichHop;
      const start = new Date(mt.thoiGianBatDau);
      const end = new Date(mt.thoiGianKetThuc);
      const year = start.getFullYear();
      const month = String(start.getMonth() + 1).padStart(2, '0');
      const day = String(start.getDate()).padStart(2, '0');
      const startH = String(start.getHours()).padStart(2, '0');
      const startM = String(start.getMinutes()).padStart(2, '0');
      const endH = String(end.getHours()).padStart(2, '0');
      const endM = String(end.getMinutes()).padStart(2, '0');

      this.meetingForm = {
        tieuDe: mt.tieuDe,
        moTa: mt.moTa || '',
        nenTang: mt.nenTang || 'Google Meet',
        duongDan: mt.duongDan,
        ngayHop: `${year}-${month}-${day}`,
        gioBatDau: `${startH}:${startM}`,
        gioKetThuc: `${endH}:${endM}`
      };
    } else {
      this.editingMeetingId = null;
      const now = new Date();
      const todayStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
      this.meetingForm = { 
        tieuDe: '', 
        moTa: '', 
        nenTang: 'Google Meet', 
        duongDan: '', 
        ngayHop: todayStr, 
        gioBatDau: '08:00', 
        gioKetThuc: '09:00' 
      };
    }
    this.showMeetingModal = true;
  }

  closeMeetingModal(): void {
    this.showMeetingModal = false;
    this.editingMeetingId = null;
  }

  private extractApiErrorMessage(err: any, fallback: string): string {
    if (!err) return fallback;
    if (typeof err.error === 'string') return err.error;
    if (err.error?.message) return err.error.message;
    if (err.error?.errors) {
      const keys = Object.keys(err.error.errors);
      if (keys.length > 0) {
        const firstKey = keys[0];
        const msgs = err.error.errors[firstKey];
        if (Array.isArray(msgs) && msgs.length > 0) {
          return `${firstKey}: ${msgs[0]}`;
        }
      }
    }
    if (err.error?.title) return err.error.title;
    if (err.status) return `Lỗi ${err.status}: ${err.statusText || 'Không thể kết nối máy chủ'}`;
    return fallback;
  }

  submitCreateMeeting(): void {
    if (!this.meetingForm.tieuDe.trim() || !this.meetingForm.duongDan.trim() || !this.meetingForm.ngayHop || !this.meetingForm.gioBatDau || !this.meetingForm.gioKetThuc) {
      this.meetingModalError = 'Vui lòng điền đầy đủ thông tin bắt buộc!';
      return;
    }

    this.meetingSubmitLoading = true;
    this.meetingModalError = '';

    const formatTimeStr = (t: string) => (t && t.length === 5) ? `${t}:00` : t;
    const startIso = new Date(`${this.meetingForm.ngayHop}T${formatTimeStr(this.meetingForm.gioBatDau)}`).toISOString();
    const endIso = new Date(`${this.meetingForm.ngayHop}T${formatTimeStr(this.meetingForm.gioKetThuc)}`).toISOString();

    const req = {
      tieuDe: this.meetingForm.tieuDe.trim(),
      moTa: this.meetingForm.moTa || '',
      nenTang: this.meetingForm.nenTang,
      duongDan: this.meetingForm.duongDan.trim(),
      thoiGianBatDau: startIso,
      thoiGianKetThuc: endIso
    };

    if (this.activeGroupId) {
      if (this.editingMeetingId) {
        this.groupService.updateGroupMeeting(this.activeGroupId, this.editingMeetingId, req).subscribe({
          next: (updated) => {
            this.meetingSubmitLoading = false;
            this.showMeetingModal = false;
            const idx = this.groupMeetings.findIndex(m => m.maLichHop === this.editingMeetingId);
            if (idx !== -1) {
              this.groupMeetings[idx] = updated;
            }
            this.editingMeetingId = null;
            this.generateCalendar();
            this.filterMeetingsByDate();
          },
          error: (err) => {
            this.meetingSubmitLoading = false;
            console.error('Error updating meeting:', err);
            this.meetingModalError = this.extractApiErrorMessage(err, 'Có lỗi xảy ra khi cập nhật lịch họp.');
          }
        });
      } else {
        this.groupService.createGroupMeeting(this.activeGroupId, req).subscribe({
          next: (meeting) => {
            this.meetingSubmitLoading = false;
            this.showMeetingModal = false;
            this.groupMeetings = [...this.groupMeetings, meeting];
            this.generateCalendar();
            this.filterMeetingsByDate();
          },
          error: (err) => {
            this.meetingSubmitLoading = false;
            console.error('Error creating meeting:', err);
            this.meetingModalError = this.extractApiErrorMessage(err, 'Có lỗi xảy ra khi tạo lịch họp.');
          }
        });
      }
    }
  }

  deleteMeeting(meeting: any): void {
    if (confirm(`Bạn có chắc chắn muốn xóa cuộc họp "${meeting.tieuDe}" không?`)) {
      if (this.activeGroupId && meeting.maLichHop) {
        this.groupService.deleteGroupMeeting(this.activeGroupId, meeting.maLichHop).subscribe({
          next: () => {
            this.groupMeetings = this.groupMeetings.filter(m => m.maLichHop !== meeting.maLichHop);
            this.generateCalendar();
            this.filterMeetingsByDate();
          },
          error: (err) => {
            console.error('Error deleting meeting:', err);
            this.groupMeetings = this.groupMeetings.filter(m => m.maLichHop !== meeting.maLichHop);
            this.generateCalendar();
            this.filterMeetingsByDate();
          }
        });
      } else {
        this.groupMeetings = this.groupMeetings.filter(m => m !== meeting);
        this.generateCalendar();
        this.filterMeetingsByDate();
      }
    }
  }
  // ── Document Management Engine (Google Drive Master-Detail Layout) ──
  folderSearchText: string = '';
  docSearchText: string = '';
  selectedFolderId: number = 0; // 0 = Tất cả tài liệu
  docViewMode: 'list' | 'grid' = 'list';
  currentPageDoc: number = 1;

  // Folder & Doc Modals
  showCreateFolderModal: boolean = false;
  newFolderName: string = '';
  showEditFolderModal: boolean = false;
  editingFolderId: number = 0;
  editingFolderName: string = '';
  showUploadDocModal: boolean = false;
  uploadDocForm = {
    title: '',
    folderId: 0,
    fileSize: '2.5 MB'
  };

  // Group Settings Form
  editGroupForm = {
    tenNhom: '',
    moTa: '',
    anhDaiDien: ''
  };

  copyActiveGroupCode(): void {
    const code = this.activeGroup?.code || 'VU9441';
    navigator.clipboard.writeText(code);
    alert(`Đã sao chép mã nhóm: ${code}`);
  }

  cancelEditGroupSettings(): void {
    if (this.activeGroup) {
      this.editGroupForm.tenNhom = this.activeGroup.name || '';
      this.editGroupForm.moTa = this.activeGroup.description || '';
    }
  }

  saveGroupSettings(): void {
    if (!this.editGroupForm.tenNhom.trim()) {
      alert('Vui lòng nhập tên nhóm!');
      return;
    }
    if (this.activeGroup) {
      this.activeGroup.name = this.editGroupForm.tenNhom.trim();
      this.activeGroup.description = this.editGroupForm.moTa.trim();
      alert('Cập nhật thông tin nhóm thành công!');
    }
  }

  triggerAvatarUpload(inputElement: HTMLInputElement): void {
    if (inputElement) inputElement.click();
  }

  onGroupAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.editGroupForm.anhDaiDien = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  foldersList: { id: number; name: string; filesCount: number; isAll?: boolean }[] = [
    { id: 0, name: 'Tất cả tài liệu', filesCount: 0, isAll: true }
  ];

  docsList: {
    id: number;
    name: string;
    ext: string;
    extBg: string;
    size: string;
    uploaderName: string;
    uploaderAvatar: string;
    updatedAt: string;
    folderId: number;
  }[] = [];

  get filteredFolders() {
    if (!this.folderSearchText.trim()) return this.foldersList;
    return this.foldersList.filter(f => f.name.toLowerCase().includes(this.folderSearchText.toLowerCase().trim()));
  }

  get selectedFolder() {
    return this.foldersList.find(f => f.id === this.selectedFolderId) || this.foldersList[0] || { id: 0, name: 'Tất cả tài liệu', filesCount: 0 };
  }

  get filteredDocs() {
    let list = this.docsList;
    if (this.selectedFolderId !== 0) {
      list = list.filter(d => d.folderId === this.selectedFolderId);
    }
    if (this.docSearchText.trim()) {
      const q = this.docSearchText.toLowerCase().trim();
      list = list.filter(d => d.name.toLowerCase().includes(q) || d.uploaderName.toLowerCase().includes(q));
    }
    return list;
  }

  selectFolder(id: number): void {
    this.selectedFolderId = id;
    this.currentPageDoc = 1;
    if (this.activeGroupId > 0) {
      this.loadGroupDocuments(this.activeGroupId, id);
    }
  }

  openCreateFolderModal(): void {
    this.newFolderName = '';
    this.showCreateFolderModal = true;
  }

  closeCreateFolderModal(): void {
    this.showCreateFolderModal = false;
  }

  submitCreateFolder(): void {
    if (!this.newFolderName.trim()) return;
    
    if (this.activeGroupId > 0) {
      this.groupService.createGroupFolder(this.activeGroupId, { tenThuMuc: this.newFolderName.trim() }).subscribe({
        next: (folder) => {
          this.showCreateFolderModal = false;
          const newFolderId = folder.maThuMuc ?? folder.MaThuMuc;
          this.loadGroupFolders(this.activeGroupId);
          if (newFolderId) {
            this.selectFolder(newFolderId);
          }
        },
        error: (err) => {
          console.error('Error creating folder:', err);
          const newId = Date.now();
          this.foldersList.push({
            id: newId,
            name: this.newFolderName.trim(),
            filesCount: 0
          });
          this.showCreateFolderModal = false;
          this.selectedFolderId = newId;
        }
      });
    } else {
      const newId = Date.now();
      this.foldersList.push({
        id: newId,
        name: this.newFolderName.trim(),
        filesCount: 0
      });
      this.showCreateFolderModal = false;
      this.selectedFolderId = newId;
    }
  }

  openEditFolderModal(folder: any, event: MouseEvent): void {
    event.stopPropagation();
    if (folder.isAll) return;
    this.editingFolderId = folder.id;
    this.editingFolderName = folder.name;
    this.showEditFolderModal = true;
  }

  closeEditFolderModal(): void {
    this.showEditFolderModal = false;
  }

  submitEditFolder(): void {
    if (!this.editingFolderName.trim() || !this.editingFolderId) return;

    if (this.activeGroupId > 0) {
      this.groupService.updateGroupFolder(this.activeGroupId, this.editingFolderId, { tenThuMuc: this.editingFolderName.trim() }).subscribe({
        next: () => {
          this.showEditFolderModal = false;
          this.loadGroupFolders(this.activeGroupId);
        },
        error: (err) => {
          console.error('Error updating folder:', err);
          const item = this.foldersList.find(f => f.id === this.editingFolderId);
          if (item) item.name = this.editingFolderName.trim();
          this.showEditFolderModal = false;
        }
      });
    } else {
      const item = this.foldersList.find(f => f.id === this.editingFolderId);
      if (item) item.name = this.editingFolderName.trim();
      this.showEditFolderModal = false;
    }
  }

  deleteFolder(folder: any, event: MouseEvent): void {
    event.stopPropagation();
    if (folder.isAll) return;

    if (confirm(`Bạn có chắc chắn muốn xóa thư mục "${folder.name}"? Tất cả tài liệu trong thư mục này sẽ được chuyển về mục "Tất cả tài liệu".`)) {
      if (this.activeGroupId > 0) {
        this.groupService.deleteGroupFolder(this.activeGroupId, folder.id).subscribe({
          next: () => {
            if (this.selectedFolderId === folder.id) {
              this.selectFolder(0);
            } else {
              this.loadGroupFolders(this.activeGroupId);
              this.loadGroupDocuments(this.activeGroupId, this.selectedFolderId);
            }
          },
          error: (err) => {
            console.error('Error deleting folder:', err);
            if (err?.status === 404) {
              // Thư mục không tồn tại trên Database (hoặc đã bị xóa), xóa khỏi giao diện
              this.foldersList = this.foldersList.filter(f => f.id !== folder.id);
              if (this.selectedFolderId === folder.id) {
                this.selectFolder(0);
              }
            } else {
              alert('Xóa thư mục thất bại: ' + (err?.error?.message || 'Không thể xóa thư mục trên máy chủ.'));
              this.loadGroupFolders(this.activeGroupId);
            }
          }
        });
      } else {
        this.foldersList = this.foldersList.filter(f => f.id !== folder.id);
        if (this.selectedFolderId === folder.id) {
          this.selectFolder(0);
        }
      }
    }
  }

  // Enhanced Upload Modal State & Drag & Drop
  selectedUploadFiles: { name: string; size: number; sizeFormatted: string; ext: string; extBg: string; file?: File }[] = [];
  isDraggingOver: boolean = false;
  uploadingDocProgress: boolean = false;

  get totalUploadSizeFormatted(): string {
    const bytes = this.selectedUploadFiles.reduce((acc, f) => acc + f.size, 0);
    if (bytes === 0) return '0 B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  openUploadDocModal(): void {
    const targetFolderId = (this.selectedFolderId && this.selectedFolderId > 0) 
      ? this.selectedFolderId 
      : (this.foldersList.find(f => !f.isAll)?.id || 1);

    this.uploadDocForm = {
      title: '',
      folderId: targetFolderId,
      fileSize: '0 MB'
    };

    // Initialize empty selected files array for real user selection
    this.selectedUploadFiles = [];
    this.uploadingDocProgress = false;
    this.showUploadDocModal = true;
  }

  closeUploadDocModal(): void {
    this.showUploadDocModal = false;
    this.selectedUploadFiles = [];
  }

  onUploadDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingOver = true;
  }

  onUploadDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingOver = false;
  }

  onUploadFileDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingOver = false;
    if (event.dataTransfer && event.dataTransfer.files) {
      this.handleSelectedFileList(Array.from(event.dataTransfer.files));
    }
  }

  onFileSelectedFromInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input && input.files) {
      this.handleSelectedFileList(Array.from(input.files));
      input.value = ''; // reset
    }
  }

  handleSelectedFileList(files: File[]): void {
    files.forEach(f => {
      const name = f.name;
      const size = f.size;
      const sizeFormatted = size < 1024 * 1024 ? (size / 1024).toFixed(1) + ' KB' : (size / (1024 * 1024)).toFixed(1) + ' MB';
      const extParts = name.split('.');
      const extStr = extParts.length > 1 ? extParts[extParts.length - 1].toUpperCase() : 'DOC';

      let extBg = 'bg-blue-500';
      if (['PPT', 'PPTX'].includes(extStr)) extBg = 'bg-orange-500';
      else if (['PDF'].includes(extStr)) extBg = 'bg-rose-500';
      else if (['XLS', 'XLSX', 'CSV'].includes(extStr)) extBg = 'bg-emerald-500';
      else if (['ZIP', 'RAR', '7Z'].includes(extStr)) extBg = 'bg-purple-500';
      else if (['SQL', 'JSON', 'JS', 'TS'].includes(extStr)) extBg = 'bg-pink-500';

      this.selectedUploadFiles.push({
        name,
        size,
        sizeFormatted,
        ext: extStr.substring(0, 4),
        extBg,
        file: f
      });
    });
  }

  removeSelectedFile(index: number): void {
    this.selectedUploadFiles.splice(index, 1);
  }

  submitUploadDoc(): void {
    if (this.selectedUploadFiles.length === 0) {
      alert('Vui lòng chọn ít nhất 1 file để tải lên!');
      return;
    }

    this.uploadingDocProgress = true;
    const targetFolderId = Number(this.uploadDocForm.folderId) > 0 ? Number(this.uploadDocForm.folderId) : null;

    if (this.activeGroupId > 0) {
      let completedCount = 0;
      this.selectedUploadFiles.forEach(fileItem => {
        const extParts = fileItem.name.split('.');
        const extStr = extParts.length > 1 ? extParts[extParts.length - 1].toLowerCase() : 'pdf';

        const req = {
          tieuDe: fileItem.name,
          moTa: '',
          maThuMuc: targetFolderId,
          duongDanFile: `https://meet.google.com/uploads/${fileItem.name}`,
          extension: `.${extStr}`,
          dungLuong: fileItem.size
        };

        this.groupService.createGroupDocument(this.activeGroupId, req).subscribe({
          next: () => {
            completedCount++;
            if (completedCount === this.selectedUploadFiles.length) {
              this.uploadingDocProgress = false;
              this.showUploadDocModal = false;
              this.loadGroupFolders(this.activeGroupId);
              this.loadGroupDocuments(this.activeGroupId, this.selectedFolderId);
            }
          },
          error: (err) => {
            console.error('Error uploading file:', err);
            completedCount++;
            if (completedCount === this.selectedUploadFiles.length) {
              this.uploadingDocProgress = false;
              this.showUploadDocModal = false;
              this.loadGroupFolders(this.activeGroupId);
              this.loadGroupDocuments(this.activeGroupId, this.selectedFolderId);
            }
          }
        });
      });
    } else {
      this.uploadingDocProgress = false;
      this.showUploadDocModal = false;
    }
  }

  deleteDoc(doc: any): void {
    if (confirm(`Bạn có chắc chắn muốn xóa tài liệu "${doc.name}" không?`)) {
      if (this.activeGroupId > 0 && doc.id) {
        this.groupService.deleteGroupDocument(this.activeGroupId, doc.id).subscribe({
          next: () => {
            this.loadGroupFolders(this.activeGroupId);
            this.loadGroupDocuments(this.activeGroupId, this.selectedFolderId);
          },
          error: (err) => {
            console.error('Error deleting doc:', err);
            if (err?.status === 404) {
              this.docsList = this.docsList.filter(d => d.id !== doc.id);
            } else {
              alert('Xóa tài liệu thất bại: ' + (err?.error?.message || 'Không thể xóa tài liệu trên máy chủ.'));
              this.loadGroupDocuments(this.activeGroupId, this.selectedFolderId);
            }
          }
        });
      } else {
        this.docsList = this.docsList.filter(d => d.id !== doc.id);
      }
    }
  }

  downloadDoc(doc: any): void {
    if (!doc) return;
    if (this.activeGroupId > 0 && doc.id) {
      this.groupService.downloadGroupDocument(this.activeGroupId, doc.id).subscribe({
        next: (blob: Blob) => {
          const blobUrl = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = blobUrl;

          let fileName = doc.name || 'tai-lieu';
          if (doc.ext && !fileName.toLowerCase().endsWith('.' + doc.ext.toLowerCase())) {
            fileName += '.' + doc.ext.toLowerCase();
          }
          a.download = fileName;

          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(blobUrl);
        },
        error: (err) => {
          console.error('Error downloading document:', err);
          alert('Không thể tải tài liệu về máy. Vui lòng thử lại sau!');
        }
      });
    } else {
      alert('Tài liệu mẫu không hỗ trợ tải về trực tiếp.');
    }
  }

  constructor(
    private groupService: GroupService,
    private chatService: ChatService,
    private chatSignalRService: ChatSignalRService
  ) {}

  get activeGroup(): GroupItem {
    return this.groupsList.find(g => g.id === this.activeGroupId) || this.groupsList[0] || {
      id: 0,
      name: 'Chưa chọn nhóm',
      code: 'NONE',
      iconBg: 'bg-purple-100',
      iconColor: 'text-purple-600',
      leader: 'N/A',
      description: 'Chưa có thông tin nhóm',
      membersCount: 0
    };
  }

  ngOnInit(): void {
    // 1. Initialize SignalR Connection
    this.chatSignalRService.startConnection();

    // 2. Subscribe to incoming SignalR messages
    this.subscriptions.add(
      this.chatSignalRService.message$.subscribe((dto: TinNhanDto) => {
        if (dto.maNhom === this.activeGroupId) {
          this.appendRealtimeMessage(dto);
        }
      })
    );

    // 3. Load my study groups from Backend
    this.loadMyGroups();
  }

  ngOnDestroy(): void {
    if (this.activeGroupId > 0) {
      this.chatSignalRService.leaveGroupChat(this.activeGroupId);
    }
    this.chatSignalRService.stopConnection();
    this.subscriptions.unsubscribe();
  }

  // Modal states & models
  showCreateModal: boolean = false;
  showJoinModal: boolean = false;
  createLoading: boolean = false;
  joinLoading: boolean = false;
  modalError: string = '';

  createForm = {
    tenNhom: '',
    moTa: '',
    maMonHoc: null as number | null,
    soLuongToiDa: 20,
    quyenRiengTu: 'Công khai',
    maThamGia: '',
    allowMemberInvite: true,
    requireApproval: false
  };

  joinCodeInput: string = '';

  generateNewGroupCode(): void {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let result = '';
    for (let i = 0; i < 8; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this.createForm.maThamGia = result;
  }

  copyGroupCode(): void {
    if (this.createForm.maThamGia) {
      try {
        navigator.clipboard.writeText(this.createForm.maThamGia);
      } catch (e) {
        console.warn('Clipboard write error', e);
      }
    }
  }

  incrementMaxMembers(): void {
    if (!this.createForm.soLuongToiDa) this.createForm.soLuongToiDa = 20;
    if (this.createForm.soLuongToiDa < 100) {
      this.createForm.soLuongToiDa++;
    }
  }

  decrementMaxMembers(): void {
    if (!this.createForm.soLuongToiDa) this.createForm.soLuongToiDa = 20;
    if (this.createForm.soLuongToiDa > 2) {
      this.createForm.soLuongToiDa--;
    }
  }

  get hasGroup(): boolean {
    return this.groupsList && this.groupsList.length > 0;
  }

  loadMyGroups(targetGroupId?: number): void {
    this.loadingGroups = true;
    this.errorMessage = '';

    this.groupService.getMyGroups().subscribe({
      next: (groups: NhomHocTapDto[]) => {
        this.loadingGroups = false;
        this.rawGroups = groups;

        const iconStyles = [
          { bg: 'bg-purple-100', color: 'text-purple-600' },
          { bg: 'bg-emerald-100', color: 'text-emerald-600' },
          { bg: 'bg-amber-100', color: 'text-amber-600' },
          { bg: 'bg-rose-100', color: 'text-rose-600' },
          { bg: 'bg-blue-100', color: 'text-blue-600' }
        ];

        this.groupsList = groups.map((g, idx) => ({
          id: g.maNhom,
          name: g.tenNhom,
          code: g.maThamGia || `GRP00${g.maNhom}`,
          iconBg: iconStyles[idx % iconStyles.length].bg,
          iconColor: iconStyles[idx % iconStyles.length].color,
          isActive: false,
          leader: g.tenNguoiTao || 'Nhóm trưởng',
          description: g.moTa || 'Nhóm học tập thông minh 📚',
          membersCount: g.soThanhVienHienTai || 1,
          leaderId: g.maNguoiTao,
          leaderName: g.tenNguoiTao || 'Nhóm trưởng',
          leaderEmail: `user${g.maNguoiTao}@studyhub.com`,
          isOwner: g.isOwner
        }));

        if (this.groupsList.length > 0) {
          const selectId = targetGroupId && this.groupsList.some(g => g.id === targetGroupId)
            ? targetGroupId
            : (this.activeGroupId > 0 && this.groupsList.some(g => g.id === this.activeGroupId) ? this.activeGroupId : this.groupsList[0].id);
          this.selectGroup(selectId);
        } else {
          this.activeGroupId = 0;
        }
      },
      error: (err) => {
        this.loadingGroups = false;
        console.error('Error fetching groups:', err);
        this.errorMessage = err?.error?.message || 'Không thể tải danh sách nhóm học tập.';
      }
    });
  }

  openCreateModal(): void {
    this.modalError = '';
    this.createForm = {
      tenNhom: '',
      moTa: '',
      maMonHoc: null,
      soLuongToiDa: 20,
      quyenRiengTu: 'Công khai',
      maThamGia: '',
      allowMemberInvite: true,
      requireApproval: false
    };
    this.generateNewGroupCode();
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  submitCreateGroup(): void {
    if (!this.createForm.tenNhom.trim()) {
      this.modalError = 'Vui lòng nhập tên nhóm học tập!';
      return;
    }
    if (this.createForm.tenNhom.trim().length < 3) {
      this.modalError = 'Tên nhóm phải có ít nhất 3 ký tự!';
      return;
    }

    this.createLoading = true;
    this.modalError = '';

    const request: CreateStudyGroupRequest = {
      tenNhom: this.createForm.tenNhom.trim(),
      moTa: this.createForm.moTa ? this.createForm.moTa.trim() : undefined,
      maMonHoc: this.createForm.maMonHoc || undefined,
      soLuongToiDa: this.createForm.soLuongToiDa || 20
    };

    this.groupService.createGroup(request).subscribe({
      next: (newGroup: NhomHocTapDto) => {
        this.createLoading = false;
        this.showCreateModal = false;
        this.loadMyGroups(newGroup.maNhom);
      },
      error: (err) => {
        this.createLoading = false;
        console.error('Error creating group:', err);
        this.modalError = err?.error?.message || 'Không thể tạo nhóm. Vui lòng thử lại!';
      }
    });
  }

  openJoinModal(): void {
    this.modalError = '';
    this.joinCodeInput = '';
    this.showJoinModal = true;
  }

  closeJoinModal(): void {
    this.showJoinModal = false;
  }

  submitJoinGroup(): void {
    if (!this.joinCodeInput.trim()) {
      this.modalError = 'Vui lòng nhập mã tham gia nhóm!';
      return;
    }

    this.joinLoading = true;
    this.modalError = '';

    this.groupService.joinGroup(this.joinCodeInput.trim()).subscribe({
      next: (group: NhomHocTapDto) => {
        this.joinLoading = false;
        this.showJoinModal = false;
        this.loadMyGroups(group.maNhom);
      },
      error: (err) => {
        this.joinLoading = false;
        console.error('Error joining group:', err);
        this.modalError = err?.error?.message || 'Mã tham gia nhóm không chính xác hoặc bạn đã ở trong nhóm này.';
      }
    });
  }

  promptCreateGroup(): void {
    this.openCreateModal();
  }

  promptJoinGroup(): void {
    this.openJoinModal();
  }

  selectGroup(id: number): void {
    if (this.activeGroupId > 0) {
      this.chatSignalRService.leaveGroupChat(this.activeGroupId);
    }

    this.activeGroupId = id;
    this.groupsList.forEach(g => g.isActive = (g.id === id));
    this.showGroupDropdown = false;

    // Join SignalR group room
    this.chatSignalRService.joinGroupChat(id);

    // Fetch members, messages, tasks, meetings, folders, and documents
    this.loadGroupMembers(id);
    this.loadGroupMessages(id);
    this.loadPinnedAnnouncement(id);
    this.loadGroupTasksFromBackend(id);
    this.loadGroupMeetings(id);
    this.loadGroupFolders(id);
    this.loadGroupDocuments(id);
  }

  switchTab(tab: 'overview' | 'chat' | 'docs' | 'tasks' | 'meetings' | 'members' | 'settings'): void {
    this.activeTab = tab;
    if (tab === 'docs' && this.activeGroupId > 0) {
      this.loadGroupFolders(this.activeGroupId);
      this.loadGroupDocuments(this.activeGroupId, this.selectedFolderId);
    } else if (tab === 'settings' && this.activeGroup) {
      this.editGroupForm.tenNhom = this.activeGroup.name || '';
      this.editGroupForm.moTa = this.activeGroup.description || '';
    }
  }

  loadGroupFolders(groupId: number): void {
    if (!groupId) return;
    this.groupService.getGroupFolders(groupId).subscribe({
      next: (folders) => {
        const allFolder = { id: 0, name: 'Tất cả tài liệu', filesCount: 0, isAll: true };
        const mapped = (folders || []).map(f => ({
          id: f.maThuMuc ?? f.MaThuMuc,
          name: f.tenThuMuc || f.TenThuMuc || 'Thư mục không tên',
          filesCount: f.soLuongFile ?? f.SoLuongFile ?? 0
        }));
        const sumFiles = mapped.reduce((acc, curr) => acc + curr.filesCount, 0);
        allFolder.filesCount = Math.max(sumFiles, this.docsList ? this.docsList.length : 0);
        this.foldersList = [allFolder, ...mapped];
      },
      error: (err) => {
        console.error('Error loading folders:', err.error || err);
      }
    });
  }

  loadGroupDocuments(groupId: number, folderId?: number): void {
    if (!groupId) {
      this.docsList = [];
      return;
    }
    this.groupService.getGroupDocuments(groupId, folderId).subscribe({
      next: (docs) => {
        this.docsList = (docs || []).map(d => {
          const extStr = (d.extension || 'DOC').replace('.', '').toUpperCase();
          let extBg = 'bg-blue-500';
          if (['PPT', 'PPTX'].includes(extStr)) extBg = 'bg-orange-500';
          else if (['PDF'].includes(extStr)) extBg = 'bg-rose-500';
          else if (['XLS', 'XLSX', 'CSV'].includes(extStr)) extBg = 'bg-emerald-500';
          else if (['ZIP', 'RAR', '7Z'].includes(extStr)) extBg = 'bg-purple-500';
          else if (['SQL', 'JSON', 'JS', 'TS'].includes(extStr)) extBg = 'bg-pink-500';

          const sizeStr = d.dungLuong 
            ? (d.dungLuong < 1024 * 1024 ? (d.dungLuong / 1024).toFixed(1) + ' KB' : (d.dungLuong / (1024 * 1024)).toFixed(1) + ' MB') 
            : '1.5 MB';

          return {
            id: d.maTaiLieu,
            name: d.tieuDe,
            ext: extStr.substring(0, 4),
            extBg: extBg,
            size: sizeStr,
            uploaderName: d.tenNguoiTaiLen || 'Thành viên',
            uploaderAvatar: d.avatarNguoiTaiLen || '',
            updatedAt: d.ngayTaiLen ? new Date(d.ngayTaiLen).toLocaleDateString('vi-VN') : 'Gần đây',
            folderId: d.maThuMuc || 0
          };
        });

        if (this.foldersList && this.foldersList.length > 0 && this.foldersList[0].isAll && (!folderId || folderId === 0)) {
          this.foldersList[0].filesCount = this.docsList.length;
        }
      },
      error: (err) => {
        console.error('Error loading documents:', err.error || err);
        this.docsList = [];
      }
    });
  }

  loadGroupMeetings(groupId: number): void {
    if (!groupId) return;
    this.groupService.getGroupMeetings(groupId).subscribe({
      next: (meetings) => {
        this.groupMeetings = meetings;
        this.generateCalendar();
        this.filterMeetingsByDate();
      },
      error: (err) => {
        console.error('Error loading group meetings:', err);
        this.groupMeetings = [];
        this.generateCalendar();
        this.filterMeetingsByDate();
      }
    });
  }

  loadingTasks: boolean = false;

  loadGroupTasksFromBackend(groupId: number): void {
    if (!groupId) return;
    this.loadingTasks = true;
    this.groupService.getGroupTasks(groupId).subscribe({
      next: (dtos: GroupTaskBackendDto[]) => {
        this.loadingTasks = false;
        this.kanbanTasks = (dtos || []).map(d => {
          let priorityLabel: 'Cao' | 'Trung bình' | 'Thấp' = 'Trung bình';
          let priorityClass = 'bg-amber-100 text-amber-600';
          if (d.doUuTien === 2) {
            priorityLabel = 'Cao';
            priorityClass = 'bg-rose-100 text-rose-500';
          } else if (d.doUuTien === 0) {
            priorityLabel = 'Thấp';
            priorityClass = 'bg-emerald-100 text-emerald-600';
          }

          let column: 'todo' | 'inProgress' | 'done' = 'todo';
          if (d.trangThai === 1) column = 'inProgress';
          else if (d.trangThai === 3) column = 'done';

          return {
            id: d.maCongViec,
            title: d.tieuDe,
            description: d.moTa || '',
            labels: [],
            checklist: [],
            comments: [],
            assigneeName: d.tenNguoiDuocGiao || d.tenNguoiTao || 'Chưa phân công',
            assigneeAvatar: d.anhNguoiDuocGiao || d.anhNguoiTao || '',
            dueDate: d.hanHoanThanh ? d.hanHoanThanh.split('T')[0] : '',
            priority: priorityLabel,
            priorityClass: priorityClass,
            completed: d.trangThai === 3,
            column: column
          } as KanbanTask;
        });
      },
      error: (err) => {
        this.loadingTasks = false;
        console.error('Error loading group tasks:', err);
      }
    });
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(' ').filter(p => !!p);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, Math.min(2, name.length)).toUpperCase();
  }

  getAvatarBgColor(name: string): string {
    const colors = [
      'bg-purple-100 text-[#5B4DFF]',
      'bg-indigo-100 text-indigo-600',
      'bg-rose-100 text-rose-600',
      'bg-blue-100 text-blue-600',
      'bg-emerald-100 text-emerald-600',
      'bg-amber-100 text-amber-600'
    ];
    let hash = 0;
    for (let i = 0; i < (name || '').length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const idx = Math.abs(hash) % colors.length;
    return colors[idx];
  }

  rawMembers: ThanhVienNhomDto[] = [];

  loadGroupMembers(groupId: number): void {
    this.loadingMembers = true;
    this.groupService.getMembers(groupId).subscribe({
      next: (members: ThanhVienNhomDto[]) => {
        this.loadingMembers = false;
        this.rawMembers = members;

        this.membersList = members.map((m) => ({
          name: m.hoTen || m.email,
          role: m.vaiTro === 2 ? 'Nhóm trưởng' : m.vaiTro === 1 ? 'Quản trị viên' : 'Thành viên',
          avatar: m.avatar || '',
          status: 'Online',
          statusClass: 'text-emerald-500'
        }));
      },
      error: (err) => {
        this.loadingMembers = false;
        console.error('Error fetching members:', err);
      }
    });
  }

  // ═══════════════════════════════════════════════
  // CHAT: PINNED ANNOUNCEMENT & FILE ATTACHMENTS
  // ═══════════════════════════════════════════════
  pinnedAnnouncement: string = 'Họp nhóm vào 20:00 tối nay để thống nhất giao diện!';
  showEditPinModal: boolean = false;
  newPinText: string = '';
  savingPin: boolean = false;

  selectedChatFile: File | null = null;
  selectedChatFilePreview: { name: string; size: string; ext: string } | null = null;
  uploadingChatFile: boolean = false;

  formatChatFileSize(bytes?: number): string {
    if (!bytes) return '0 B';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  loadPinnedAnnouncement(groupId: number): void {
    if (!groupId) return;
    this.chatService.getPinnedAnnouncement(groupId).subscribe({
      next: (res) => {
        if (res?.announcement) {
          this.pinnedAnnouncement = res.announcement;
        }
      },
      error: (err) => {
        console.warn('Could not load pinned announcement:', err);
      }
    });
  }

  openEditPinModal(): void {
    this.newPinText = this.pinnedAnnouncement;
    this.showEditPinModal = true;
  }

  closeEditPinModal(): void {
    this.showEditPinModal = false;
  }

  savePinnedAnnouncement(): void {
    if (!this.newPinText.trim() || !this.activeGroupId) return;
    this.savingPin = true;
    this.chatService.updatePinnedAnnouncement(this.activeGroupId, this.newPinText.trim()).subscribe({
      next: (res) => {
        this.savingPin = false;
        this.pinnedAnnouncement = this.newPinText.trim();
        this.showEditPinModal = false;
      },
      error: (err) => {
        this.savingPin = false;
        console.error('Error updating pinned announcement:', err);
        // Optimistically update
        this.pinnedAnnouncement = this.newPinText.trim();
        this.showEditPinModal = false;
      }
    });
  }

  loadGroupMessages(groupId: number): void {
    this.loadingMessages = true;
    this.chatService.getGroupMessages(groupId).subscribe({
      next: (messages: TinNhanDto[]) => {
        this.loadingMessages = false;

        this.chatMessages = messages.map(msg => ({
          id: msg.maTinNhan,
          senderName: msg.tenNguoiGui || 'Thành viên',
          senderAvatar: msg.avatarNguoiGui || '',
          time: new Date(msg.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
          content: msg.noiDung,
          isMe: msg.isMine,
          loaiTinNhan: msg.loaiTinNhan,
          attachment: msg.attachment ? {
            fileName: msg.attachment.tenFile,
            fileSize: this.formatChatFileSize(msg.attachment.dungLuong),
            fileUrl: msg.attachment.duongDan.startsWith('http') ? msg.attachment.duongDan : 'http://localhost:5186' + msg.attachment.duongDan,
            ext: msg.attachment.dinhDang
          } : undefined
        }));
      },
      error: (err) => {
        this.loadingMessages = false;
        console.error('Error fetching chat messages:', err);
      }
    });
  }

  appendRealtimeMessage(msg: TinNhanDto): void {
    if (!this.chatMessages.some(m => m.id === msg.maTinNhan)) {
      this.chatMessages.push({
        id: msg.maTinNhan,
        senderName: msg.tenNguoiGui || 'Thành viên',
        senderAvatar: msg.avatarNguoiGui || '',
        time: new Date(msg.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        content: msg.noiDung,
        isMe: msg.isMine,
        loaiTinNhan: msg.loaiTinNhan,
        attachment: msg.attachment ? {
          fileName: msg.attachment.tenFile,
          fileSize: this.formatChatFileSize(msg.attachment.dungLuong),
          fileUrl: msg.attachment.duongDan.startsWith('http') ? msg.attachment.duongDan : 'http://localhost:5186' + msg.attachment.duongDan,
          ext: msg.attachment.dinhDang
        } : undefined
      });
      this.shouldScrollToBottom = true;
    }
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  scrollToBottom(): void {
    try {
      if (this.chatScrollContainer?.nativeElement) {
        const el = this.chatScrollContainer.nativeElement;
        el.scrollTop = el.scrollHeight;
      }
    } catch (e) {}
  }

  attachFile(): void {
    const fileInput = document.getElementById('chatFileInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
  }

  onChatFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (!file) return;

    this.selectedChatFile = file;
    const ext = file.name.split('.').pop()?.toUpperCase() || 'FILE';
    this.selectedChatFilePreview = {
      name: file.name,
      size: this.formatChatFileSize(file.size),
      ext: ext
    };
    event.target.value = '';
  }

  removeSelectedChatFile(): void {
    this.selectedChatFile = null;
    this.selectedChatFilePreview = null;
  }

  downloadChatFile(url?: string, fileName?: string): void {
    const targetFileName = fileName || 'tai-lieu';
    let targetUrl = url;
    if (!targetUrl) {
      targetUrl = `/uploads/chat/${targetFileName}`;
    }

    const fullUrl = targetUrl.startsWith('http') 
      ? targetUrl 
      : `http://localhost:5186${targetUrl.startsWith('/') ? '' : '/'}${targetUrl}`;

    fetch(fullUrl)
      .then(res => {
        if (!res.ok) throw new Error('Không thể tải file trực tiếp từ server');
        return res.blob();
      })
      .then(blob => {
        const blobUrl = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = blobUrl;
        a.download = targetFileName;
        document.body.appendChild(a);
        a.click();
        setTimeout(() => {
          document.body.removeChild(a);
          window.URL.revokeObjectURL(blobUrl);
        }, 500);
      })
      .catch(err => {
        console.warn('Fetch blob download fallback to direct link:', err);
        const a = document.createElement('a');
        a.href = fullUrl;
        a.download = targetFileName;
        a.target = '_blank';
        document.body.appendChild(a);
        a.click();
        setTimeout(() => {
          document.body.removeChild(a);
        }, 300);
      });
  }

  getCurrentTime(): string {
    const now = new Date();
    return now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  }

  sendMessage(): void {
    const content = this.newMessageText.trim();
    if (!content && !this.selectedChatFile) return;

    const fileToSend = this.selectedChatFile;
    const filePreview = this.selectedChatFilePreview;
    this.newMessageText = '';
    this.removeSelectedChatFile();

    // --- MOCK: Optimistically add message to UI immediately ---
    const mockMsg: ChatMessage = {
      id: Date.now(),
      senderName: 'Bạn',
      senderAvatar: '',
      time: this.getCurrentTime(),
      content: content || (fileToSend ? fileToSend.name : ''),
      isMe: true,
      loaiTinNhan: fileToSend ? 2 : 0,
      attachment: filePreview ? {
        fileName: filePreview.name,
        fileSize: filePreview.size,
        ext: filePreview.ext
      } : undefined
    };
    this.chatMessages.push(mockMsg);
    this.shouldScrollToBottom = true;

    // --- API: Send File or Text Message ---
    if (this.activeGroupId > 0) {
      if (fileToSend) {
        this.uploadingChatFile = true;
        this.chatService.uploadFileMessage(this.activeGroupId, fileToSend, content).subscribe({
          next: (msgDto: TinNhanDto) => {
            this.uploadingChatFile = false;
            const idx = this.chatMessages.findIndex(m => m.id === mockMsg.id);
            if (idx !== -1) {
              this.chatMessages[idx] = {
                id: msgDto.maTinNhan,
                senderName: msgDto.tenNguoiGui || 'Bạn',
                senderAvatar: msgDto.avatarNguoiGui || '',
                time: new Date(msgDto.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
                content: msgDto.noiDung,
                isMe: true,
                loaiTinNhan: msgDto.loaiTinNhan,
                attachment: msgDto.attachment ? {
                  fileName: msgDto.attachment.tenFile,
                  fileSize: this.formatChatFileSize(msgDto.attachment.dungLuong),
                  fileUrl: msgDto.attachment.duongDan.startsWith('http') ? msgDto.attachment.duongDan : 'http://localhost:5186' + msgDto.attachment.duongDan,
                  ext: msgDto.attachment.dinhDang
                } : mockMsg.attachment
              };
            }

            // Sync to Documents tab in background
            this.loadGroupFolders(this.activeGroupId);
            this.loadGroupDocuments(this.activeGroupId);
          },
          error: (err) => {
            this.uploadingChatFile = false;
            console.warn('File message saved locally only (API error):', err?.message);
          }
        });
      } else {
        this.chatSignalRService.sendMessage(this.activeGroupId, content);

        this.chatService.sendMessage(this.activeGroupId, {
          maNhom: this.activeGroupId,
          noiDung: content,
          loaiTinNhan: 0
        }).subscribe({
          next: (msgDto: TinNhanDto) => {
            const idx = this.chatMessages.findIndex(m => m.id === mockMsg.id);
            if (idx !== -1) {
              this.chatMessages[idx] = {
                id: msgDto.maTinNhan,
                senderName: msgDto.tenNguoiGui || 'Bạn',
                senderAvatar: msgDto.avatarNguoiGui || '',
                time: new Date(msgDto.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
                content: msgDto.noiDung,
                isMe: true
              };
            }
          },
          error: (err) => {
            console.warn('Message saved locally only (API error):', err?.message);
          }
        });
      }
    }
  }

  leaveCurrentGroup(): void {
    if (!this.activeGroupId || this.activeGroupId <= 0) return;

    const groupName = this.activeGroup ? this.activeGroup.name : 'nhóm này';
    const isOwner = this.activeGroup ? this.activeGroup.isOwner : false;
    const confirmMsg = isOwner
      ? `Bạn là Trưởng nhóm. Nếu bạn rời khỏi nhóm, nhóm "${groupName}" có thể bị giải tán. Bạn có chắc chắn muốn rời nhóm không?`
      : `Bạn có chắc chắn muốn rời khỏi nhóm "${groupName}" không?`;

    if (confirm(confirmMsg)) {
      this.loadingGroups = true;
      this.groupService.leaveGroup(this.activeGroupId).subscribe({
        next: () => {
          if (this.activeGroupId > 0) {
            this.chatSignalRService.leaveGroupChat(this.activeGroupId);
          }
          this.activeGroupId = 0;
          this.loadMyGroups();
        },
        error: (err) => {
          this.loadingGroups = false;
          console.error('Error leaving group:', err);
          const msg = err?.error?.message || 'Không thể rời nhóm. Vui lòng thử lại!';
          alert(msg);
        }
      });
    }
  }

  toggleLeftSidebar(): void {
    this.isLeftSidebarCollapsed = !this.isLeftSidebarCollapsed;
  }

  toggleRightSidebar(): void {
    this.isRightSidebarCollapsed = !this.isRightSidebarCollapsed;
  }

  taskModalError: string = '';
  taskSubmitLoading: boolean = false;
  editingTaskId: number | null = null;

  openTaskModal(status: 'todo' | 'inProgress' | 'review' | 'done' = 'todo', task?: KanbanTask): void {
    this.taskModalError = '';
    this.taskSubmitLoading = false;
    if (task) {
      this.editingTaskId = task.id;
      this.newTaskForm = { 
        title: task.title, 
        description: task.description, 
        assigneeName: task.assigneeName === 'Chưa phân công' ? '' : task.assigneeName, 
        priority: task.priority as any, 
        dueDate: task.dueDate, 
        status: task.column as any 
      };
    } else {
      this.editingTaskId = null;
      this.newTaskForm = { title: '', description: '', assigneeName: '', priority: 'Trung bình', dueDate: '', status: status };
    }
    this.showTaskModal = true;
    this.showStatusDropdown = false;
    this.showPriorityDropdown = false;
    this.showAssigneeDropdown = false;
  }

  showStatusDropdown: boolean = false;

  getStatusText(status: string): string {
    if (status === 'todo') return 'Cần thực hiện';
    if (status === 'inProgress') return 'Đang thực hiện';
    if (status === 'done') return 'Hoàn thành';
    return status;
  }

  getStatusClass(status: string): string {
    if (status === 'todo') return 'bg-[#EEF2FF] text-[#4F46E5]';
    if (status === 'inProgress') return 'bg-[#FEF3C7] text-[#D97706]';
    if (status === 'done') return 'bg-[#D1FAE5] text-[#059669]';
    return 'bg-gray-100 text-gray-600';
  }

  selectStatus(status: 'todo' | 'inProgress' | 'done'): void {
    this.newTaskForm.status = status;
    this.showStatusDropdown = false;
  }

  showPriorityDropdown: boolean = false;
  showAssigneeDropdown: boolean = false;

  selectPriority(p: 'Cao' | 'Trung bình' | 'Thấp'): void {
    this.newTaskForm.priority = p;
    this.showPriorityDropdown = false;
  }

  selectAssignee(name: string): void {
    this.newTaskForm.assigneeName = name;
    this.showAssigneeDropdown = false;
  }

  getAssigneeAvatar(name: string): string {
    const member = this.membersList.find(m => m.name === name);
    return member?.avatar || '';
  }

  closeTaskModal(): void {
    this.showTaskModal = false;
    this.taskModalError = '';
    this.taskSubmitLoading = false;
  }

  getPriorityClass(p: string): string {
    if (p === 'Cao') return 'bg-rose-100 text-rose-600';
    if (p === 'Thấp') return 'bg-blue-100 text-blue-600';
    return 'bg-amber-100 text-amber-600';
  }

  // ──── FILTER ────
  getFilteredTasks(colId: string): KanbanTask[] {
    return this.kanbanTasks.filter(t => {
      if (t.column !== colId) return false;
      if (this.taskFilter.search && !t.title.toLowerCase().includes(this.taskFilter.search.toLowerCase())) return false;
      if (this.taskFilter.priority && t.priority !== this.taskFilter.priority) return false;
      if (this.taskFilter.assignee && t.assigneeName !== this.taskFilter.assignee) return false;
      return true;
    });
  }

  clearFilters(): void {
    this.taskFilter = { search: '', priority: '', assignee: '' };
  }

  get hasActiveFilter(): boolean {
    return !!(this.taskFilter.search || this.taskFilter.priority || this.taskFilter.assignee);
  }

  get totalTaskCount(): number { return this.kanbanTasks.length; }
  get doneTaskCount(): number { return this.kanbanTasks.filter(t => t.column === 'done').length; }

  // ──── CARD DETAIL ────
  openCard(task: KanbanTask): void {
    this.editingCard = {
      ...task,
      labels: task.labels.map(l => ({...l})),
      checklist: task.checklist.map(i => ({...i})),
      comments: [...task.comments]
    };
    this.showCardDetail = true;
    this.newChecklistText = '';
    this.newCommentText = '';
    this.showLabelPicker = false;
  }

  closeCard(): void {
    this.showCardDetail = false;
    this.editingCard = null;
    this.showLabelPicker = false;
  }

  saveCard(): void {
    if (!this.editingCard) return;
    const idx = this.kanbanTasks.findIndex(t => t.id === this.editingCard!.id);
    if (idx !== -1) this.kanbanTasks[idx] = { ...this.editingCard };
    this.closeCard();
  }

  deleteCurrentCard(): void {
    if (!this.editingCard) return;
    this.kanbanTasks = this.kanbanTasks.filter(t => t.id !== this.editingCard!.id);
    this.closeCard();
  }

  moveCurrentCard(col: 'todo' | 'inProgress' | 'review' | 'done'): void {
    if (!this.editingCard) return;
    this.editingCard.column = col;
    this.editingCard.completed = col === 'done';
  }

  // ──── CHECKLIST ────
  addChecklistItem(): void {
    if (!this.newChecklistText.trim() || !this.editingCard) return;
    this.editingCard.checklist.push({ id: Date.now(), text: this.newChecklistText.trim(), done: false });
    this.newChecklistText = '';
  }

  removeChecklistItem(itemId: number): void {
    if (!this.editingCard) return;
    this.editingCard.checklist = this.editingCard.checklist.filter(i => i.id !== itemId);
  }

  getChecklistProgress(task: KanbanTask): number {
    if (!task.checklist?.length) return 0;
    return Math.round((task.checklist.filter(i => i.done).length / task.checklist.length) * 100);
  }

  // ──── LABELS ────
  toggleLabel(label: KanbanLabel): void {
    if (!this.editingCard) return;
    const idx = this.editingCard.labels.findIndex(l => l.id === label.id);
    if (idx !== -1) this.editingCard.labels.splice(idx, 1);
    else this.editingCard.labels.push({ ...label });
  }

  hasLabel(label: KanbanLabel): boolean {
    return !!this.editingCard?.labels.find(l => l.id === label.id);
  }

  // ──── COMMENTS ────
  addComment(): void {
    if (!this.newCommentText.trim() || !this.editingCard) return;
    const comment: KanbanComment = {
      id: Date.now(),
      text: this.newCommentText.trim(),
      author: 'Bạn',
      avatar: '',
      time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
    };
    this.editingCard.comments.push(comment);
    this.newCommentText = '';
  }

  // ──── QUICK ADD ────
  startQuickAdd(colId: string): void {
    this.quickAddCol = colId;
    this.quickAddTitle = '';
  }

  submitQuickAdd(): void {
    if (!this.quickAddTitle.trim()) { this.cancelQuickAdd(); return; }
    const defaultAvatar = '';
    this.kanbanTasks.push({
      id: Date.now(),
      title: this.quickAddTitle.trim(),
      description: '',
      labels: [],
      checklist: [],
      comments: [],
      assigneeName: 'Chưa phân công',
      assigneeAvatar: defaultAvatar,
      dueDate: '',
      priority: 'Trung bình',
      priorityClass: 'bg-amber-100 text-amber-600',
      completed: false,
      column: this.quickAddCol as any
    });
    this.cancelQuickAdd();
  }

  cancelQuickAdd(): void {
    this.quickAddCol = '';
    this.quickAddTitle = '';
  }

  // ──── INLINE MOVE ────
  moveCard(task: KanbanTask, to: 'todo' | 'inProgress' | 'review' | 'done'): void {
    const t = this.kanbanTasks.find(k => k.id === task.id);
    if (t) { t.column = to; t.completed = to === 'done'; }

    if (this.activeGroupId && task.id) {
      let statusByte = 0;
      if (to === 'inProgress') statusByte = 1;
      else if (to === 'done') statusByte = 3;

      this.groupService.updateGroupTaskStatus(this.activeGroupId, task.id, statusByte).subscribe({
        error: (err) => {
          console.error('Error updating task status on server:', err);
        }
      });
    }
  }

  moveTask(task: KanbanTask, from: string, to: 'todo' | 'inProgress' | 'review' | 'done'): void {
    this.moveCard(task, to);
  }

  // ──── DUE DATE HELPERS ────
  isDueOverdue(dueDate: string): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date(new Date().toDateString());
  }

  isDueSoon(dueDate: string): boolean {
    if (!dueDate) return false;
    const diff = new Date(dueDate).getTime() - new Date().getTime();
    return diff > 0 && diff < 3 * 24 * 60 * 60 * 1000;
  }

  formatDueDate(dueDate: string): string {
    if (!dueDate) return '';
    return new Date(dueDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
  }

  // ──── SUBMIT ADD TASK (from modal) ────
  submitAddTask(): void {
    if (!this.newTaskForm.title.trim()) {
      this.taskModalError = 'Vui lòng nhập tên công việc!';
      return;
    }

    this.taskSubmitLoading = true;
    this.taskModalError = '';

    if (this.editingTaskId) {
      // Mock update locally since backend doesn't have an update endpoint for full task details yet
      const taskIndex = this.kanbanTasks.findIndex(t => t.id === this.editingTaskId);
      if (taskIndex !== -1) {
        const matchedMember = this.membersList.find(m => m.name === this.newTaskForm.assigneeName);
        const avatar = matchedMember?.avatar || '';
        
        this.kanbanTasks[taskIndex] = {
          ...this.kanbanTasks[taskIndex],
          title: this.newTaskForm.title.trim(),
          description: this.newTaskForm.description || '',
          assigneeName: this.newTaskForm.assigneeName || 'Chưa phân công',
          assigneeAvatar: avatar,
          dueDate: this.newTaskForm.dueDate || '',
          priority: this.newTaskForm.priority,
          priorityClass: this.getPriorityClass(this.newTaskForm.priority),
          column: this.newTaskForm.status,
          completed: this.newTaskForm.status === 'done'
        };
      }
      this.taskSubmitLoading = false;
      this.showTaskModal = false;
      this.editingTaskId = null;
      return;
    }

    if (this.activeGroupId) {
      let doUuTienByte = 1; // Trung binh
      if (this.newTaskForm.priority === 'Cao') doUuTienByte = 2;
      else if (this.newTaskForm.priority === 'Thấp') doUuTienByte = 0;

      let assignedUserId: number | undefined = undefined;
      if (this.newTaskForm.assigneeName && this.rawMembers && this.rawMembers.length > 0) {
        const found = this.rawMembers.find(m => (m.hoTen || m.email) === this.newTaskForm.assigneeName);
        if (found) assignedUserId = found.maNguoiDung;
      }

      let statusByte = 0;
      if (this.newTaskForm.status === 'inProgress') statusByte = 1;
      else if (this.newTaskForm.status === 'review') statusByte = 2; // Defaulting review to 2
      else if (this.newTaskForm.status === 'done') statusByte = 3;

      const req: CreateGroupTaskBackendRequest = {
        tieuDe: this.newTaskForm.title.trim(),
        moTa: this.newTaskForm.description || '',
        doUuTien: doUuTienByte,
        hanHoanThanh: this.newTaskForm.dueDate ? new Date(this.newTaskForm.dueDate).toISOString() : undefined,
        maNguoiDuocGiao: assignedUserId,
        trangThai: statusByte
      };

      this.groupService.createGroupTask(this.activeGroupId, req).subscribe({
        next: (d: GroupTaskBackendDto) => {
          this.taskSubmitLoading = false;
          this.showTaskModal = false;

          let priorityLabel: 'Cao' | 'Trung bình' | 'Thấp' = 'Trung bình';
          let priorityClass = 'bg-amber-100 text-amber-600';
          if (d.doUuTien === 2) {
            priorityLabel = 'Cao';
            priorityClass = 'bg-rose-100 text-rose-500';
          } else if (d.doUuTien === 0) {
            priorityLabel = 'Thấp';
            priorityClass = 'bg-emerald-100 text-emerald-600';
          }

          let column: 'todo' | 'inProgress' | 'done' = 'todo';
          if (d.trangThai === 1) column = 'inProgress';
          else if (d.trangThai === 3) column = 'done';

          const newTask: KanbanTask = {
            id: d.maCongViec,
            title: d.tieuDe,
            description: d.moTa || '',
            labels: [],
            checklist: [],
            comments: [],
            assigneeName: d.tenNguoiDuocGiao || d.tenNguoiTao || 'Chưa phân công',
            assigneeAvatar: d.anhNguoiDuocGiao || d.anhNguoiTao || '',
            dueDate: d.hanHoanThanh ? d.hanHoanThanh.split('T')[0] : '',
            priority: priorityLabel,
            priorityClass: priorityClass,
            completed: d.trangThai === 3,
            column: column
          };

          this.kanbanTasks = [newTask, ...this.kanbanTasks];
        },
        error: (err) => {
          this.taskSubmitLoading = false;
          console.error('Error creating group task on server:', err);
          this.taskModalError = err?.error?.message || 'Có lỗi xảy ra khi tạo công việc trên máy chủ.';
        }
      });
    } else {
      const matchedMember = this.membersList.find(m => m.name === this.newTaskForm.assigneeName);
      const avatar = matchedMember?.avatar || '';

      const newTask: KanbanTask = {
        id: Date.now(),
        title: this.newTaskForm.title.trim(),
        description: this.newTaskForm.description || '',
        labels: [],
        checklist: [],
        comments: [],
        assigneeName: this.newTaskForm.assigneeName || 'Chưa phân công',
        assigneeAvatar: avatar,
        dueDate: this.newTaskForm.dueDate || '',
        priority: this.newTaskForm.priority,
        priorityClass: this.getPriorityClass(this.newTaskForm.priority),
        completed: this.newTaskForm.status === 'done',
        column: this.newTaskForm.status
      };
      this.kanbanTasks = [newTask, ...this.kanbanTasks];
      this.taskSubmitLoading = false;
      this.showTaskModal = false;
    }
  }

  deleteTask(task: KanbanTask): void {
    if (confirm(`Bạn có chắc chắn muốn xóa công việc "${task.title}" không?`)) {
      if (this.activeGroupId && task.id > 100000) { 
        // Assuming real task IDs from DB are standard IDs, whereas mock IDs might be Date.now()
        // Wait, all tasks created have proper IDs if from DB. Let's just call API if activeGroupId is set.
        // Actually, if we have activeGroupId, let's just always call API and if it fails, fallback or just ignore.
      }
      
      if (this.activeGroupId) {
        this.groupService.deleteGroupTask(this.activeGroupId, task.id).subscribe({
          next: () => {
            this.kanbanTasks = this.kanbanTasks.filter(t => t.id !== task.id);
          },
          error: (err) => {
            console.error('Error deleting task:', err);
            // If it's a mock task that doesn't exist on server, just remove it locally
            this.kanbanTasks = this.kanbanTasks.filter(t => t.id !== task.id);
          }
        });
      } else {
        this.kanbanTasks = this.kanbanTasks.filter(t => t.id !== task.id);
      }
    }
  }

  // ──── UNIQUE ASSIGNEES (for filter) ────
  get uniqueAssignees(): string[] {
    const names = this.kanbanTasks.map(t => t.assigneeName).filter(n => n && n !== 'Chưa phân công');
    return [...new Set(names)];
  }

  // ──── DRAG AND DROP ────
  draggedTask: KanbanTask | null = null;

  onDragStart(event: DragEvent, task: KanbanTask): void {
    this.draggedTask = task;
    if (event.dataTransfer) {
      event.dataTransfer.setData('text/plain', task.id.toString());
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDrop(event: DragEvent, targetColumn: 'todo' | 'inProgress' | 'review' | 'done'): void {
    event.preventDefault();
    if (this.draggedTask) {
      this.moveCard(this.draggedTask, targetColumn);
      this.draggedTask = null;
    }
  }

  onDragEnd(): void {
    this.draggedTask = null;
  }

  toggleGroupDropdown(): void {
    this.showGroupDropdown = !this.showGroupDropdown;
  }
}
