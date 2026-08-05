import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { DashboardService } from './dashboard.service';

export interface TaskDto {
  maCongViec: number;
  maNguoiDung: number;
  maMonHoc?: number;
  tenMonHoc?: string;
  maMon?: string;
  tieuDe: string;
  moTa?: string;
  doUuTien: number; // 0: Low, 1: Medium, 2: High, 3: Critical
  trangThai: number; // 0: Not started, 1: In progress, 2: Paused, 3: Completed, 4: Overdue
  ngayBatDau?: string;
  hanHoanThanh?: string;
  ngayHoanThanh?: string;
  tiLeHoanThanh: number;
  mauSac?: string;
  danhDauQuanTrong: boolean;
  danhDauYeuThich: boolean;
  ghiChu?: string;
}

export interface PagedList<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private apiUrl = 'http://localhost:5186/api/v1/tasks';
  private tasksCache = new Map<string, Observable<PagedList<TaskDto>>>();

  constructor(
    private http: HttpClient,
    private dashboardService: DashboardService
  ) {}

  getTasks(params: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    priority?: number;
    status?: number;
    subjectId?: number;
    sortBy?: string;
    sortDirection?: string;
  }): Observable<PagedList<TaskDto>> {
    let httpParams = new HttpParams();
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.priority !== undefined && params.priority !== null) httpParams = httpParams.set('priority', params.priority.toString());
    if (params.status !== undefined && params.status !== null) httpParams = httpParams.set('status', params.status.toString());
    if (params.subjectId !== undefined && params.subjectId !== null) httpParams = httpParams.set('subjectId', params.subjectId.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);

    const cacheKey = httpParams.toString();
    if (!this.tasksCache.has(cacheKey)) {
      const request$ = this.http.get<PagedList<TaskDto>>(this.apiUrl, { params: httpParams }).pipe(
        shareReplay(1)
      );
      this.tasksCache.set(cacheKey, request$);
    }
    return this.tasksCache.get(cacheKey)!;
  }

  clearCache() {
    this.tasksCache.clear();
    this.dashboardService.clearCache();
  }

  getTaskById(id: number): Observable<TaskDto> {
    return this.http.get<TaskDto>(`${this.apiUrl}/${id}`);
  }

  createTask(task: Partial<TaskDto>): Observable<TaskDto> {
    return this.http.post<TaskDto>(this.apiUrl, task).pipe(
      tap(() => this.clearCache())
    );
  }

  updateTask(id: number, task: Partial<TaskDto>): Observable<TaskDto> {
    return this.http.put<TaskDto>(`${this.apiUrl}/${id}`, task).pipe(
      tap(() => this.clearCache())
    );
  }

  updateTaskStatus(id: number, status: number): Observable<TaskDto> {
    return this.http.patch<TaskDto>(`${this.apiUrl}/${id}/status`, { trangThai: status }).pipe(
      tap(() => this.clearCache())
    );
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.clearCache())
    );
  }
}
