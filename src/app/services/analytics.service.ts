import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

export interface SubjectProgressDto {
  maMonHoc: number;
  tenMonHoc: string;
  maMon: string;
  mauSac: string;
  taskCount: number;
  progress: number;
}

export interface WeeklyActivityDto {
  dayName: string;
  date: string;
  focusMinutes: number;
  completedTasks: number;
}

export interface HeatMapEntryDto {
  date: string;
  value: number;
}

export interface UpcomingDeadlineDto {
  maCongViec: number;
  tieuDe: string;
  tenMonHoc: string;
  hanHoanThanh?: string;
  doUuTien: number;
  priorityLabel: string;
  dueLabel: string;
  isOverdue: boolean;
}

export interface AnalyticsDto {
  totalFocusMinutes: number;
  totalPomodoros: number;
  totalTasks: number;
  completedTasks: number;
  overdueTasks: number;
  taskCompletionRate: number;
  currentStreak: number;
  productivityScore: number;
  subjectProgress: SubjectProgressDto[];
  weeklyActivity: WeeklyActivityDto[];
  heatMap: HeatMapEntryDto[];
  upcomingDeadlines: UpcomingDeadlineDto[];
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private apiUrl = 'http://localhost:5186/api/v1/analytics';
  private analyticsCache$: Observable<AnalyticsDto> | null = null;

  constructor(private http: HttpClient) {}

  getAnalytics(): Observable<AnalyticsDto> {
    if (!this.analyticsCache$) {
      this.analyticsCache$ = this.http.get<AnalyticsDto>(this.apiUrl).pipe(
        shareReplay(1)
      );
    }
    return this.analyticsCache$;
  }

  clearCache() {
    this.analyticsCache$ = null;
  }
}
