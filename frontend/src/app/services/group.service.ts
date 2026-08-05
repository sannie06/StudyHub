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
}
