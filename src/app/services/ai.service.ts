import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AiChatRequest {
  message: string;
  promptType?: string;
}

export interface AiChatResponse {
  reply: string;
  actionSuggestions: string[];
  workloadLevel?: string;
  intent?: string;
  requiredInformation?: string[];
}

export interface StudyPlanRequest {
  goal: string;
  numberOfDays: number;
}

export interface StudyPlanItem {
  day: string;
  taskName: string;
  duration: string;
  focusArea: string;
}

export interface StudyPlanResponse {
  title: string;
  advice: string;
  planItems: StudyPlanItem[];
}

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private apiUrl = 'http://localhost:5186/api/v1/ai';

  constructor(private http: HttpClient) {}

  chat(request: AiChatRequest): Observable<AiChatResponse> {
    return this.http.post<AiChatResponse>(`${this.apiUrl}/chat`, request);
  }

  generateStudyPlan(request: StudyPlanRequest): Observable<StudyPlanResponse> {
    return this.http.post<StudyPlanResponse>(`${this.apiUrl}/study-plan`, request);
  }

  analyzeWorkload(): Observable<{ workloadAnalysis: string }> {
    return this.http.get<{ workloadAnalysis: string }>(`${this.apiUrl}/workload`);
  }

  getStudyAdvice(): Observable<{ advice: string }> {
    return this.http.get<{ advice: string }>(`${this.apiUrl}/advice`);
  }
}
