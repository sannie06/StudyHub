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
  taskCount: number;
  progress: number;
}

export interface SubjectTag {
  id: number;
  name: string;
  color: string;
}

@Injectable({
  providedIn: 'root'
})
export class SubjectService {
  private apiUrl = 'http://localhost:5186/api/v1/subjects';
  private subjectsCache$: Observable<SubjectDto[]> | null = null;
  private localTagsKey = 'studyhub_shared_subject_tags_v1';

  // System default subject tags catalog
  private defaultSubjectTags: SubjectTag[] = [
    { id: 1, name: 'Cơ sở dữ liệu', color: '#3B82F6' },
    { id: 2, name: 'Cấu trúc dữ liệu và giải thuật', color: '#F59E0B' },
    { id: 3, name: 'Lập trình Web', color: '#10B981' },
    { id: 4, name: 'Tiếng Anh 2', color: '#8B5CF6' },
    { id: 5, name: 'PTPM', color: '#6366F1' },
    { id: 6, name: 'Java', color: '#10B981' },
    { id: 7, name: 'Kỹ năng mềm', color: '#EC4899' },
    { id: 8, name: 'Công nghệ phần mềm', color: '#14B8A6' },
    { id: 9, name: 'Thiết kế', color: '#F97316' },
    { id: 10, name: 'Toán', color: '#14B8A6' }
  ];

  constructor(
    private http: HttpClient,
    private dashboardService: DashboardService
  ) {}

  // Local storage helpers
  getLocalSubjectTags(): SubjectTag[] {
    try {
      const data = localStorage.getItem(this.localTagsKey);
      if (data) {
        return JSON.parse(data);
      }
    } catch (e) {
      console.error('Error reading subject tags from localStorage:', e);
    }
    return this.defaultSubjectTags;
  }

  saveLocalSubjectTags(tags: SubjectTag[]): void {
    try {
      localStorage.setItem(this.localTagsKey, JSON.stringify(tags));
    } catch (e) {
      console.error('Error saving subject tags to localStorage:', e);
    }
  }

  getSubjects(): Observable<SubjectDto[]> {
    if (!this.subjectsCache$) {
      this.subjectsCache$ = this.http.get<SubjectDto[]>(this.apiUrl).pipe(
        shareReplay(1)
      );
    }
    return this.subjectsCache$;
  }

  // Unified SubjectTag[] getter merging API + LocalStorage
  getSubjectTags(): Observable<SubjectTag[]> {
    return this.getSubjects().pipe(
      map(dtos => {
        const localList = this.getLocalSubjectTags();
        const apiTags: SubjectTag[] = dtos.map(d => ({
          id: d.maMonHoc,
          name: d.tenMonHoc,
          color: d.mauSac || '#6366F1'
        }));

        // Merge without duplicates by name (case-insensitive)
        const tagMap = new Map<string, SubjectTag>();
        localList.forEach(t => tagMap.set(t.name.trim().toLowerCase(), t));
        apiTags.forEach(t => tagMap.set(t.name.trim().toLowerCase(), t));

        const merged = Array.from(tagMap.values());
        this.saveLocalSubjectTags(merged);
        return merged;
      }),
      catchError(err => {
        console.warn('API getSubjects failed, returning local subject tags:', err);
        return of(this.getLocalSubjectTags());
      })
    );
  }

  addSubjectTag(name: string, color: string): SubjectTag {
    const trimmedName = name.trim();
    const currentTags = this.getLocalSubjectTags();
    const existing = currentTags.find(t => t.name.toLowerCase() === trimmedName.toLowerCase());

    if (existing) {
      existing.color = color;
      this.saveLocalSubjectTags(currentTags);
      return existing;
    }

    const newId = currentTags.length > 0 ? Math.max(...currentTags.map(t => t.id)) + 1 : 1;
    const newTag: SubjectTag = {
      id: newId,
      name: trimmedName,
      color: color || '#6366F1'
    };

    currentTags.push(newTag);
    this.saveLocalSubjectTags(currentTags);

    // Sync to backend API async
    const dtoPayload: Partial<SubjectDto> = {
      tenMonHoc: trimmedName,
      maMon: trimmedName.length >= 3 ? trimmedName.substring(0, 3).toUpperCase() : trimmedName.toUpperCase(),
      mauSac: color || '#6366F1',
      icon: 'book',
      trangThai: 1
    };

    this.createSubject(dtoPayload).subscribe({
      next: (created) => console.log('Synced subject to API:', created),
      error: (err) => console.warn('API subject sync skipped:', err)
    });

    return newTag;
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
