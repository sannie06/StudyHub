import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface NhomHocTapDto {
  maNhom: number;
  maNguoiTao: number;
  tenNguoiTao: string;
  maMonHoc?: number;
  tenMonHoc?: string;
  tenNhom: string;
  moTa: string;
  anhDaiDien: string;
  maThamGia: string;
  soLuongToiDa: number;
  soThanhVienHienTai: number;
  trangThai: number;
  isOwner: boolean;
  isMember: boolean;
}

export interface ThanhVienNhomDto {
  maThanhVien: number;
  maNhom: number;
  maNguoiDung: number;
  hoTen: string;
  email: string;
  avatar?: string;
  vaiTro: number; // 0: Member, 1: Moderator, 2: Owner
  trangThai: number;
  ngayThamGia: string;
}

export interface CreateStudyGroupRequest {
  tenNhom: string;
  moTa?: string;
  maMonHoc?: number;
  anhDaiDien?: string;
  soLuongToiDa: number;
}

export interface UpdateStudyGroupRequest {
  tenNhom: string;
  moTa?: string;
  maMonHoc?: number;
  anhDaiDien?: string;
  soLuongToiDa: number;
}

export interface GroupTaskBackendDto {
  maCongViec: number;
  maNhomHocTap: number;
  tieuDe: string;
  moTa: string;
  doUuTien: number; // 0: Thap, 1: Trung binh, 2: Cao
  trangThai: number; // 0: todo, 1: inProgress, 3: done
  ngayBatDau?: string;
  hanHoanThanh?: string;
  maNguoiDuocGiao?: number;
  tenNguoiDuocGiao?: string;
  anhNguoiDuocGiao?: string;
  nguoiTaoId: number;
  tenNguoiTao?: string;
  anhNguoiTao?: string;
  ngayTao: string;
}

export interface CreateGroupTaskBackendRequest {
  tieuDe: string;
  moTa?: string;
  doUuTien: number;
  hanHoanThanh?: string;
  maNguoiDuocGiao?: number;
  trangThai?: number;
}

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  private apiUrl = 'http://localhost:5186/api/v1/groups';

  constructor(private http: HttpClient) {}

  getMyGroups(search?: string): Observable<NhomHocTapDto[]> {
    let url = this.apiUrl;
    if (search) {
      url += `?search=${encodeURIComponent(search)}`;
    }
    return this.http.get<NhomHocTapDto[]>(url);
  }

  getGroupById(id: number): Observable<NhomHocTapDto> {
    return this.http.get<NhomHocTapDto>(`${this.apiUrl}/${id}`);
  }

  createGroup(request: CreateStudyGroupRequest): Observable<NhomHocTapDto> {
    return this.http.post<NhomHocTapDto>(this.apiUrl, request);
  }

  updateGroup(id: number, request: UpdateStudyGroupRequest): Observable<NhomHocTapDto> {
    return this.http.put<NhomHocTapDto>(`${this.apiUrl}/${id}`, request);
  }

  deleteGroup(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  joinGroup(maThamGia: string): Observable<NhomHocTapDto> {
    return this.http.post<NhomHocTapDto>(`${this.apiUrl}/join`, { maThamGia });
  }

  leaveGroup(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/leave`, {});
  }

  getMembers(id: number): Observable<ThanhVienNhomDto[]> {
    return this.http.get<ThanhVienNhomDto[]>(`${this.apiUrl}/${id}/members`);
  }

  addMember(id: number, memberUserId: number): Observable<ThanhVienNhomDto> {
    return this.http.post<ThanhVienNhomDto>(`${this.apiUrl}/${id}/members`, memberUserId);
  }

  removeMember(id: number, memberUserId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/members/${memberUserId}`);
  }

  // ── Group Tasks APIs ──
  getGroupTasks(id: number): Observable<GroupTaskBackendDto[]> {
    return this.http.get<GroupTaskBackendDto[]>(`${this.apiUrl}/${id}/tasks`);
  }

  createGroupTask(id: number, request: CreateGroupTaskBackendRequest): Observable<GroupTaskBackendDto> {
    return this.http.post<GroupTaskBackendDto>(`${this.apiUrl}/${id}/tasks`, request);
  }

  updateGroupTaskStatus(id: number, taskId: number, trangThai: number): Observable<GroupTaskBackendDto> {
    return this.http.patch<GroupTaskBackendDto>(`${this.apiUrl}/${id}/tasks/${taskId}/status`, { trangThai });
  }

  deleteGroupTask(id: number, taskId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/tasks/${taskId}`);
  }

  getGroupMeetings(id: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/meetings`);
  }

  createGroupMeeting(id: number, request: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/meetings`, request);
  }

  updateGroupMeeting(id: number, meetingId: number, request: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/meetings/${meetingId}`, request);
  }

  deleteGroupMeeting(id: number, meetingId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/meetings/${meetingId}`);
  }

  getGroupFolders(id: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/folders`);
  }

  createGroupFolder(id: number, request: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/folders`, request);
  }

  updateGroupFolder(id: number, folderId: number, request: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/folders/${folderId}`, request);
  }

  deleteGroupFolder(id: number, folderId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/folders/${folderId}`);
  }

  getGroupDocuments(id: number, folderId?: number): Observable<any[]> {
    let url = `${this.apiUrl}/${id}/documents`;
    if (folderId && folderId > 0) {
      url += `?folderId=${folderId}`;
    }
    return this.http.get<any[]>(url);
  }

  createGroupDocument(id: number, request: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/documents`, request);
  }

  deleteGroupDocument(id: number, documentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/documents/${documentId}`);
  }

  downloadGroupDocument(id: number, documentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/documents/${documentId}/download`, {
      responseType: 'blob'
    });
  }
}
