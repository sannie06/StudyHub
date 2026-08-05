import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { SubjectService, SubjectDto } from '../../services/subject.service';

@Component({
  selector: 'app-subject',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    ButtonModule, 
    DialogModule, 
    InputTextModule, 
    TooltipModule
  ],
  templateUrl: './subject.component.html',
  styles: []
})
export class SubjectComponent implements OnInit {
  subjects: SubjectDto[] = [];
  loading = true;
  error = '';
  
  displayDialog = false;
  subjectForm!: FormGroup;
  isEditMode = false;
  selectedSubjectId: number | null = null;
  submitLoading = false;

  constructor(
    private fb: FormBuilder,
    private subjectService: SubjectService
  ) {}

  ngOnInit() {
    this.initForm();
    this.loadSubjects();
  }

  initForm() {
    this.subjectForm = this.fb.group({
      tenMonHoc: ['', [Validators.required, Validators.maxLength(150)]],
      maMon: ['', [Validators.required, Validators.maxLength(50)]],
      moTa: [''],
      mauSac: ['#6366F1', [Validators.required, Validators.pattern(/^#(?:[0-9a-fA-F]{3}){1,2}$/)]],
      icon: ['pi-book', [Validators.required, Validators.maxLength(50)]]
    });
  }

  loadSubjects() {
    this.loading = true;
    this.error = '';
    this.subjectService.getSubjects().subscribe({
      next: (data) => {
        this.subjects = data;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Không thể tải danh sách môn học. Vui lòng tải lại trang.';
        console.error(err);
      }
    });
  }

  showAddDialog() {
    this.isEditMode = false;
    this.selectedSubjectId = null;
    this.subjectForm.reset({
      mauSac: '#6366F1',
      icon: 'pi-book'
    });
    this.displayDialog = true;
  }

  showEditDialog(subject: SubjectDto) {
    this.isEditMode = true;
    this.selectedSubjectId = subject.maMonHoc;
    this.subjectForm.patchValue({
      tenMonHoc: subject.tenMonHoc,
      maMon: subject.maMon,
      moTa: subject.moTa,
      mauSac: subject.mauSac,
      icon: subject.icon
    });
    this.displayDialog = true;
  }

  onSubmit() {
    if (this.subjectForm.invalid) {
      this.subjectForm.markAllAsTouched();
      return;
    }

    this.submitLoading = true;
    const formData = this.subjectForm.value;

    if (this.isEditMode && this.selectedSubjectId) {
      this.subjectService.updateSubject(this.selectedSubjectId, { ...formData, trangThai: 1 }).subscribe({
        next: () => {
          this.submitLoading = false;
          this.displayDialog = false;
          this.loadSubjects();
        },
        error: (err) => {
          this.submitLoading = false;
          alert(err.error?.title || 'Lỗi khi cập nhật môn học.');
        }
      });
    } else {
      this.subjectService.createSubject(formData).subscribe({
        next: () => {
          this.submitLoading = false;
          this.displayDialog = false;
          this.loadSubjects();
        },
        error: (err) => {
          this.submitLoading = false;
          alert(err.error?.title || 'Lỗi khi tạo môn học.');
        }
      });
    }
  }

  onDeleteSubject(id: number) {
    if (!confirm('Bạn có chắc chắn muốn xóa môn học này không?')) {
      return;
    }

    this.subjectService.deleteSubject(id).subscribe({
      next: () => {
        this.loadSubjects();
      },
      error: (err) => {
        // Safe display of the business rule violation
        alert(err.error?.title || 'Không thể xóa môn học này.');
      }
    });
  }
}
