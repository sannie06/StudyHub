import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TepDinhKemChatDto {
  maFile: number;
  tenFile: string;
  duongDan: string;
  dungLuong: number;
  dinhDang: string;
}

export interface TinNhanDto {
  maTinNhan: number;
  maNhom: number;
  maNguoiGui: number;
  tenNguoiGui: string;
  avatarNguoiGui?: string;
  noiDung: string;
  loaiTinNhan: number; // 0: Text, 1: Image, 2: File, 3: System
  daChinhSua: boolean;
  ngayGui: string;
  isMine: boolean;
  attachment?: TepDinhKemChatDto;
}

export interface SendChatMessageRequest {
  maNhom: number;
  noiDung: string;
  loaiTinNhan?: number;
}

export interface TypingNotificationDto {
  maNhom: number;
  maNguoiDung: number;
  tenNguoiDung: string;
  isTyping: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = 'http://localhost:5186/api/v1/groups';

  constructor(private http: HttpClient) {}

  getGroupMessages(groupId: number, page = 1, pageSize = 50): Observable<TinNhanDto[]> {
    return this.http.get<TinNhanDto[]>(`${this.apiUrl}/${groupId}/messages?page=${page}&pageSize=${pageSize}`);
  }

  sendMessage(groupId: number, request: SendChatMessageRequest): Observable<TinNhanDto> {
    return this.http.post<TinNhanDto>(`${this.apiUrl}/${groupId}/messages`, request);
  }

  uploadFileMessage(groupId: number, file: File, content?: string): Observable<TinNhanDto> {
    const formData = new FormData();
    formData.append('file', file);
    if (content) {
      formData.append('content', content);
    }
    return this.http.post<TinNhanDto>(`${this.apiUrl}/${groupId}/messages/upload`, formData);
  }

  getPinnedAnnouncement(groupId: number): Observable<{ announcement: string }> {
    return this.http.get<{ announcement: string }>(`${this.apiUrl}/${groupId}/pinned-announcement`);
  }

  updatePinnedAnnouncement(groupId: number, announcement: string): Observable<{ success: boolean; announcement: string }> {
    return this.http.put<{ success: boolean; announcement: string }>(`${this.apiUrl}/${groupId}/pinned-announcement`, { announcement });
  }

  deleteMessage(messageId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/messages/${messageId}`);
  }
}
