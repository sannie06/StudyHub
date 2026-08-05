import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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

  deleteMessage(messageId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/messages/${messageId}`);
  }
}
