import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AiService, AiChatResponse, StudyPlanResponse, StudyPlanItem } from '../../services/ai.service';

export interface ChatHistoryItem {
  id: number;
  title: string;
  time: string;
  icon: string;
  isActive?: boolean;
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
    .chat-container::-webkit-scrollbar { width: 4px; }
    .chat-container::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 9999px; }
  `]
})
export class AiAssistantComponent implements OnInit {
  activeTab: 'chat' | 'analytics' | 'recommendations' | 'help' = 'chat';
  promptInput: string = '';

  isAiThinking: boolean = false;
  errorMessage: string = '';

  messages: ChatMessage[] = [
    {
      sender: 'ai',
      text: 'Xin chào! 👋 Tôi là Trợ lý AI cá nhân trên StudyHub. Tôi có thể giúp bạn lập kế hoạch học tập, kiểm tra deadline, hoặc phân tích tiến độ hôm nay.',
      time: '10:30',
      actionSuggestions: ['Hôm nay nên học gì?', 'Xem deadline sắp tới', 'Phân tích mức độ quá tải', 'Sinh kế hoạch học tập 7 ngày']
    }
  ];

  chatHistory: ChatHistoryItem[] = [
    { id: 1, title: 'Lập kế hoạch ôn thi Java', time: '10:30', icon: 'pi-comments', isActive: true },
    { id: 2, title: 'Tóm tắt tài liệu Cơ sở dữ liệu', time: '09:15', icon: 'pi-file' },
    { id: 3, title: 'Phân tích tiến độ học tập', time: 'Hôm qua', icon: 'pi-chart-bar' },
    { id: 4, title: 'Gợi ý bài tập DSA', time: 'Hôm qua', icon: 'pi-code' },
    { id: 5, title: 'Hỗ trợ nhóm DATN', time: '2 ngày trước', icon: 'pi-users' },
    { id: 6, title: 'Giải thích thuật toán BFS', time: '3 ngày trước', icon: 'pi-code' },
    { id: 7, title: 'Lịch học tuần tới', time: '3 ngày trước', icon: 'pi-calendar' }
  ];

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

  constructor(private aiService: AiService) {}

  ngOnInit() {
    this.loadWorkloadAndAdvice();
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

  selectHistory(id: number) {
    this.chatHistory.forEach(h => h.isActive = (h.id === id));
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
      },
      error: (err) => {
        this.isAiThinking = false;
        console.error('Error generating study plan:', err);
      }
    });
  }

  startNewChat() {
    this.messages = [
      {
        sender: 'ai',
        text: 'Cuộc trò chuyện mới đã bắt đầu! Bạn muốn hỏi gì hôm nay?',
        time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
        actionSuggestions: ['Hôm nay nên học gì?', 'Xem deadline sắp tới', 'Sinh kế hoạch 7 ngày']
      }
    ];
  }
}
