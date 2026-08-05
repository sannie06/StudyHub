import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { DashboardService } from './dashboard.service';

export interface SubjectDto {
  maMonHoc: number;
  tenMonHoc: string;
  maMon: string;
  moTa?: string;
  mauSac: string;
  icon: string;
  trangThai: number;
  taskCount: number;
  progress: number;
}

@Injectable({
  providedIn: 'root'
})
export class SubjectService {
  private apiUrl = 'http://localhost:5186/api/v1/subjects';
  private subjectsCache$: Observable<SubjectDto[]> | null = null;

  constructor(
    private http: HttpClient,
    private dashboardService: DashboardService
  ) {}

  getSubjects(): Observable<SubjectDto[]> {
    if (!this.subjectsCache$) {
      this.subjectsCache$ = this.http.get<SubjectDto[]>(this.apiUrl).pipe(
        shareReplay(1)
      );
    }
    return this.subjectsCache$;
  }

  clearCache() {
    this.subjectsCache$ = null;
    this.dashboardService.clearCache();
  }

  createSubject(subject: Partial<SubjectDto>): Observable<SubjectDto> {
    return this.http.post<SubjectDto>(this.apiUrl, subject).pipe(
      tap(() => this.clearCache())
    );
  }

  updateSubject(id: number, subject: Partial<SubjectDto>): Observable<SubjectDto> {
    return this.http.put<SubjectDto>(`${this.apiUrl}/${id}`, subject).pipe(
      tap(() => this.clearCache())
    );
  }

  deleteSubject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.clearCache())
    );
  }
}
