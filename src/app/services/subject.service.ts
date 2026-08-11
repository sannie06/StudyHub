import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { shareReplay, tap, catchError, map } from 'rxjs/operators';
import { DashboardService } from './dashboard.service';

export interface SubjectDto {
  maMonHoc: number;
  tenMonHoc: string;
  maMon: string;
  moTa?: string;
  mauSac: string;
  icon: string;
  trangThai: number;
  taskCount?: number;
  progress?: number;
}

export interface SubjectTag {
  id: number;
  name: string;
  color: string;
  code?: string;
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

  clearCache() {
    this.subjectsCache$ = null;
    this.dashboardService.clearCache();
  }

  getSubjects(): Observable<SubjectDto[]> {
    if (!this.subjectsCache$) {
      this.subjectsCache$ = this.http.get<SubjectDto[]>(this.apiUrl).pipe(
        shareReplay(1)
      );
    }
    return this.subjectsCache$;
  }

  // Load Real Subject Tags directly from SQL Server Database via API
  getSubjectTags(): Observable<SubjectTag[]> {
    return this.getSubjects().pipe(
      map(dtos => {
        if (!dtos || !Array.isArray(dtos)) return [];
        return dtos.map(d => ({
          id: d.maMonHoc,
          name: d.tenMonHoc,
          color: d.mauSac || '#6366F1',
          code: d.maMon
        }));
      }),
      catchError(err => {
        console.warn('API getSubjects error:', err);
        return of([]);
      })
    );
  }

  createSubject(subject: Partial<SubjectDto>): Observable<SubjectDto> {
    return this.http.post<SubjectDto>(this.apiUrl, subject).pipe(
      tap(() => this.clearCache())
    );
  }

  createSubjectTag(name: string, color: string): Observable<SubjectTag> {
    const trimmedName = name.trim();
    // Generate valid clean unique course code (MaMon)
    const normalizedCode = this.generateSubjectCode(trimmedName);

    const payload: Partial<SubjectDto> = {
      tenMonHoc: trimmedName,
      maMon: normalizedCode,
      mauSac: color.startsWith('#') ? color : '#6366F1',
      icon: 'pi-tag',
      moTa: `Môn học ${trimmedName}`
    };

    return this.createSubject(payload).pipe(
      map(dto => ({
        id: dto.maMonHoc,
        name: dto.tenMonHoc,
        color: dto.mauSac || color,
        code: dto.maMon
      }))
    );
  }

  addSubjectTag(name: string, color: string): SubjectTag {
    const trimmedName = name.trim();
    const tempTag: SubjectTag = {
      id: Date.now() % 100000,
      name: trimmedName,
      color: color.startsWith('#') ? color : '#6366F1'
    };

    // Save to Database via API
    this.createSubjectTag(trimmedName, color).subscribe({
      next: (created) => {
        tempTag.id = created.id;
        tempTag.code = created.code;
      },
      error: (err) => console.warn('Could not save subject tag to Database:', err)
    });

    return tempTag;
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

  private generateSubjectCode(name: string): string {
    // Remove Vietnamese accents and special characters
    const cleanStr = name.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/[^a-zA-Z0-9\s]/g, '');
    const words = cleanStr.split(/\s+/).filter(w => w.length > 0);
    let codePrefix = '';
    if (words.length >= 2) {
      codePrefix = words.map(w => w[0].toUpperCase()).join('');
    } else if (words.length === 1) {
      codePrefix = words[0].substring(0, Math.min(4, words[0].length)).toUpperCase();
    } else {
      codePrefix = 'MH';
    }
    const randSuffix = Math.floor(100 + Math.random() * 900);
    return `${codePrefix}_${randSuffix}`;
  }
}
