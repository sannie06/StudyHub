import { Component, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { take } from 'rxjs/operators';
import { TaskService, TaskDto, PagedList } from '../../services/task.service';
import { SubjectService, SubjectTag } from '../../services/subject.service';

export interface TaskItem {
  id: number;
  title: string;
  desc: string;
  tag: string;
  tagClass: string;
  tagColor?: string;
  priority: string;
  priorityClass: string;
  dueDate: string;
  dueWarning: string;
  warnClass: string;
  status: string;
  statusClass: string;
  progress: number;
  progressColor: string;
}

export interface KanbanItem {
  id?: number;
  title: string;
  tag: string;
  tagClass: string;
  tagColor?: string;
  due: string;
  priority: string;
  pClass: string;
}



export interface StatusOption {
  value: string;
  label: string;
  badgeClass: string;
}

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DragDropModule],
  templateUrl: './tasks.component.html',
  styles: [`
    .donut-svg { transform: rotate(-90deg); }
    
    /* Custom Slider Styles */
    .custom-slider {
      -webkit-appearance: none;
      width: 100%;
      height: 8px;
      border-radius: 8px;
      outline: none;
    }
    .custom-slider::-webkit-slider-thumb {
      -webkit-appearance: none;
      appearance: none;
      width: 20px;
      height: 20px;
      border-radius: 50%;
      background: #ffffff;
      border: 3px solid #5B4DFF;
      cursor: pointer;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      transition: transform 0.2s;
    }
    .custom-slider::-webkit-slider-thumb:hover {
      transform: scale(1.2);
    }

    /* Angular CDK Drag & Drop Animations & Visual Styling */
    .cdk-drag-preview {
      box-shadow: 0 12px 28px -4px rgba(91, 77, 255, 0.25), 0 8px 12px -6px rgba(0, 0, 0, 0.1);
      transform: scale(1.03) rotate(1deg);
      border-radius: 0.75rem;
      border: 1px solid #818cf8;
      cursor: grabbing !important;
      opacity: 0.96;
      background-color: #ffffff;
      z-index: 1000 !important;
    }
    .cdk-drag-placeholder {
      opacity: 0.45;
      background: rgba(91, 77, 255, 0.06) !important;
      border: 2px dashed #5B4DFF !important;
      border-radius: 0.75rem !important;
      min-height: 70px;
    }
    .cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }
    .cdk-drop-list-dragging .cdk-drag:not(.cdk-drag-placeholder) {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }
    .cdk-drop-list-dragging {
      border-color: #818cf8 !important;
      background-color: rgba(91, 77, 255, 0.03) !important;
      transition: background-color 0.2s ease, border-color 0.2s ease;
    }
  `]
})
export class TasksComponent implements OnInit {
  currentView: 'list' | 'kanban' = 'list';
  showCreateTask: boolean = false;
  isEditMode: boolean = false;

  milestones = [
    { value: 0, label: 'Bắt đầu' },
    { value: 25, label: 'Lên kế hoạch' },
    { value: 50, label: 'Đang thực hiện' },
    { value: 75, label: 'Gần hoàn thành' },
    { value: 100, label: 'Hoàn thành' }
  ];

  // New task form model
  newTask = {
    title: '',
    desc: '',
    subject: '',
    priority: 'medium' as 'high' | 'medium' | 'low',
    startDate: '',
    dueDate: '',
    status: 'todo',
    progress: 0
  };

  // Custom Subject Tag Select State
  subjectTags: SubjectTag[] = [
    { id: 1, name: 'PTPM', color: '#6366F1' },
    { id: 2, name: 'Cơ sở dữ liệu', color: '#3B82F6' },
    { id: 3, name: 'Java', color: '#10B981' },
    { id: 4, name: 'Web', color: '#8B5CF6' },
    { id: 5, name: 'Kỹ năng', color: '#EC4899' },
    { id: 6, name: 'Thiết kế', color: '#F97316' },
    { id: 7, name: 'Toán', color: '#14B8A6' }
  ];

  presetColors: string[] = [
    '#EF4444', // Red
    '#F97316', // Orange
    '#F59E0B', // Amber
    '#10B981', // Emerald
    '#06B6D4', // Cyan
    '#3B82F6', // Blue
    '#6366F1', // Indigo
    '#8B5CF6', // Purple
    '#EC4899', // Pink
    '#F43F5E', // Rose
    '#84CC16', // Lime
    '#14B8A6', // Teal
    '#0284C7', // Sky
    '#4F46E5', // Dark Indigo
    '#D946EF', // Fuchsia
    '#E11D48', // Crimson
    '#D97706', // Gold
    '#059669', // Dark Emerald
    '#0891B2', // Deep Cyan
    '#4338CA', // Deep Blue
    '#7E22CE', // Deep Purple
    '#BE185D', // Deep Pink
    '#64748B', // Slate
    '#475569'  // Dark Gray
  ];

  isSubjectDropdownOpen: boolean = false;
  selectedSubjectTag: SubjectTag | null = null;
  activeMoreOptionsTagId: number | null = null;

  // Custom Status Select State
  statusOptions: StatusOption[] = [
    { value: 'todo', label: 'Cần thực hiện', badgeClass: 'bg-blue-50 text-blue-600' },
    { value: 'inprogress', label: 'Đang thực hiện', badgeClass: 'bg-amber-50 text-amber-600' },
    { value: 'done', label: 'Hoàn thành', badgeClass: 'bg-emerald-50 text-emerald-600' }
  ];
  isStatusDropdownOpen: boolean = false;

  // Tag Modal (Create / Edit)
  showTagModal: boolean = false;
  tagModalMode: 'create' | 'edit' = 'create';
  editingTagId: number | null = null;
  tagFormName: string = '';
  tagFormColor: string = '#6366F1';

  // Custom Color Picker Popover State
  showCustomColorPopover: boolean = false;
  customColorHex: string = '#6366F1';
  currentHue: number = 240;
  currentSat: number = 84;
  currentLight: number = 65;

  // API Connection State
  isLoading: boolean = false;
  errorMessage: string | null = null;
  rawTasks: TaskDto[] = [];
  tasks: TaskItem[] = [];

  kanbanTodo: KanbanItem[] = [];
  kanbanInProgress: KanbanItem[] = [];
  kanbanDone: KanbanItem[] = [];

  // --- Helper to generate dynamic date strings relative to today ---
  private getRelativeDate(offsetDays: number): string {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    d.setDate(d.getDate() + offsetDays);
    const dd = d.getDate().toString().padStart(2, '0');
    const mm = (d.getMonth() + 1).toString().padStart(2, '0');
    const yyyy = d.getFullYear();
    return `${dd}/${mm}/${yyyy}`;
  }

  // --- Dynamic AI Suggestions & Deadline Alerts calculated from active API tasks ---
  get aiSuggestions() {
    return (this.tasks || [])
      .filter(t => t.status !== 'Hoàn thành' && (t.dueWarning === 'Quá hạn' || t.priority === 'Cao' || t.priority === 'Khẩn cấp'))
      .slice(0, 3)
      .map((t, idx) => ({
        rank: idx + 1,
        title: t.title,
        due: t.dueWarning || 'Trong hạn',
        dueClass: t.warnClass,
        priority: t.priority,
        pClass: t.priorityClass
      }));
  }

  get deadlineAlerts() {
    return (this.tasks || [])
      .filter(t => t.status !== 'Hoàn thành' && t.dueWarning)
      .slice(0, 3)
      .map(t => ({
        title: t.title,
        due: t.dueWarning,
        badge: t.dueWarning,
        badgeClass: t.priorityClass
      }));
  }

  // Filter Bar States
  filterSearch: string = '';
  filterStatus: string = 'all';
  filterPriority: string = 'all';
  filterSubject: string = 'all';
  filterDueDate: string = '';

  isFilterStatusDropdownOpen: boolean = false;
  isFilterPriorityDropdownOpen: boolean = false;
  isFilterSubjectDropdownOpen: boolean = false;

  showOtherFiltersModal: boolean = false;
  editingTaskId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService,
    private subjectService: SubjectService,
    private eRef: ElementRef
  ) {}





  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-dropdown-container')) {
      this.isSubjectDropdownOpen = false;
      this.isStatusDropdownOpen = false;
      this.activeMoreOptionsTagId = null;
      this.isFilterStatusDropdownOpen = false;
      this.isFilterPriorityDropdownOpen = false;
      this.isFilterSubjectDropdownOpen = false;
    }
  }

  toggleOtherFiltersModal() {
    this.showOtherFiltersModal = !this.showOtherFiltersModal;
  }

  // --- Filter Dropdown Toggle & Selection Methods ---
  toggleFilterStatusDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.isFilterStatusDropdownOpen = !this.isFilterStatusDropdownOpen;
    this.isFilterPriorityDropdownOpen = false;
    this.isFilterSubjectDropdownOpen = false;
  }

  selectFilterStatus(statusVal: string, event?: Event) {
    if (event) event.stopPropagation();
    this.filterStatus = statusVal;
    this.isFilterStatusDropdownOpen = false;
  }

  toggleFilterPriorityDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.isFilterPriorityDropdownOpen = !this.isFilterPriorityDropdownOpen;
    this.isFilterStatusDropdownOpen = false;
    this.isFilterSubjectDropdownOpen = false;
  }

  selectFilterPriority(priorityVal: string, event?: Event) {
    if (event) event.stopPropagation();
    this.filterPriority = priorityVal;
    this.isFilterPriorityDropdownOpen = false;
  }

  toggleFilterSubjectDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.isFilterSubjectDropdownOpen = !this.isFilterSubjectDropdownOpen;
    this.isFilterStatusDropdownOpen = false;
    this.isFilterPriorityDropdownOpen = false;
  }

  selectFilterSubject(subjectVal: string, event?: Event) {
    if (event) event.stopPropagation();
    this.filterSubject = subjectVal;
    this.isFilterSubjectDropdownOpen = false;
  }

  getFilterStatusLabel(): string {
    if (this.filterStatus === 'all') return 'Trạng thái: Tất cả';
    if (this.filterStatus === 'todo') return 'Cần thực hiện';
    if (this.filterStatus === 'inprogress') return 'Đang thực hiện';
    if (this.filterStatus === 'done') return 'Hoàn thành';
    if (this.filterStatus === 'overdue') return 'Quá hạn';
    return 'Trạng thái';
  }

  getFilterPriorityLabel(): string {
    if (this.filterPriority === 'all') return 'Mức độ ưu tiên: Tất cả';
    return 'Ưu tiên: ' + this.filterPriority;
  }

  getFilterSubjectLabel(): string {
    if (this.filterSubject === 'all') return 'Môn học / Tag: Tất cả';
    return 'Tag: ' + this.filterSubject;
  }

  // --- Dynamic Subject Tag List extracted from tasks + defaults ---
  get dynamicSubjectTags(): SubjectTag[] {
    const tagMap = new Map<string, { name: string; color: string }>();

    // 1. Include default subjectTags
    if (this.subjectTags && Array.isArray(this.subjectTags)) {
      this.subjectTags.forEach(t => {
        if (t && t.name && t.name.trim()) {
          const key = t.name.trim().toLowerCase();
          if (!tagMap.has(key)) {
            tagMap.set(key, { name: t.name.trim(), color: t.color || '#6366F1' });
          }
        }
      });
    }

    // 2. Extract dynamically from active tasks
    if (this.tasks && Array.isArray(this.tasks)) {
      this.tasks.forEach(t => {
        if (t && t.tag && t.tag.trim()) {
          const key = t.tag.trim().toLowerCase();
          if (!tagMap.has(key)) {
            tagMap.set(key, { name: t.tag.trim(), color: t.tagColor || '#6366F1' });
          }
        }
      });
    }

    const result: SubjectTag[] = [];
    let counter = 1;
    tagMap.forEach((val) => {
      result.push({ id: counter++, name: val.name, color: val.color });
    });

    return result;
  }

  // --- Safe Date Normalizer Helper (Converts any date string to YYYY-MM-DD) ---
  private normalizeDateToYMD(dateStr?: string): string | null {
    if (!dateStr || dateStr === 'Không có') return null;
    try {
      const str = dateStr.trim();

      // Case 1: Contains / (e.g. "28/07/2026" or "2/7/2024" or "25/06 - Quá hạn")
      if (str.includes('/')) {
        const nums = str.match(/\d+/g);
        if (nums && nums.length >= 2) {
          const day = nums[0].padStart(2, '0');
          const month = nums[1].padStart(2, '0');
          const year = nums.length >= 3 ? nums[2] : new Date().getFullYear().toString();
          const fullYear = year.length === 2 ? '20' + year : year;
          return `${fullYear}-${month}-${day}`;
        }
      }

      // Case 2: Contains - (e.g. "2024-06-25" or "2024-06-25T00:00:00")
      if (str.includes('-')) {
        const dateOnly = str.split('T')[0].split(' ')[0];
        const parts = dateOnly.split('-');
        if (parts.length === 3) {
          const year = parts[0];
          const month = parts[1].padStart(2, '0');
          const day = parts[2].padStart(2, '0');
          return `${year}-${month}-${day}`;
        }
      }

      // Case 3: Try standard Date parse
      const d = new Date(str);
      if (!isNaN(d.getTime())) {
        const y = d.getFullYear();
        const m = (d.getMonth() + 1).toString().padStart(2, '0');
        const day = d.getDate().toString().padStart(2, '0');
        return `${y}-${m}-${day}`;
      }
    } catch (e) {}
    return null;
  }

  resetAllFilters() {
    this.filterSearch = '';
    this.filterStatus = 'all';
    this.filterPriority = 'all';
    this.filterSubject = 'all';
    this.filterDueDate = '';
  }

  // --- Comprehensive Filtered Task Getters (Safe-Null & Crash Free) ---
  get totalTasks(): number { return this.tasks.length; }
  get todoCount(): number { return this.tasks.filter(t => t.status === 'Cần thực hiện').length; }
  get inProgressCount(): number { return this.tasks.filter(t => t.status === 'Đang thực hiện').length; }
  get completedCount(): number { return this.tasks.filter(t => t.status === 'Hoàn thành').length; }
  get overdueCount(): number { return this.tasks.filter(t => t.dueWarning === 'Quá hạn').length; }

  get filteredTasks(): TaskItem[] {
    if (!this.tasks || !Array.isArray(this.tasks)) return [];

    return this.tasks.filter(task => {
      if (!task) return false;

      // 1. Search Query Filter
      if (this.filterSearch && this.filterSearch.trim()) {
        const search = this.filterSearch.trim().toLowerCase();
        const title = (task.title || '').toLowerCase();
        const desc = (task.desc || '').toLowerCase();
        const tag = (task.tag || '').toLowerCase();
        if (!title.includes(search) && !desc.includes(search) && !tag.includes(search)) return false;
      }

      // 2. Status Filter
      if (this.filterStatus && this.filterStatus !== 'all') {
        if (this.filterStatus === 'todo' && task.status !== 'Cần thực hiện') return false;
        if (this.filterStatus === 'inprogress' && task.status !== 'Đang thực hiện' && task.status !== 'Tạm dừng') return false;
        if (this.filterStatus === 'done' && task.status !== 'Hoàn thành') return false;
        if (this.filterStatus === 'overdue' && task.status !== 'Quá hạn' && task.dueWarning !== 'Quá hạn') return false;
      }

      // 3. Priority Filter
      if (this.filterPriority && this.filterPriority !== 'all') {
        if (task.priority !== this.filterPriority) return false;
      }

      // 4. Subject / Tag Filter (Case-insensitive & Null-safe)
      if (this.filterSubject && this.filterSubject !== 'all') {
        const targetSubject = this.filterSubject.trim().toLowerCase();
        const taskSubject = (task.tag || '').trim().toLowerCase();
        if (taskSubject !== targetSubject) return false;
      }

      // 5. Due Date Filter (YMD Safe Comparison)
      if (this.filterDueDate) {
        const taskYMD = this.normalizeDateToYMD(task.dueDate);
        if (taskYMD !== this.filterDueDate) return false;
      }

      return true;
    });
  }

  get filteredKanbanTodo(): KanbanItem[] {
    if (this.filterStatus !== 'all' && this.filterStatus !== 'todo') {
      if (this.filterStatus === 'overdue') {
        return this.filterKanbanItems(this.kanbanTodo).filter(item => {
          const ymd = this.normalizeDateToYMD(item.due);
          const today = new Date().toISOString().split('T')[0];
          return ymd && ymd < today;
        });
      }
      return [];
    }
    return this.filterKanbanItems(this.kanbanTodo);
  }

  get filteredKanbanInProgress(): KanbanItem[] {
    if (this.filterStatus !== 'all' && this.filterStatus !== 'inprogress') {
      if (this.filterStatus === 'overdue') {
        return this.filterKanbanItems(this.kanbanInProgress).filter(item => {
          const ymd = this.normalizeDateToYMD(item.due);
          const today = new Date().toISOString().split('T')[0];
          return ymd && ymd < today;
        });
      }
      return [];
    }
    return this.filterKanbanItems(this.kanbanInProgress);
  }

  get filteredKanbanDone(): KanbanItem[] {
    if (this.filterStatus !== 'all' && this.filterStatus !== 'done') {
      return [];
    }
    return this.filterKanbanItems(this.kanbanDone);
  }

  private filterKanbanItems(items: KanbanItem[]): KanbanItem[] {
    if (!items || !Array.isArray(items)) return [];

    return items.filter(item => {
      if (!item) return false;

      // 1. Search Query (matches title, tag, due date, or task description)
      if (this.filterSearch && this.filterSearch.trim()) {
        const search = this.filterSearch.trim().toLowerCase();
        const title = (item.title || '').toLowerCase();
        const tag = (item.tag || '').toLowerCase();
        const due = (item.due || '').toLowerCase();
        const masterTask = this.tasks.find(t => (t.id && item.id ? t.id === item.id : t.title === item.title));
        const desc = masterTask ? (masterTask.desc || '').toLowerCase() : '';

        if (!title.includes(search) && !tag.includes(search) && !due.includes(search) && !desc.includes(search)) {
          return false;
        }
      }

      // 2. Priority Filter (handles 'Cao'/'high', 'Trung bình'/'medium', 'Thấp'/'low')
      if (this.filterPriority && this.filterPriority !== 'all') {
        const itemPri = (item.priority || '').toLowerCase();
        const filterPri = this.filterPriority.toLowerCase();
        let targetPriority = filterPri;
        if (filterPri === 'high') targetPriority = 'cao';
        else if (filterPri === 'medium') targetPriority = 'trung bình';
        else if (filterPri === 'low') targetPriority = 'thấp';

        if (itemPri !== targetPriority && itemPri !== filterPri) return false;
      }

      // 3. Subject / Tag Filter
      if (this.filterSubject && this.filterSubject !== 'all') {
        const targetSubject = this.filterSubject.trim().toLowerCase();
        const itemSubject = (item.tag || '').trim().toLowerCase();
        if (itemSubject !== targetSubject) return false;
      }

      // 4. Due Date Filter
      if (this.filterDueDate) {
        const itemYMD = this.normalizeDateToYMD(item.due);
        if (itemYMD !== this.filterDueDate) return false;
      }

      return true;
    });
  }

  // --- Task Actions: Edit, Delete, Toggle Status ---
  toggleTaskStatus(task: TaskItem) {
    if (!task || !task.id) return;

    let statusCode: number;
    if (task.status === 'Hoàn thành') {
      task.status = 'Cần thực hiện';
      task.statusClass = 'bg-blue-50 text-blue-600 font-semibold';
      task.progress = 0;
      task.progressColor = '#5B4DFF';
      statusCode = 0;
    } else {
      task.status = 'Hoàn thành';
      task.statusClass = 'bg-emerald-50 text-emerald-600 font-semibold';
      task.progress = 100;
      task.progressColor = '#10b981';
      statusCode = 3;
    }

    // Optimistic UI update
    this.recomputeAllDueStatuses();
    this.rebuildKanbanLists();

    // API Call via PATCH /tasks/{id}/status
    this.taskService.updateTaskStatus(task.id, statusCode).subscribe({
      next: (updatedDto: TaskDto) => {
        if (updatedDto) {
          const index = this.tasks.findIndex(t => t.id === task.id);
          if (index !== -1) {
            this.tasks[index] = this.mapDtoToTaskItem(updatedDto);
            this.recomputeAllDueStatuses();
            this.rebuildKanbanLists();
          }
        }
      },
      error: (err) => {
        console.warn('API updateTaskStatus error. Re-syncing with SQL Server DB:', err);
        this.loadTasksFromApi();
      }
    });
  }

  confirmDeleteTask(taskId: number, event?: Event) {
    if (event) event.stopPropagation();
    if (confirm('Bạn có chắc chắn muốn xóa công việc này không?')) {
      this.tasks = this.tasks.filter(t => t.id !== taskId);
      this.recomputeAllDueStatuses();
      this.rebuildKanbanLists();

      this.taskService.deleteTask(taskId).subscribe({
        next: () => {
          this.successToastMessage = `Đã xóa công việc thành công!`;
          setTimeout(() => { this.successToastMessage = null; }, 3000);
        },
        error: (err) => {
          console.warn('Delete task API error. Re-syncing with SQL Server DB:', err);
          this.loadTasksFromApi();
        }
      });
    }
  }

  openEditTaskModal(task: TaskItem, event?: Event) {
    if (event) event.stopPropagation();
    this.editingTaskId = task.id;
    this.isEditMode = true;
    this.newTask.title = task.title;
    this.newTask.desc = task.desc;
    this.newTask.progress = task.progress;

    const tagMatch = this.subjectTags.find(t => t.name.toLowerCase() === task.tag.toLowerCase());
    if (tagMatch) {
      this.selectedSubjectTag = tagMatch;
      this.newTask.subject = tagMatch.name;
    } else {
      this.selectedSubjectTag = { id: 999, name: task.tag, color: task.tagColor || '#6366F1' };
      this.newTask.subject = task.tag;
    }

    if (task.priority === 'Cao') this.newTask.priority = 'high';
    else if (task.priority === 'Thấp') this.newTask.priority = 'low';
    else this.newTask.priority = 'medium';

    if (task.status === 'Đang thực hiện') this.newTask.status = 'inprogress';
    else if (task.status === 'Hoàn thành') this.newTask.status = 'done';
    else this.newTask.status = 'todo';

    this.showCreateTask = true;
  }

  drop(event: CdkDragDrop<KanbanItem[]>) {
    const movedItem = event.previousContainer.data[event.previousIndex];
    if (!movedItem) return;

    // Only process cross-column drops
    if (event.previousContainer === event.container) return;

    // Find master task in this.tasks with type-safe ID comparison
    const task = this.tasks.find(t => (t.id !== undefined && movedItem.id !== undefined ? Number(t.id) === Number(movedItem.id) : t.title === movedItem.title));
    if (!task) return;

    // Determine new status
    let statusCode = 0;
    if (event.container.id === 'todoList') {
      task.status = 'Cần thực hiện';
      task.statusClass = 'bg-blue-50 text-blue-600 font-semibold';
      task.progress = 0;
      task.progressColor = '#5B4DFF';
      statusCode = 0;
    } else if (event.container.id === 'inProgressList') {
      task.status = 'Đang thực hiện';
      task.statusClass = 'bg-amber-50 text-amber-600 font-semibold';
      task.progress = 50;
      task.progressColor = '#f59e0b';
      statusCode = 1;
    } else if (event.container.id === 'doneList') {
      task.status = 'Hoàn thành';
      task.statusClass = 'bg-emerald-50 text-emerald-600 font-semibold';
      task.progress = 100;
      task.progressColor = '#10b981';
      statusCode = 3;
    }

    // Optimistic UI update: recompute due statuses & rebuild kanban from master array
    this.recomputeAllDueStatuses();
    this.rebuildKanbanLists();

    // Backend API Sync via PATCH /tasks/{id}/status
    if (task.id !== undefined && task.id !== null) {
      this.taskService.updateTaskStatus(task.id, statusCode).subscribe({
        next: (updatedDto: TaskDto) => {
          if (updatedDto) {
            const index = this.tasks.findIndex(t => Number(t.id) === Number(task.id));
            if (index !== -1) {
              this.tasks[index] = this.mapDtoToTaskItem(updatedDto);
              this.recomputeAllDueStatuses();
              this.rebuildKanbanLists();
            }
          }
        },
        error: (err) => {
          console.warn('API updateTaskStatus error during drag & drop. Re-syncing with SQL Server DB:', err);
          this.loadTasksFromApi();
        }
      });
    }
  }

  // --- Dynamic Soft Color Badge Helper ---
  getTagBadgeStyle(colorHex?: string) {
    if (!colorHex) return {};
    return {
      'background-color': colorHex + '1A', // ~10% opacity soft bg
      'color': colorHex
    };
  }

  // --- Status Select Methods ---
  toggleStatusDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.isStatusDropdownOpen = !this.isStatusDropdownOpen;
    this.isSubjectDropdownOpen = false;
    this.activeMoreOptionsTagId = null;
  }

  selectStatus(option: StatusOption, event?: Event) {
    if (event) event.stopPropagation();
    this.newTask.status = option.value;
    this.isStatusDropdownOpen = false;

    // Sync progress based on status selection
    if (this.newTask.status === 'todo') {
      this.newTask.progress = 0;
    } else if (this.newTask.status === 'done') {
      this.newTask.progress = 100;
    } else if (this.newTask.status === 'inprogress') {
      if (this.newTask.progress === 0 || this.newTask.progress === 100) {
        this.newTask.progress = 50;
      }
    }
  }

  onProgressChange() {
    if (this.newTask.progress === 0) {
      this.newTask.status = 'todo';
    } else if (this.newTask.progress === 100) {
      this.newTask.status = 'done';
    } else {
      this.newTask.status = 'inprogress';
    }
  }

  setQuickProgress(p: number) {
    this.newTask.progress = p;
    this.onProgressChange();
  }

  getProgressStep(): number {
    return Math.round(this.newTask.progress / 25);
  }

  getSelectedStatusOption(): StatusOption {
    return this.statusOptions.find(o => o.value === this.newTask.status) || this.statusOptions[0];
  }

  ngOnInit() {
    this.subjectService.getSubjectTags().subscribe({
      next: (tags) => {
        if (tags && tags.length > 0) {
          this.subjectTags = tags;
        }
      },
      error: (err) => console.warn('Could not load subject tags from SubjectService:', err)
    });

    this.route.queryParams.subscribe(params => {
      if (params['view'] === 'kanban') {
        this.currentView = 'kanban';
      } else if (params['view'] === 'list') {
        this.currentView = 'list';
      }
    });

    this.loadTasksFromApi();
  }

  loadTasksFromApi() {
    this.isLoading = true;
    this.errorMessage = null;

    this.taskService.getTasks({ pageSize: 50 }).pipe(take(1)).subscribe({
      next: (data: PagedList<TaskDto>) => {
        this.isLoading = false;
        this.rawTasks = data.items || [];
        this.processTasksData(this.rawTasks);
      },
      error: (err) => {
        this.isLoading = false;
        console.warn('API getTasks error:', err);
        this.rawTasks = [];
        this.tasks = [];
        this.kanbanTodo = [];
        this.kanbanInProgress = [];
        this.kanbanDone = [];
        this.errorMessage = 'Chưa có công việc nào hoặc không thể kết nối máy chủ.';
      }
    });
  }

  private getSavedLocalTasks() {
    return null;
  }

  private saveLocalTasks() {
    // Single Source of Truth: SQL Server DB via API (no local task list overriding)
  }

  private processTasksData(dtos: TaskDto[]) {
    // Single Source of Truth: SQL Server Database via API
    this.tasks = (dtos || []).map(dto => this.mapDtoToTaskItem(dto));
    this.recomputeAllDueStatuses();
    this.rebuildKanbanLists();
  }

  private mapDtoToTaskItem(dto: TaskDto): TaskItem {
    const priorityInfo = this.getPriorityDisplay(dto.doUuTien);
    const statusInfo = this.getStatusDisplay(dto.trangThai);
    const dueInfo = this.formatDueDate(dto.hanHoanThanh, dto.trangThai);

    return {
      id: dto.maCongViec,
      title: dto.tieuDe,
      desc: dto.moTa || '',
      tag: dto.tenMonHoc || dto.maMon || 'Công việc',
      tagClass: 'bg-purple-50 text-purple-600 font-medium',
      tagColor: dto.mauSac || '#6366F1',
      priority: priorityInfo.text,
      priorityClass: priorityInfo.cssClass,
      dueDate: dueInfo.formattedDate,
      dueWarning: dueInfo.warningText,
      warnClass: dueInfo.warningClass,
      status: statusInfo.text,
      statusClass: statusInfo.cssClass,
      progress: dto.tiLeHoanThanh ?? (dto.trangThai === 3 ? 100 : 0),
      progressColor: dto.mauSac || (dto.trangThai === 3 ? '#10b981' : (dto.trangThai === 1 ? '#f59e0b' : '#5B4DFF'))
    };
  }

  private mapDtoToKanbanItem(dto: TaskDto): KanbanItem {
    const priorityInfo = this.getPriorityDisplay(dto.doUuTien);
    const dueInfo = this.formatDueDate(dto.hanHoanThanh, dto.trangThai);

    return {
      id: dto.maCongViec,
      title: dto.tieuDe,
      tag: dto.tenMonHoc || dto.maMon || 'Công việc',
      tagClass: 'bg-purple-50 text-purple-600 font-medium',
      tagColor: dto.mauSac || '#6366F1',
      due: dueInfo.formattedDate,
      priority: priorityInfo.text,
      pClass: priorityInfo.cssClass
    };
  }

  private getPriorityDisplay(priority: number): { text: string; cssClass: string } {
    switch (priority) {
      case 0:
        return { text: 'Thấp', cssClass: 'bg-emerald-100 text-emerald-600' };
      case 1:
        return { text: 'Trung bình', cssClass: 'bg-amber-100 text-amber-600' };
      case 2:
        return { text: 'Cao', cssClass: 'bg-rose-100 text-rose-600' };
      case 3:
        return { text: 'Khẩn cấp', cssClass: 'bg-red-100 text-red-700 font-bold' };
      default:
        return { text: 'Bình thường', cssClass: 'bg-gray-100 text-gray-600' };
    }
  }

  private getStatusDisplay(status: number): { text: string; cssClass: string } {
    switch (status) {
      case 0:
        return { text: 'Cần thực hiện', cssClass: 'bg-blue-50 text-blue-600 font-semibold' };
      case 1:
        return { text: 'Đang thực hiện', cssClass: 'bg-amber-50 text-amber-600 font-semibold' };
      case 2:
        return { text: 'Tạm dừng', cssClass: 'bg-gray-100 text-gray-600 font-semibold' };
      case 3:
        return { text: 'Hoàn thành', cssClass: 'bg-emerald-50 text-emerald-600 font-semibold' };
      case 4:
        return { text: 'Quá hạn', cssClass: 'bg-rose-50 text-rose-600 font-semibold' };
      default:
        return { text: 'Cần thực hiện', cssClass: 'bg-blue-50 text-blue-600 font-semibold' };
    }
  }

  private formatDueDate(dateStr?: string, status?: number): { formattedDate: string; warningText: string; warningClass: string } {
    if (status === 3) {
      return { formattedDate: dateStr ? new Date(dateStr).toLocaleDateString('vi-VN') : '', warningText: 'Đã hoàn thành', warningClass: 'text-emerald-600 font-medium' };
    }

    if (!dateStr) {
      return { formattedDate: 'Không có', warningText: '', warningClass: 'text-gray-400' };
    }

    const date = new Date(dateStr);
    const formattedDate = date.toLocaleDateString('vi-VN');
    const now = new Date();
    const diffTime = date.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0 || status === 4) {
      return { formattedDate, warningText: 'Quá hạn', warningClass: 'text-red-600 font-bold' };
    } else if (diffDays === 0) {
      return { formattedDate, warningText: 'Hôm nay', warningClass: 'text-amber-500 font-medium' };
    } else {
      return {
        formattedDate,
        warningText: `Còn ${diffDays} ngày`,
        warningClass: diffDays <= 2 ? 'text-red-500 font-medium' : (diffDays <= 5 ? 'text-orange-500 font-medium' : 'text-gray-400')
      };
    }
  }

  /**
   * Safe helper to parse date strings (supports DD/MM/YYYY, YYYY-MM-DD, ISO string) to Date at midnight.
   */
  private parseDateStringToMidnight(dateStr: string): Date | null {
    if (!dateStr || dateStr === 'Không có' || dateStr === 'Chưa xếp') return null;

    const dmyMatch = dateStr.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
    if (dmyMatch) {
      const day = parseInt(dmyMatch[1], 10);
      const month = parseInt(dmyMatch[2], 10) - 1;
      const year = parseInt(dmyMatch[3], 10);
      const d = new Date(year, month, day);
      d.setHours(0, 0, 0, 0);
      return d;
    }

    const d = new Date(dateStr);
    if (!isNaN(d.getTime())) {
      d.setHours(0, 0, 0, 0);
      return d;
    }

    return null;
  }

  /**
   * Pre-computes dynamic due date status (dueWarning + warnClass) for all tasks in this.tasks.
   * Invoked on data load (API/localStorage), create, edit, status update, and delete.
   */
  recomputeAllDueStatuses() {
    if (!this.tasks || !Array.isArray(this.tasks)) return;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    this.tasks.forEach(task => {
      if (task.status === 'Hoàn thành') {
        task.dueWarning = 'Đã hoàn thành';
        task.warnClass = 'text-emerald-600 font-medium';
        return;
      }

      if (!task.dueDate || task.dueDate === 'Không có' || task.dueDate === 'Chưa xếp') {
        task.dueWarning = '';
        task.warnClass = 'text-gray-400';
        return;
      }

      const due = this.parseDateStringToMidnight(task.dueDate);
      if (!due) {
        return;
      }

      const diffMs = due.getTime() - today.getTime();
      const diffDays = Math.round(diffMs / (1000 * 60 * 60 * 24));

      if (diffDays < 0 || task.status === 'Quá hạn') {
        task.dueWarning = 'Quá hạn';
        task.warnClass = 'text-red-600 font-bold';
      } else if (diffDays === 0) {
        task.dueWarning = 'Hôm nay';
        task.warnClass = 'text-amber-500 font-medium';
      } else if (diffDays <= 2) {
        task.dueWarning = `Còn ${diffDays} ngày`;
        task.warnClass = 'text-red-500 font-medium';
      } else if (diffDays <= 5) {
        task.dueWarning = `Còn ${diffDays} ngày`;
        task.warnClass = 'text-orange-500 font-medium';
      } else {
        task.dueWarning = `Còn ${diffDays} ngày`;
        task.warnClass = 'text-gray-400';
      }
    });
  }

  /**
   * Fast O(1) getter for task due status from pre-computed task properties.
   */
  computeDueStatus(task: TaskItem): { label: string; cssClass: string } {
    return { label: task.dueWarning || '', cssClass: task.warnClass || 'text-gray-400' };
  }

  // Counts for filter badges
  get countAll(): number { return this.tasks.length; }
  get countTodo(): number { return this.tasks.filter(t => t.status === 'Cần thực hiện').length; }
  get countInProgress(): number { return this.tasks.filter(t => t.status === 'Đang thực hiện' || t.status === 'Tạm dừng').length; }
  get countDone(): number { return this.tasks.filter(t => t.status === 'Hoàn thành').length; }
  get countOverdue(): number { return this.tasks.filter(t => t.status === 'Quá hạn').length; }

  setView(v: 'list' | 'kanban') {
    this.currentView = v;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { view: v },
      queryParamsHandling: 'merge'
    });
  }

  setFilterStatus(s: string) {
    this.filterStatus = s;
  }

  showCreateTaskForm(initialStatus: string = 'todo') {
    this.editingTaskId = null;
    this.isEditMode = false;
    this.selectedSubjectTag = null;

    let progressVal = 0;
    if (initialStatus === 'done') progressVal = 100;
    else if (initialStatus === 'inprogress') progressVal = 50;

    this.newTask = {
      title: '',
      desc: '',
      subject: '',
      priority: 'medium',
      startDate: '',
      dueDate: '',
      status: initialStatus,
      progress: progressVal
    };
    this.showCreateTask = true;
  }

  successToastMessage: string | null = null;

  cancelCreateTask() {
    this.showCreateTask = false;
    this.isEditMode = false;
    this.editingTaskId = null;
    this.selectedSubjectTag = null;
    this.isSubjectDropdownOpen = false;
    this.isStatusDropdownOpen = false;
    this.activeMoreOptionsTagId = null;
    this.newTask = { title: '', desc: '', subject: '', priority: 'medium', startDate: '', dueDate: '', status: 'todo', progress: 0 };
  }

  submitCreateTask() {
    if (!this.newTask.title || !this.newTask.title.trim()) {
      alert('Vui lòng nhập tiêu đề công việc!');
      return;
    }

    const title = this.newTask.title.trim();
    const desc = this.newTask.desc.trim();
    const subjectName = this.selectedSubjectTag?.name || this.newTask.subject || 'Công việc';
    const subjectColor = this.selectedSubjectTag?.color || '#6366F1';

    // Priority mapping
    let priorityLabel = 'Trung bình';
    let priorityClass = 'bg-amber-100 text-amber-600';
    let priorityCode = 1;
    if (this.newTask.priority === 'high') {
      priorityLabel = 'Cao';
      priorityClass = 'bg-red-100 text-red-600';
      priorityCode = 2;
    } else if (this.newTask.priority === 'low') {
      priorityLabel = 'Thấp';
      priorityClass = 'bg-emerald-100 text-emerald-600';
      priorityCode = 0;
    }

    // Status mapping
    let statusLabel = 'Cần thực hiện';
    let statusClass = 'bg-blue-50 text-blue-600 font-semibold';
    let statusCode = 0;
    if (this.newTask.status === 'inprogress') {
      statusLabel = 'Đang thực hiện';
      statusClass = 'bg-amber-50 text-amber-600 font-semibold';
      statusCode = 1;
    } else if (this.newTask.status === 'done') {
      statusLabel = 'Hoàn thành';
      statusClass = 'bg-emerald-50 text-emerald-600 font-semibold';
      statusCode = 3;
    }

    const taskDtoPayload: Partial<TaskDto> = {
      maMonHoc: (this.selectedSubjectTag && this.selectedSubjectTag.id > 0 && this.selectedSubjectTag.id < 100) ? this.selectedSubjectTag.id : undefined,
      tieuDe: title,
      moTa: desc,
      doUuTien: priorityCode,
      trangThai: statusCode,
      ngayBatDau: this.newTask.startDate || undefined,
      hanHoanThanh: this.newTask.dueDate || undefined,
      tenMonHoc: subjectName,
      mauSac: subjectColor,
      danhDauQuanTrong: this.newTask.priority === 'high'
    };

    // POST /api/v1/tasks -> map returned TaskDto to TaskItem -> push to this.tasks -> recompute & rebuild
    this.taskService.createTask(taskDtoPayload).subscribe({
      next: (createdTask: TaskDto) => {
        if (createdTask && createdTask.maCongViec) {
          const newTaskItem = this.mapDtoToTaskItem(createdTask);
          this.tasks.unshift(newTaskItem);
          this.recomputeAllDueStatuses();
          this.rebuildKanbanLists();
        } else {
          this.loadTasksFromApi();
        }

        this.successToastMessage = `Đã tạo task "${title}" thành công!`;
        setTimeout(() => {
          this.successToastMessage = null;
        }, 4000);

        this.cancelCreateTask();
      },
      error: (err) => {
        console.error('API Create Task error details:', err?.error || err);
        alert('Tạo công việc thất bại. Vui lòng kiểm tra lại kết nối!');
      }
    });
  }

  updateTask() {
    if (!this.newTask.title || !this.newTask.title.trim()) {
      alert('Vui lòng nhập tiêu đề công việc!');
      return;
    }

    if (this.editingTaskId === null) return;

    const title = this.newTask.title.trim();
    const desc = this.newTask.desc.trim();
    const subjectName = this.selectedSubjectTag?.name || this.newTask.subject || 'Công việc';
    const subjectColor = this.selectedSubjectTag?.color || '#6366F1';

    let priorityCode = 1;
    if (this.newTask.priority === 'high') priorityCode = 2;
    else if (this.newTask.priority === 'low') priorityCode = 0;

    let statusCode = 0;
    if (this.newTask.status === 'inprogress') statusCode = 1;
    else if (this.newTask.status === 'done') statusCode = 3;

    const taskDtoPayload: Partial<TaskDto> = {
      maCongViec: this.editingTaskId,
      maMonHoc: (this.selectedSubjectTag && this.selectedSubjectTag.id > 0 && this.selectedSubjectTag.id < 100) ? this.selectedSubjectTag.id : undefined,
      tieuDe: title,
      moTa: desc,
      doUuTien: priorityCode,
      trangThai: statusCode,
      ngayBatDau: this.newTask.startDate || undefined,
      hanHoanThanh: this.newTask.dueDate || undefined,
      tenMonHoc: subjectName,
      mauSac: subjectColor,
      danhDauQuanTrong: this.newTask.priority === 'high'
    };

    // PUT /api/v1/tasks/{id} -> map returned updatedDto to TaskItem -> update this.tasks
    this.taskService.updateTask(this.editingTaskId, taskDtoPayload).subscribe({
      next: (updatedDto: TaskDto) => {
        const taskIndex = this.tasks.findIndex(t => t.id === this.editingTaskId);
        if (taskIndex !== -1 && updatedDto) {
          this.tasks[taskIndex] = this.mapDtoToTaskItem(updatedDto);
          this.recomputeAllDueStatuses();
          this.rebuildKanbanLists();
        } else {
          this.loadTasksFromApi();
        }

        this.successToastMessage = `Đã cập nhật công việc thành công!`;
        setTimeout(() => {
          this.successToastMessage = null;
        }, 3000);

        this.cancelCreateTask();
      },
      error: (err) => {
        console.error('API Update Task error details:', err?.error || err);
        alert('Cập nhật công việc thất bại!');
      }
    });
  }

  private rebuildKanbanLists() {
    this.kanbanTodo = this.tasks.filter(t => t.status === 'Cần thực hiện').map(t => this.mapToKanbanItem(t));
    this.kanbanInProgress = this.tasks.filter(t => t.status === 'Đang thực hiện').map(t => this.mapToKanbanItem(t));
    this.kanbanDone = this.tasks.filter(t => t.status === 'Hoàn thành').map(t => this.mapToKanbanItem(t));
  }

  private mapToKanbanItem(t: TaskItem): KanbanItem {
    return {
      id: t.id,
      title: t.title,
      tag: t.tag,
      tagClass: t.tagClass,
      tagColor: t.tagColor,
      due: t.dueDate !== 'Chưa xếp' ? `Hạn chót: ${t.dueDate}` : 'Chưa có hạn',
      priority: t.priority,
      pClass: t.priorityClass
    };
  }

  // --- Subject Select & Tag Management Methods ---
  toggleSubjectDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.isSubjectDropdownOpen = !this.isSubjectDropdownOpen;
    this.isStatusDropdownOpen = false;
    this.activeMoreOptionsTagId = null;
  }

  selectSubjectTag(tag: SubjectTag, event?: Event) {
    if (event) event.stopPropagation();
    this.selectedSubjectTag = tag;
    this.newTask.subject = tag.name;
    this.isSubjectDropdownOpen = false;
    this.activeMoreOptionsTagId = null;
  }

  toggleTagMoreOptions(event: Event, tagId: number) {
    event.stopPropagation();
    if (this.activeMoreOptionsTagId === tagId) {
      this.activeMoreOptionsTagId = null;
    } else {
      this.activeMoreOptionsTagId = tagId;
    }
  }

  openCreateTagModal(event?: Event) {
    if (event) event.stopPropagation();
    this.isSubjectDropdownOpen = false;
    this.activeMoreOptionsTagId = null;
    this.tagModalMode = 'create';
    this.editingTagId = null;
    this.tagFormName = '';
    this.tagFormColor = '#6366F1';
    this.showTagModal = true;
  }

  openEditTagModal(event: Event, tag: SubjectTag) {
    event.stopPropagation();
    this.activeMoreOptionsTagId = null;
    this.isSubjectDropdownOpen = false;
    this.tagModalMode = 'edit';
    this.editingTagId = tag.id;
    this.tagFormName = tag.name;
    this.tagFormColor = tag.color;
    this.showTagModal = true;
  }

  deleteTag(event: Event, tagId: number) {
    event.stopPropagation();
    this.activeMoreOptionsTagId = null;
    this.subjectTags = this.subjectTags.filter(t => t.id !== tagId);
    if (this.selectedSubjectTag?.id === tagId) {
      this.selectedSubjectTag = null;
      this.newTask.subject = '';
    }
  }

  saveTag() {
    if (!this.tagFormName.trim()) return;

    if (this.tagModalMode === 'create') {
      const newTag = this.subjectService.addSubjectTag(this.tagFormName.trim(), this.tagFormColor);
      if (!this.subjectTags.some(t => t.name.toLowerCase() === newTag.name.toLowerCase())) {
        this.subjectTags.push(newTag);
      }
      this.selectSubjectTag(newTag);
    } else if (this.tagModalMode === 'edit' && this.editingTagId !== null) {
      const tagIndex = this.subjectTags.findIndex(t => t.id === this.editingTagId);
      if (tagIndex !== -1) {
        this.subjectTags[tagIndex].name = this.tagFormName.trim();
        this.subjectTags[tagIndex].color = this.tagFormColor;

        if (this.selectedSubjectTag?.id === this.editingTagId) {
          this.selectedSubjectTag = { ...this.subjectTags[tagIndex] };
          this.newTask.subject = this.subjectTags[tagIndex].name;
        }
        this.subjectService.saveLocalSubjectTags(this.subjectTags);
      }
    }

    this.showTagModal = false;
  }

  closeTagModal() {
    this.showTagModal = false;
    this.showCustomColorPopover = false;
  }

  // --- Custom Color Picker Popover Methods (Google Calendar Style Card) ---
  openCustomColorPopover(event?: Event) {
    if (event) event.stopPropagation();
    this.customColorHex = (this.tagFormColor || '#6366F1').toUpperCase();
    const hsl = hexToHsl(this.customColorHex);
    this.currentHue = hsl.h;
    this.currentSat = hsl.s;
    this.currentLight = hsl.l;
    this.showCustomColorPopover = true;
  }

  closeCustomColorPopover(event?: Event) {
    if (event) event.stopPropagation();
    this.showCustomColorPopover = false;
  }

  onHueChange() {
    this.updateHexFromHsl();
  }

  onGradientBoxClick(event: MouseEvent) {
    const target = event.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();
    const x = Math.max(0, Math.min(event.clientX - rect.left, rect.width));
    const y = Math.max(0, Math.min(event.clientY - rect.top, rect.height));

    this.currentSat = Math.round((x / rect.width) * 100);
    const v = 1 - (y / rect.height);
    const satFrac = this.currentSat / 100;
    this.currentLight = Math.round(v * (1 - satFrac / 2) * 100);

    this.updateHexFromHsl();
  }

  updateHexFromHsl() {
    this.customColorHex = hslToHex(this.currentHue, this.currentSat, this.currentLight);
  }

  onNativeColorChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input && input.value) {
      this.customColorHex = input.value.toUpperCase();
      const hsl = hexToHsl(this.customColorHex);
      this.currentHue = hsl.h;
      this.currentSat = hsl.s;
      this.currentLight = hsl.l;
    }
  }

  onHexTextChange() {
    if (/^#[0-9A-Fa-f]{6}$/.test(this.customColorHex)) {
      const hsl = hexToHsl(this.customColorHex);
      this.currentHue = hsl.h;
      this.currentSat = hsl.s;
      this.currentLight = hsl.l;
    }
  }

  saveCustomColor(event?: Event) {
    if (event) event.stopPropagation();
    let hex = this.customColorHex.trim();
    if (!hex.startsWith('#')) hex = '#' + hex;
    hex = hex.toUpperCase();
    this.tagFormColor = hex;

    if (!this.presetColors.includes(hex)) {
      this.presetColors.push(hex);
    }
    this.showCustomColorPopover = false;
  }
}

// --- HSL <-> HEX Utility Functions ---
function hslToHex(h: number, s: number, l: number): string {
  s /= 100;
  l /= 100;
  const a = s * Math.min(l, 1 - l);
  const f = (n: number, k = (n + h / 30) % 12) =>
    l - a * Math.max(Math.min(k - 3, 9 - k, 1), -1);
  const r = Math.round(255 * f(0));
  const g = Math.round(255 * f(8));
  const b = Math.round(255 * f(4));
  return `#${((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1).toUpperCase()}`;
}

function hexToHsl(hex: string): { h: number; s: number; l: number } {
  let c = hex.replace('#', '');
  if (c.length === 3) c = c.split('').map(x => x + x).join('');
  if (c.length !== 6) return { h: 240, s: 84, l: 65 };

  const r = parseInt(c.substring(0, 2), 16) / 255;
  const g = parseInt(c.substring(2, 4), 16) / 255;
  const b = parseInt(c.substring(4, 6), 16) / 255;

  const max = Math.max(r, g, b), min = Math.min(r, g, b);
  let h = 0, s = 0, l = (max + min) / 2;

  if (max !== min) {
    const d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    switch (max) {
      case r: h = (g - b) / d + (g < b ? 6 : 0); break;
      case g: h = (b - r) / d + 2; break;
      case b: h = (r - g) / d + 4; break;
    }
    h /= 6;
  }

  return {
    h: Math.round(h * 360),
    s: Math.round(s * 100),
    l: Math.round(l * 100)
  };
}
