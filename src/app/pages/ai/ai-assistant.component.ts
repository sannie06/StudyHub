import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AiService, AiChatResponse, StudyPlanResponse, StudyPlanItem } from '../../services/ai.service';
import { DashboardService, DashboardData } from '../../services/dashboard.service';

export interface ChatSession {
  id: string;
  title: string;
  time: string;
  icon: string;
  messages: ChatMessage[];
  createdAt: number;
}

export interface QuickPromptItem {
  id: number;
  title: string;
  icon: string;
  iconBg: string;
  promptType?: string;
}

export interface PlanDayItem {
  dayTitle: string;
  topics: string;
  hours: string;
}

export interface AIRecommendationItem {
  id: number;
  title: string;
  desc: string;
  badge: string;
  badgeClass: string;
  icon: string;
}

export interface ChatMessage {
  sender: 'user' | 'ai';
  text: string;
  time: string;
  actionSuggestions?: string[];
  workloadLevel?: string;
  planItems?: PlanDayItem[];
}

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './ai-assistant.component.html',
  styles: [`
    .chat-container::-webkit-scrollbar,
    .custom-scrollbar::-webkit-scrollbar { width: 5px; }
    .chat-container::-webkit-scrollbar-track,
    .custom-scrollbar::-webkit-scrollbar-track { background: #f8fafc; border-radius: 9999px; }
    .chat-container::-webkit-scrollbar-thumb,
    .custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 9999px; }
    .chat-container::-webkit-scrollbar-thumb:hover,
    .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
  `]
})
export class AiAssistantComponent implements OnInit, AfterViewChecked {
  @ViewChild('chatContainer') private chatContainer?: ElementRef;
  private shouldScrollToBottom: boolean = false;

  activeTab: 'chat' | 'analytics' | 'recommendations' | 'help' = 'chat';
  promptInput: string = '';
  historySearchQuery: string = '';

  isAiThinking: boolean = false;
  errorMessage: string = '';

  chatSessions: ChatSession[] = [];
  activeSessionId: string = '';
  messages: ChatMessage[] = [];

  quickPrompts: QuickPromptItem[] = [
    { id: 1, title: 'Lập kế hoạch học tập', icon: 'pi-file-edit', iconBg: 'bg-purple-50 text-purple-600', promptType: 'WorkloadAnalysis' },
    { id: 2, title: 'Lịch học hôm nay', icon: 'pi-clock', iconBg: 'bg-amber-50 text-amber-600', promptType: 'TodaySchedule' },
    { id: 3, title: 'Xem deadline sắp tới', icon: 'pi-list-check', iconBg: 'bg-rose-50 text-rose-600', promptType: 'UpcomingDeadlines' },
    { id: 4, title: 'Gợi ý ưu tiên công việc', icon: 'pi-chart-line', iconBg: 'bg-emerald-50 text-emerald-600', promptType: 'PriorityTasks' },
    { id: 5, title: 'Phân tích tiến độ học tập', icon: 'pi-chart-bar', iconBg: 'bg-blue-50 text-blue-600', promptType: 'WorkloadAnalysis' },
    { id: 6, title: 'Tạo câu hỏi ôn tập', icon: 'pi-pencil', iconBg: 'bg-indigo-50 text-indigo-600' }
  ];

  planDays: PlanDayItem[] = [
    { dayTitle: 'Ngày 1: OOP & Class', topics: 'Class, Object, Inheritance, Polymorphism', hours: '2 - 3 giờ' },
    { dayTitle: 'Ngày 2: Collection Framework', topics: 'List, Set, Map, Iterator, Generics', hours: '2 - 3 giờ' },
    { dayTitle: 'Ngày 3: Exception Handling & File IO', topics: 'Try-catch, Custom Exception, File, Serialization', hours: '2 - 3 giờ' },
    { dayTitle: 'Ngày 4: JDBC & Database', topics: 'JDBC, Connection, Statement, ResultSet', hours: '2 - 3 giờ' },
    { dayTitle: 'Ngày 5: Multithreading', topics: 'Thread, Runnable, Synchronization', hours: '2 - 3 giờ' },
    { dayTitle: 'Ngày 6: Bài tập tổng hợp', topics: 'Làm bài tập tổng hợp các chủ đề đã học', hours: '3 - 4 giờ' },
    { dayTitle: 'Ngày 7: Ôn tập & Mock Test', topics: 'Ôn tập lý thuyết + làm đề thi thử', hours: '3 - 4 giờ' }
  ];

  recommendations: AIRecommendationItem[] = [
    { id: 1, title: 'Ưu tiên hôm nay', desc: 'Hoàn thành bài tập JDBC', badge: 'Quan trọng', badgeClass: 'bg-rose-100 text-rose-600', icon: 'pi-compass' },
    { id: 2, title: 'Pomodoro đề xuất', desc: 'Học Java 2 phiên (50 phút)', badge: 'Tối ưu', badgeClass: 'bg-blue-100 text-blue-600', icon: 'pi-clock' },
    { id: 3, title: 'Hoạt động nhóm', desc: 'Nhóm DATN cần họp trong tuần này', badge: 'Gợi ý', badgeClass: 'bg-purple-100 text-purple-700', icon: 'pi-users' },
    { id: 4, title: 'Lịch học gợi ý', desc: 'Ôn tập Java vào 20:00 tối nay', badge: 'Gợi ý', badgeClass: 'bg-purple-100 text-purple-700', icon: 'pi-calendar' }
  ];

  dashboardStats = {
    tongSoCongViec: 35,
    congViecHoanThanh: 28,
    completionRate: 80,
    quaHan: 2,
    streakDays: 7,
    performanceRate: 82,
    lastUpdatedTime: '10:30'
  };

  constructor(
    private aiService: AiService,
    private dashboardService: DashboardService,
    private router: Router
  ) {}

  onSelectRecommendation(rec: AIRecommendationItem) {
    if (rec.title.includes('Ưu tiên')) {
      this.sendPrompt('Phân tích mức độ quá tải và gợi ý thứ tự ưu tiên các công việc hôm nay', 'PriorityTasks');
    } else if (rec.title.includes('Pomodoro')) {
      this.router.navigate(['/pomodoro']);
    } else if (rec.title.includes('Hoạt động nhóm')) {
      this.router.navigate(['/groups']);
    } else if (rec.title.includes('Lịch học')) {
      this.router.navigate(['/calendar']);
    }
  }

  ngOnInit() {
    this.loadWorkloadAndAdvice();
    this.loadChatSessionsFromStorage();
    this.loadRealDashboardStats();
  }

  loadRealDashboardStats() {
    this.dashboardService.getDashboardData().subscribe({
      next: (res: DashboardData) => {
        if (res && res.statistics) {
          const total = res.statistics.tongSoCongViec || 0;
          const completed = res.statistics.congViecHoanThanh || 0;
          const rate = total > 0 ? Math.round((completed / total) * 100) : 0;
          const todayDeadline = res.statistics.deadlineHomNay || 0;

          const nowTime = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

          this.dashboardStats = {
            tongSoCongViec: total,
            congViecHoanThanh: completed,
            completionRate: rate,
            quaHan: todayDeadline,
            streakDays: 7,
            performanceRate: rate > 0 ? rate : (total > 0 ? rate : 82),
            lastUpdatedTime: nowTime
          };
        }
      },
      error: (err) => console.error('Error fetching real dashboard stats for AI component:', err)
    });
  }

  loadChatSessionsFromStorage() {
    const saved = localStorage.getItem('studyhub_ai_chat_sessions');
    if (saved) {
      try {
        this.chatSessions = JSON.parse(saved);
      } catch (e) {
        this.chatSessions = [];
      }
    }

    if (!this.chatSessions || this.chatSessions.length === 0) {
      const nowStr = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
      const defaultSession: ChatSession = {
        id: 'chat_' + Date.now(),
        title: 'Hội thoại mới',
        time: nowStr,
        icon: 'pi-comments',
        createdAt: Date.now(),
        messages: [
          {
            sender: 'ai',
            text: 'Xin chào! 👋 Tôi là Trợ lý AI cá nhân trên StudyHub. Tôi có thể giúp bạn lập kế hoạch học tập, kiểm tra deadline, hoặc phân tích tiến độ hôm nay.',
            time: nowStr,
            actionSuggestions: ['Hôm nay nên học gì?', 'Xem deadline sắp tới', 'Phân tích mức độ quá tải', 'Sinh kế hoạch học tập 7 ngày']
          }
        ]
      };
      this.chatSessions = [defaultSession];
      this.saveChatSessionsToStorage();
    }

    this.activeSessionId = this.chatSessions[0].id;
    this.messages = this.chatSessions[0].messages;
  }

  saveChatSessionsToStorage() {
    localStorage.setItem('studyhub_ai_chat_sessions', JSON.stringify(this.chatSessions));
  }

  get filteredChatHistory(): ChatSession[] {
    if (!this.historySearchQuery || !this.historySearchQuery.trim()) {
      return this.chatSessions;
    }
    const q = this.historySearchQuery.toLowerCase().trim();
    return this.chatSessions.filter(s => s.title.toLowerCase().includes(q));
  }

  selectHistory(id: string) {
    this.activeSessionId = id;
    const found = this.chatSessions.find(s => s.id === id);
    if (found) {
      this.messages = found.messages;
      this.shouldScrollToBottom = true;
    }
  }

  deleteHistory(id: string, event?: Event) {
    if (event) event.stopPropagation();
    this.chatSessions = this.chatSessions.filter(s => s.id !== id);
    if (this.chatSessions.length === 0) {
      this.startNewChat();
    } else {
      if (this.activeSessionId === id) {
        this.selectHistory(this.chatSessions[0].id);
      }
      this.saveChatSessionsToStorage();
    }
  }

  private syncActiveSession() {
    const active = this.chatSessions.find(s => s.id === this.activeSessionId);
    if (active) {
      active.messages = [...this.messages];
      const lastMsg = this.messages[this.messages.length - 1];
      if (lastMsg) {
        active.time = lastMsg.time;
      }
      if (active.title === 'Hội thoại mới' || active.title === 'Cuộc trò chuyện mới') {
        const userMsg = this.messages.find(m => m.sender === 'user');
        if (userMsg) {
          active.title = userMsg.text.length > 25 ? userMsg.text.substring(0, 25) + '...' : userMsg.text;
        }
      }
      this.saveChatSessionsToStorage();
    }
  }

  loadWorkloadAndAdvice() {
    this.aiService.analyzeWorkload().subscribe({
      next: (res) => {
        if (res && res.workloadAnalysis) {
          const firstLine = res.workloadAnalysis.split('\n').find(l => l.includes('MỨC ĐỘ')) || 'Mức độ cân bằng';
          this.recommendations[0].desc = firstLine.replace('MỨC ĐỘ: ', '');
        }
      },
      error: (err) => console.error('Error analyzing workload:', err)
    });

    this.aiService.getStudyAdvice().subscribe({
      next: (res) => {
        if (res && res.advice) {
          const lines = res.advice.split('\n').filter(l => l.trim().length > 0);
          if (lines.length > 0) {
            this.recommendations[1].desc = lines[0].replace(/^\d+\.\s*/, '');
          }
        }
      },
      error: (err) => console.error('Error fetching advice:', err)
    });
  }

  ngAfterViewChecked() {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  scrollToBottom(): void {
    try {
      if (this.chatContainer) {
        this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
      }
    } catch (err) {}
  }

  selectQuickPrompt(prompt: QuickPromptItem) {
    if (prompt.title.includes('kế hoạch học tập')) {
      this.triggerStudyPlan('Ôn tập kiến thức môn học trong 7 ngày');
    } else {
      this.sendPrompt(prompt.title, prompt.promptType);
    }
  }

  sendPrompt(textToSend?: string, promptType?: string) {
    const text = (textToSend || this.promptInput || '').trim();
    if (!text) return;

    const timeStr = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

    // Push User message
    this.messages.push({
      sender: 'user',
      text,
      time: timeStr
    });

    this.promptInput = '';
    this.isAiThinking = true;
    this.errorMessage = '';
    this.syncActiveSession();
    this.shouldScrollToBottom = true;

    this.aiService.chat({ message: text, promptType }).subscribe({
      next: (res: AiChatResponse) => {
        this.isAiThinking = false;
        this.messages.push({
          sender: 'ai',
          text: res.reply,
          time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
          actionSuggestions: res.actionSuggestions,
          workloadLevel: res.workloadLevel
        });
        this.syncActiveSession();
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isAiThinking = false;
        console.error('Error calling AI chat API:', err);
        if (err.status === 401) {
          this.messages.push({
            sender: 'ai',
            text: '⚠️ Bạn cần đăng nhập để tương tác với Trợ lý AI.',
            time: timeStr
          });
        } else {
          this.messages.push({
            sender: 'ai',
            text: '⚠️ Hệ thống AI tạm thời bận. Vui lòng thử lại sau giây lát.',
            time: timeStr
          });
        }
        this.syncActiveSession();
        this.shouldScrollToBottom = true;
      }
    });
  }

  triggerStudyPlan(goal: string) {
    const timeStr = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    this.messages.push({
      sender: 'user',
      text: `Lập kế hoạch học tập cho mục tiêu: ${goal}`,
      time: timeStr
    });

    this.isAiThinking = true;
    this.syncActiveSession();
    this.shouldScrollToBottom = true;

    this.aiService.generateStudyPlan({ goal, numberOfDays: 7 }).subscribe({
      next: (res: StudyPlanResponse) => {
        this.isAiThinking = false;
        const mappedItems: PlanDayItem[] = res.planItems.map(p => ({
          dayTitle: `${p.day}: ${p.taskName}`,
          topics: p.focusArea,
          hours: p.duration
        }));

        this.planDays = mappedItems;

        this.messages.push({
          sender: 'ai',
          text: res.advice || `Dưới đây là kế hoạch học tập 7 ngày cho mục tiêu: ${goal}`,
          time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
          planItems: mappedItems,
          actionSuggestions: ['Tạo lịch học', 'Điều chỉnh kế hoạch']
        });
        this.syncActiveSession();
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isAiThinking = false;
        console.error('Error generating study plan:', err);
        this.syncActiveSession();
        this.shouldScrollToBottom = true;
      }
    });
  }

  startNewChat() {
    const nowStr = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    const newSession: ChatSession = {
      id: 'chat_' + Date.now(),
      title: 'Hội thoại mới',
      time: nowStr,
      icon: 'pi-comments',
      createdAt: Date.now(),
      messages: [
        {
          sender: 'ai',
          text: 'Cuộc trò chuyện mới đã bắt đầu! Bạn muốn hỏi gì hôm nay?',
          time: nowStr,
          actionSuggestions: ['Hôm nay nên học gì?', 'Xem deadline sắp tới', 'Sinh kế hoạch 7 ngày']
        }
      ]
    };

    this.chatSessions.unshift(newSession);
    this.activeSessionId = newSession.id;
    this.messages = newSession.messages;
    this.saveChatSessionsToStorage();
    this.shouldScrollToBottom = true;
  }
}
