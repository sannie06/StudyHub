import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService, DocumentGroupDto, TaiLieuDto } from '../../services/document.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent implements OnInit {
  groups: DocumentGroupDto[] = [];
  selectedGroup: DocumentGroupDto | null = null;
  documents: TaiLieuDto[] = [];

  loading = false;
  error: string | null = null;
  searchQuery = '';

  // Modals state
  showUploadModal = false;
  showEditModal = false;

  // Forms state
  uploadForm = {
    tieuDe: '',
    moTa: '',
    file: null as File | null
  };

  editForm = {
    maTaiLieu: 0,
    tieuDe: '',
    moTa: ''
  };

  constructor(private documentService: DocumentService) {}

  ngOnInit(): void {
    this.loadGroups();
  }

  loadGroups(): void {
    this.loading = true;
    this.error = null;
    this.documentService.getMyGroups().subscribe({
      next: (groups) => {
        this.groups = groups;
        if (groups.length > 0) {
          this.selectedGroup = groups[0];
          this.loadDocuments();
        } else {
          this.loading = false;
        }
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách nhóm:', err);
        this.error = 'Không thể tải danh sách nhóm học tập. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  loadDocuments(): void {
    if (!this.selectedGroup) return;
    this.loading = true;
    this.error = null;
    this.documentService.getDocuments(this.selectedGroup.maNhom, this.searchQuery).subscribe({
      next: (docs) => {
        this.documents = docs;
        this.loading = false;
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách tài liệu:', err);
        this.error = 'Không thể tải danh sách tài liệu. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  onGroupChange(group: DocumentGroupDto): void {
    this.selectedGroup = group;
    this.searchQuery = '';
    this.loadDocuments();
  }

  onSearch(): void {
    this.loadDocuments();
  }

  // File Handlers
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.uploadForm.file = file;
      if (!this.uploadForm.tieuDe) {
        // Auto-fill title with filename without extension
        this.uploadForm.tieuDe = file.name.substring(0, file.name.lastIndexOf('.')) || file.name;
      }
    }
  }

  openUploadModal(): void {
    this.uploadForm = {
      tieuDe: '',
      moTa: '',
      file: null
    };
    this.showUploadModal = true;
  }

  closeUploadModal(): void {
    this.showUploadModal = false;
  }

  onUploadSubmit(): void {
    if (!this.selectedGroup) return;
    if (!this.uploadForm.tieuDe || !this.uploadForm.file) {
      alert('Vui lòng nhập tiêu đề và chọn tệp tin.');
      return;
    }

    this.loading = true;
    this.documentService.uploadDocument(
      this.selectedGroup.maNhom,
      this.uploadForm.tieuDe,
      this.uploadForm.moTa,
      this.uploadForm.file
    ).subscribe({
      next: () => {
        this.closeUploadModal();
        this.loadDocuments();
      },
      error: (err) => {
        console.error('Lỗi tải lên tài liệu:', err);
        alert(err.error?.message || 'Có lỗi xảy ra khi tải lên tài liệu.');
        this.loading = false;
      }
    });
  }

  // Edit Handlers
  openEditModal(doc: TaiLieuDto): void {
    this.editForm = {
      maTaiLieu: doc.maTaiLieu,
      tieuDe: doc.tieuDe,
      moTa: doc.moTa
    };
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
  }

  onEditSubmit(): void {
    if (!this.editForm.tieuDe) {
      alert('Vui lòng nhập tiêu đề.');
      return;
    }

    this.loading = true;
    this.documentService.updateDocument(
      this.editForm.maTaiLieu,
      this.editForm.tieuDe,
      this.editForm.moTa
    ).subscribe({
      next: () => {
        this.closeEditModal();
        this.loadDocuments();
      },
      error: (err) => {
        console.error('Lỗi cập nhật tài liệu:', err);
        alert('Có lỗi xảy ra khi cập nhật tài liệu.');
        this.loading = false;
      }
    });
  }

  // Delete Handler
  onDelete(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa tài liệu này không?')) {
      this.loading = true;
      this.documentService.deleteDocument(id).subscribe({
        next: () => {
          this.loadDocuments();
        },
        error: (err) => {
          console.error('Lỗi khi xóa tài liệu:', err);
          alert('Không thể xóa tài liệu. Có thể bạn không có quyền.');
          this.loading = false;
        }
      });
    }
  }

  // Download Handler
  onDownload(doc: TaiLieuDto): void {
    this.documentService.downloadDocument(doc.maTaiLieu, doc.tenGoc);
    // Optimistic download count increment on client side
    doc.luotTai++;
  }

  // Helper formatting methods
  formatBytes(bytes: number, decimals = 1): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }

  getFileIcon(ext: string): string {
    const e = ext.toLowerCase();
    if (e === '.pdf') return 'pi pi-file-pdf text-red-500';
    if (e === '.docx' || e === '.doc') return 'pi pi-file-word text-blue-500';
    if (e === '.xlsx' || e === '.xls') return 'pi pi-file-excel text-green-500';
    if (e === '.pptx' || e === '.ppt') return 'pi pi-file text-orange-500';
    if (e === '.png' || e === '.jpg' || e === '.jpeg' || e === '.webp') return 'pi pi-image text-purple-500';
    if (e === '.zip' || e === '.rar') return 'pi pi-folder text-yellow-500';
    return 'pi pi-file text-slate-500';
  }
}
