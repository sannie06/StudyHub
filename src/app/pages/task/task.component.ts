import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DrawerModule } from 'primeng/drawer';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ProgressBarModule } from 'primeng/progressbar';
import { TaskService, TaskDto, PagedList } from '../../services/task.service';
import { SubjectService, SubjectDto } from '../../services/subject.service';

@Component({
  selector: 'app-task',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    ReactiveFormsModule, 
    DrawerModule, 
    DialogModule, 
    ButtonModule, 
    InputTextModule, 
    SelectModule,
    ProgressBarModule
  ],
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.scss']
})
export class TaskComponent implements OnInit {
  tasks: TaskDto[] = [];
  subjects: SubjectDto[] = [];
  
  loading = true;
  error = '';
  totalCount = 0;
  
  // Filters
  pageNumber = 1;
  pageSize = 10;
  search = '';
  selectedPriority: number | null = null;
  selectedStatus: number | null = null;
  selectedSubjectId: number | null = null;
  sortBy = 'hanhoanthanh';
  sortDirection = 'asc';

  // Responsive Drawer/Dialog control
  displayDetail = false;
  viewMode: 'drawer' | 'dialog' | 'fullscreen' = 'drawer';
  taskForm!: FormGroup;
  isEditMode = false;
  selectedTaskId: number | null = null;
  submitLoading = false;

  priorities = [
    { label: 'Tất cả độ ưu tiên', value: null },
    { label: 'Thấp', value: 0 },
    { label: 'Trung bình', value: 1 },
    { label: 'Cao', value: 2 },
    { label: 'Khẩn cấp', value: 3 }
  ];

  statuses = [
    { label: 'Tất cả trạng thái', value: null },
    { label: 'Chưa bắt đầu', value: 0 },
    { label: 'Đang thực hiện', value: 1 },
    { label: 'Tạm dừng', value: 2 },
    { label: 'Hoàn thành', value: 3 },
    { label: 'Quá hạn', value: 4 }
  ];

  sortOptions = [
    { label: 'Hạn hoàn thành', value: 'hanhoanthanh' },
    { label: 'Độ ưu tiên', value: 'priority' },
    { label: 'Tiêu đề', value: 'title' }
  ];

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private subjectService: SubjectService
  ) {}

  ngOnInit() {
    this.detectScreenSize();
    this.initForm();
    this.loadSubjects();
    this.loadTasks();
  }

  @HostListener('window:resize')
  onResize() {
    this.detectScreenSize();
  }

  detectScreenSize() {
    const width = window.innerWidth;
    if (width >= 1024) {
      this.viewMode = 'drawer';
    } else if (width >= 768) {
      this.viewMode = 'dialog';
    } else {
      this.viewMode = 'fullscreen';
    }
  }

  initForm() {
    this.taskForm = this.fb.group({
      tieuDe: ['', [Validators.required, Validators.maxLength(200)]],
      maMonHoc: [null],
      moTa: [''],
      doUuTien: [1, [Validators.required]],
      trangThai: [0, [Validators.required]],
      tiLeHoanThanh: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
      ngayBatDau: [null],
      hanHoanThanh: [null],
      mauSac: ['#6366F1'],
      danhDauQuanTrong: [false],
      danhDauYeuThich: [false],
      ghiChu: ['']
    });
  }

  loadSubjects() {
    this.subjectService.getSubjects().subscribe({
      next: (data) => this.subjects = data,
      error: (err) => console.error('Lỗi khi tải môn học', err)
    });
  }

  loadTasks() {
    this.loading = true;
    this.error = '';
    
    this.taskService.getTasks({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      search: this.search,
      priority: this.selectedPriority !== null ? this.selectedPriority : undefined,
      status: this.selectedStatus !== null ? this.selectedStatus : undefined,
      subjectId: this.selectedSubjectId !== null ? this.selectedSubjectId : undefined,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    }).subscribe({
      next: (res) => {
        this.tasks = res.items;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Không thể tải danh sách công việc. Vui lòng tải lại.';
        console.error(err);
      }
    });
  }

  onFilterChange() {
    this.pageNumber = 1;
    this.loadTasks();
  }

  toggleSortDirection() {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    this.loadTasks();
  }

  showAddDetail() {
    this.isEditMode = false;
    this.selectedTaskId = null;
    this.taskForm.reset({
      doUuTien: 1,
      trangThai: 0,
      tiLeHoanThanh: 0,
      mauSac: '#6366F1',
      danhDauQuanTrong: false,
      danhDauYeuThich: false
    });
    this.displayDetail = true;
  }

  showEditDetail(task: TaskDto) {
    this.isEditMode = true;
    this.selectedTaskId = task.maCongViec;
    
    this.taskForm.patchValue({
      tieuDe: task.tieuDe,
      maMonHoc: task.maMonHoc,
      moTa: task.moTa,
      doUuTien: task.doUuTien,
      trangThai: task.trangThai,
      tiLeHoanThanh: task.tiLeHoanThanh,
      ngayBatDau: task.ngayBatDau ? task.ngayBatDau.substring(0, 10) : null,
      hanHoanThanh: task.hanHoanThanh ? task.hanHoanThanh.substring(0, 10) : null,
      mauSac: task.mauSac || '#6366F1',
      danhDauQuanTrong: task.danhDauQuanTrong,
      danhDauYeuThich: task.danhDauYeuThich,
      ghiChu: task.ghiChu
    });
    
    this.displayDetail = true;
  }

  onSubmit() {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    const startVal = this.taskForm.value.ngayBatDau;
    const endVal = this.taskForm.value.hanHoanThanh;

    if (startVal && endVal && new Date(endVal) < new Date(startVal)) {
      alert('Hạn hoàn thành phải lớn hơn hoặc bằng ngày bắt đầu.');
      return;
    }

    this.submitLoading = true;
    const formData = this.taskForm.value;

    if (this.isEditMode && this.selectedTaskId) {
      this.taskService.updateTask(this.selectedTaskId, formData).subscribe({
        next: () => {
          this.submitLoading = false;
          this.displayDetail = false;
          this.loadTasks();
        },
        error: (err) => {
          this.submitLoading = false;
          alert(err.error?.title || 'Lỗi khi cập nhật công việc.');
        }
      });
    } else {
      this.taskService.createTask(formData).subscribe({
        next: () => {
          this.submitLoading = false;
          this.displayDetail = false;
          this.loadTasks();
        },
        error: (err) => {
          this.submitLoading = false;
          alert(err.error?.title || 'Lỗi khi tạo công việc.');
        }
      });
    }
  }

  onDeleteTask(id: number) {
    if (!confirm('Bạn có chắc chắn muốn xóa công việc này không?')) {
      return;
    }

    this.taskService.deleteTask(id).subscribe({
      next: () => this.loadTasks(),
      error: (err) => alert(err.error?.title || 'Lỗi khi xóa công việc.')
    });
  }

  onToggleStatus(task: TaskDto) {
    const newStatus = task.trangThai === 3 ? 1 : 3; // Toggle between Completed and In Progress
    this.taskService.updateTaskStatus(task.maCongViec, newStatus).subscribe({
      next: () => this.loadTasks(),
      error: (err) => alert(err.error?.title || 'Lỗi khi cập nhật trạng thái.')
    });
  }

  getPriorityLabel(priority: number): string {
    switch (priority) {
      case 0: return 'Thấp';
      case 1: return 'Trung bình';
      case 2: return 'Cao';
      case 3: return 'Khẩn cấp';
      default: return '';
    }
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 0: return 'Chưa bắt đầu';
      case 1: return 'Đang thực hiện';
      case 2: return 'Tạm dừng';
      case 3: return 'Hoàn thành';
      case 4: return 'Quá hạn';
      default: return '';
    }
  }

  getPriorityClass(priority: number): string {
    switch (priority) {
      case 0: return 'bg-slate-100 text-slate-600';
      case 1: return 'bg-blue-50 text-blue-600';
      case 2: return 'bg-amber-50 text-amber-600';
      case 3: return 'bg-red-50 text-red-600';
      default: return '';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 0: return 'bg-slate-100 text-slate-500';
      case 1: return 'bg-blue-50 text-blue-600';
      case 2: return 'bg-amber-50 text-amber-600';
      case 3: return 'bg-emerald-50 text-emerald-600';
      case 4: return 'bg-red-50 text-red-600';
      default: return '';
    }
  }
}
