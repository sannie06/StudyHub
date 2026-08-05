import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentGroupDto {
  maNhom: number;
  tenNhom: string;
}

export interface TaiLieuDto {
  maTaiLieu: number;
  maNhom: number;
  maNguoiTaiLen: number;
  tenNguoiTaiLen: string;
  tieuDe: string;
  moTa: string;
  luotTai: number;
  ngayTaiLen: string;
  ngayCapNhat?: string;
  maFile: number;
  tenGoc: string;
  loaiFile: string;
  dungLuong: number;
  extension: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private apiUrl = 'http://localhost:5186/api/v1/documents';

  constructor(private http: HttpClient) {}

  getMyGroups(): Observable<DocumentGroupDto[]> {
    return this.http.get<DocumentGroupDto[]>(`${this.apiUrl}/groups`);
  }

  getDocuments(maNhom: number, search?: string): Observable<TaiLieuDto[]> {
    let url = `${this.apiUrl}?maNhom=${maNhom}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<TaiLieuDto[]>(url);
  }

  getDocumentById(id: number): Observable<TaiLieuDto> {
    return this.http.get<TaiLieuDto>(`${this.apiUrl}/${id}`);
  }

  uploadDocument(maNhom: number, tieuDe: string, moTa: string, file: File): Observable<TaiLieuDto> {
    const formData = new FormData();
    formData.append('maNhom', maNhom.toString());
    formData.append('tieuDe', tieuDe);
    formData.append('moTa', moTa);
    formData.append('file', file, file.name);

    return this.http.post<TaiLieuDto>(this.apiUrl, formData);
  }

  updateDocument(id: number, tieuDe: string, moTa: string): Observable<TaiLieuDto> {
    return this.http.put<TaiLieuDto>(`${this.apiUrl}/${id}`, { tieuDe, moTa });
  }

  deleteDocument(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadDocument(id: number, fileName: string): void {
    this.http.get(`${this.apiUrl}/${id}/download`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Lỗi khi tải xuống tài liệu:', err);
      }
    });
  }
}
