import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskDto } from './task.service';

export interface KanbanCardDto {
  maThe: number;
  maCot: number;
  maCongViec: number;
  thuTu: number;
  task: TaskDto;
}

export interface KanbanColumnDto {
  maCot: number;
  tenCot: string;
  mauSac?: string;
  thuTu: number;
  gioiHanThe?: number;
  cards: KanbanCardDto[];
}

export interface KanbanBoardDto {
  maBoard: number;
  tenBoard: string;
  moTa?: string;
  mauSac?: string;
  macDinh: boolean;
  columns: KanbanColumnDto[];
}

export interface CardPositionDto {
  maThe: number;
  maCot: number;
  thuTu: number;
  newTaskStatus?: number;
}

export interface MoveCardRequest {
  cardPositions: CardPositionDto[];
}

@Injectable({
  providedIn: 'root'
})
export class KanbanService {
  private apiUrl = 'http://localhost:5186/api/v1/kanban';

  constructor(private http: HttpClient) {}

  getBoards(): Observable<Partial<KanbanBoardDto>[]> {
    return this.http.get<Partial<KanbanBoardDto>[]>(`${this.apiUrl}/boards`);
  }

  getBoardDetails(boardId: number): Observable<KanbanBoardDto> {
    return this.http.get<KanbanBoardDto>(`${this.apiUrl}/boards/${boardId}`);
  }

  moveCards(request: MoveCardRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/cards/move`, request);
  }

  createColumn(boardId: number, name: string, color?: string): Observable<KanbanColumnDto> {
    return this.http.post<KanbanColumnDto>(`${this.apiUrl}/boards/${boardId}/columns`, { tenCot: name, mauSac: color });
  }

  deleteColumn(columnId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/columns/${columnId}`);
  }
}
