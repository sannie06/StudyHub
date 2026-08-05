import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { GroupService, NhomHocTapDto, ThanhVienNhomDto, CreateStudyGroupRequest } from '../../services/group.service';
import { ChatService, TinNhanDto } from '../../services/chat.service';
import { ChatSignalRService } from '../../services/chat-signalr.service';

export interface GroupItem {
  id: number;
  name: string;
  code: string;
  iconBg: string;
  iconColor: string;
  isActive?: boolean;
  leader?: string;
  description?: string;
  membersCount?: number;
}

export interface ChatMessage {
  id: number;
  senderName: string;
  senderAvatar: string;
  time: string;
  content: string;
  isMe: boolean;
  attachment?: {
    fileName: string;
    fileSize: string;
  };
  reaction?: {
    emoji: string;
    count: number;
  };
}

export interface TaskItemSummary {
  id: number;
  title: string;
  assigneeAvatar: string;
  assigneeName: string;
  dueDate?: string;
  priority?: string;
  priorityClass?: string;
  completed: boolean;
}

@Component({
  selector: 'app-groups',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './groups.component.html',
  styles: [`
    .chat-scroll::-webkit-scrollbar { width: 4px; }
    .chat-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 9999px; }
  `]
})
export class GroupsComponent implements OnInit, OnDestroy {
  activeTab: 'overview' | 'chat' | 'docs' | 'tasks' | 'meetings' | 'members' | 'settings' = 'overview';
  activeGroupId: number = 0;
  newMessageText: string = '';

  // Loading and Error States
  loadingGroups: boolean = false;
  loadingMessages: boolean = false;
  loadingMembers: boolean = false;
  errorMessage: string = '';

  // Dropdown select state
  showGroupDropdown: boolean = false;

  // Collapsible sidebar states
  isLeftSidebarCollapsed: boolean = false;
  isRightSidebarCollapsed: boolean = false;

  // Real Groups List from API
  groupsList: GroupItem[] = [];
  rawGroups: NhomHocTapDto[] = [];

  // Real Chat Messages from API + SignalR
  chatMessages: ChatMessage[] = [];

  // Real Members List from API
  membersList: { name: string; role: string; avatar: string; status: string; statusClass: string }[] = [];

  // Subscriptions
  private subscriptions: Subscription = new Subscription();

  todoTasks: TaskItemSummary[] = [
    { id: 1, title: 'Thiết kế giao diện đăng nhập', assigneeAvatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=80&auto=format&fit=crop&q=80', assigneeName: 'Lê Văn C', dueDate: '30/06', priority: 'Cao', priorityClass: 'bg-rose-100 text-rose-600', completed: false },
    { id: 2, title: 'Viết tài liệu yêu cầu', assigneeAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=80&auto=format&fit=crop&q=80', assigneeName: 'Phạm Minh D', dueDate: '02/07', priority: 'Trung bình', priorityClass: 'bg-amber-100 text-amber-600', completed: false },
    { id: 3, title: 'Xây dựng API tìm kiếm', assigneeAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=80&auto=format&fit=crop&q=80', assigneeName: 'Trần Thị B', dueDate: '05/07', priority: 'Cao', priorityClass: 'bg-rose-100 text-rose-600', completed: false }
  ];

  inProgressTasks: TaskItemSummary[] = [
    { id: 4, title: 'Thiết kế database', assigneeAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=80&auto=format&fit=crop&q=80', assigneeName: 'Trần Thị B', dueDate: '28/06', priority: 'Cao', priorityClass: 'bg-rose-100 text-rose-600', completed: false },
    { id: 5, title: 'Giao diện trang chủ', assigneeAvatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=80&auto=format&fit=crop&q=80', assigneeName: 'Lê Văn C', dueDate: '28/06', priority: 'Trung bình', priorityClass: 'bg-amber-100 text-amber-600', completed: false },
    { id: 6, title: 'Chức năng mượn sách', assigneeAvatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=80&auto=format&fit=crop&q=80', assigneeName: 'Nguyễn Văn A', dueDate: '01/07', priority: 'Cao', priorityClass: 'bg-rose-100 text-rose-600', completed: false }
  ];

  doneTasks: TaskItemSummary[] = [
    { id: 7, title: 'Phân tích yêu cầu', assigneeAvatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=80&auto=format&fit=crop&q=80', assigneeName: 'Nguyễn Văn A', completed: true },
    { id: 8, title: 'Use Case', assigneeAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=80&auto=format&fit=crop&q=80', assigneeName: 'Phạm Minh D', completed: true },
    { id: 9, title: 'Thiết kế sơ đồ ERD', assigneeAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=80&auto=format&fit=crop&q=80', assigneeName: 'Trần Thị B', completed: true }
  ];

  upcomingMeetings = [
    { title: 'Họp review tiến độ', time: 'Hôm nay, 20:00 - 21:00', platform: 'Google Meet', link: 'https://meet.google.com/abc-defg-hij' },
    { title: 'Họp thống nhất giao diện', time: '30/06/2024, 20:00 - 21:00', platform: 'Zoom Meeting', link: 'https://zoom.us/j/1234567890' },
    { title: 'Họp tổng kết tuần', time: '06/07/2024, 19:30 - 20:30', platform: 'Google Meet', link: 'https://meet.google.com/xyz-uvwx-yza' }
  ];

  docsList = [
    { name: 'Slide báo cáo tiến độ.pptx', size: '12.4 MB', type: 'ppt', updatedBy: 'Nguyễn Minh Anh', updatedAt: '2 giờ trước' },
    { name: 'database_design.sql', size: '2.4 MB', type: 'sql', updatedBy: 'Trần Thị B', updatedAt: 'Hôm qua' },
    { name: 'SRS_Document_V1.pdf', size: '4.8 MB', type: 'pdf', updatedBy: 'Phạm Minh D', updatedAt: '3 ngày trước' },
    { name: 'API_Contracts_StudyHub.json', size: '150 KB', type: 'json', updatedBy: 'Trần Thị B', updatedAt: '5 ngày trước' },
  ];

  foldersList = [
    { name: 'Tài liệu phân tích & đặc tả', filesCount: 5, color: 'text-blue-500' },
    { name: 'Bản vẽ thiết kế & Wireframe', filesCount: 8, color: 'text-purple-500' },
    { name: 'Mã nguồn & Script DB', filesCount: 3, color: 'text-emerald-500' }
  ];

  constructor(
    private groupService: GroupService,
    private chatService: ChatService,
    private chatSignalRService: ChatSignalRService
  ) {}

  get activeGroup(): GroupItem {
    return this.groupsList.find(g => g.id === this.activeGroupId) || this.groupsList[0] || {
      id: 0,
      name: 'Chưa chọn nhóm',
      code: 'NONE',
      iconBg: 'bg-purple-100',
      iconColor: 'text-purple-600',
      leader: 'N/A',
      description: 'Chưa có thông tin nhóm',
      membersCount: 0
    };
  }

  ngOnInit(): void {
    // 1. Initialize SignalR Connection
    this.chatSignalRService.startConnection();

    // 2. Subscribe to incoming SignalR messages
    this.subscriptions.add(
      this.chatSignalRService.message$.subscribe((dto: TinNhanDto) => {
        if (dto.maNhom === this.activeGroupId) {
          this.appendRealtimeMessage(dto);
        }
      })
    );

    // 3. Load my study groups from Backend
    this.loadMyGroups();
  }

  ngOnDestroy(): void {
    if (this.activeGroupId > 0) {
      this.chatSignalRService.leaveGroupChat(this.activeGroupId);
    }
    this.chatSignalRService.stopConnection();
    this.subscriptions.unsubscribe();
  }

  loadMyGroups(): void {
    this.loadingGroups = true;
    this.errorMessage = '';

    this.groupService.getMyGroups().subscribe({
      next: (groups: NhomHocTapDto[]) => {
        this.loadingGroups = false;
        this.rawGroups = groups;

        const iconStyles = [
          { bg: 'bg-purple-100', color: 'text-purple-600' },
          { bg: 'bg-emerald-100', color: 'text-emerald-600' },
          { bg: 'bg-amber-100', color: 'text-amber-600' },
          { bg: 'bg-rose-100', color: 'text-rose-600' },
          { bg: 'bg-blue-100', color: 'text-blue-600' }
        ];

        this.groupsList = groups.map((g, idx) => ({
          id: g.maNhom,
          name: g.tenNhom,
          code: g.maThamGia || `GRP00${g.maNhom}`,
          iconBg: iconStyles[idx % iconStyles.length].bg,
          iconColor: iconStyles[idx % iconStyles.length].color,
          isActive: idx === 0,
          leader: g.tenNguoiTao || 'Nhóm trưởng',
          description: g.moTa || 'Nhóm học tập thông minh 📚',
          membersCount: g.soThanhVienHienTai || 1
        }));

        if (this.groupsList.length > 0) {
          this.selectGroup(this.groupsList[0].id);
        }
      },
      error: (err) => {
        this.loadingGroups = false;
        console.error('Error fetching groups:', err);
        this.errorMessage = err?.error?.message || 'Không thể tải danh sách nhóm học tập.';
      }
    });
  }

  selectGroup(id: number): void {
    if (this.activeGroupId > 0) {
      this.chatSignalRService.leaveGroupChat(this.activeGroupId);
    }

    this.activeGroupId = id;
    this.groupsList.forEach(g => g.isActive = (g.id === id));
    this.showGroupDropdown = false;

    // Join SignalR group room
    this.chatSignalRService.joinGroupChat(id);

    // Fetch members and messages
    this.loadGroupMembers(id);
    this.loadGroupMessages(id);
  }

  loadGroupMembers(groupId: number): void {
    this.loadingMembers = true;
    this.groupService.getMembers(groupId).subscribe({
      next: (members: ThanhVienNhomDto[]) => {
        this.loadingMembers = false;
        const defaultAvatars = [
          'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=80&auto=format&fit=crop&q=80',
          'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=80&auto=format&fit=crop&q=80',
          'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=80&auto=format&fit=crop&q=80',
          'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=80&auto=format&fit=crop&q=80'
        ];

        this.membersList = members.map((m, idx) => ({
          name: m.hoTen || m.email,
          role: m.vaiTro === 2 ? 'Nhóm trưởng' : m.vaiTro === 1 ? 'Quản trị viên' : 'Thành viên',
          avatar: m.avatar || defaultAvatars[idx % defaultAvatars.length],
          status: 'Online',
          statusClass: 'text-emerald-500'
        }));
      },
      error: (err) => {
        this.loadingMembers = false;
        console.error('Error fetching members:', err);
      }
    });
  }

  loadGroupMessages(groupId: number): void {
    this.loadingMessages = true;
    this.chatService.getGroupMessages(groupId).subscribe({
      next: (messages: TinNhanDto[]) => {
        this.loadingMessages = false;
        const defaultAvatar = 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=80&auto=format&fit=crop&q=80';

        this.chatMessages = messages.map(msg => ({
          id: msg.maTinNhan,
          senderName: msg.tenNguoiGui || 'Thành viên',
          senderAvatar: msg.avatarNguoiGui || defaultAvatar,
          time: new Date(msg.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
          content: msg.noiDung,
          isMe: msg.isMine
        }));
      },
      error: (err) => {
        this.loadingMessages = false;
        console.error('Error fetching chat messages:', err);
      }
    });
  }

  appendRealtimeMessage(msg: TinNhanDto): void {
    const defaultAvatar = 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=80&auto=format&fit=crop&q=80';
    if (!this.chatMessages.some(m => m.id === msg.maTinNhan)) {
      this.chatMessages.push({
        id: msg.maTinNhan,
        senderName: msg.tenNguoiGui || 'Thành viên',
        senderAvatar: msg.avatarNguoiGui || defaultAvatar,
        time: new Date(msg.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        content: msg.noiDung,
        isMe: msg.isMine
      });
    }
  }

  sendMessage(): void {
    if (!this.newMessageText.trim() || this.activeGroupId <= 0) return;

    const content = this.newMessageText.trim();
    this.newMessageText = '';

    // 1. Send via SignalR
    this.chatSignalRService.sendMessage(this.activeGroupId, content);

    // 2. Also send via REST API for persistence guarantee
    this.chatService.sendMessage(this.activeGroupId, {
      maNhom: this.activeGroupId,
      noiDung: content,
      loaiTinNhan: 0
    }).subscribe({
      next: (msgDto: TinNhanDto) => {
        this.appendRealtimeMessage(msgDto);
      },
      error: (err) => {
        console.error('Error sending message via API:', err);
      }
    });
  }

  promptCreateGroup(): void {
    const name = prompt('Nhập tên nhóm học tập mới:');
    if (!name || !name.trim()) return;

    const desc = prompt('Nhập mô tả nhóm (không bắt buộc):') || '';
    const request: CreateStudyGroupRequest = {
      tenNhom: name.trim(),
      moTa: desc.trim(),
      soLuongToiDa: 20
    };

    this.groupService.createGroup(request).subscribe({
      next: (newGroup: NhomHocTapDto) => {
        alert(`Tạo nhóm "${newGroup.tenNhom}" thành công! Mã tham gia: ${newGroup.maThamGia}`);
        this.loadMyGroups();
      },
      error: (err) => {
        console.error('Error creating group:', err);
        alert(err?.error?.message || 'Tạo nhóm thất bại.');
      }
    });
  }

  promptJoinGroup(): void {
    const code = prompt('Nhập mã tham gia nhóm (ví dụ: ABC12345):');
    if (!code || !code.trim()) return;

    this.groupService.joinGroup(code.trim()).subscribe({
      next: (group: NhomHocTapDto) => {
        alert(`Đã tham gia nhóm "${group.tenNhom}" thành công!`);
        this.loadMyGroups();
      },
      error: (err) => {
        console.error('Error joining group:', err);
        alert(err?.error?.message || 'Mã tham gia nhóm không chính xác hoặc đã tham gia.');
      }
    });
  }

  leaveCurrentGroup(): void {
    if (this.activeGroupId <= 0) return;
    if (confirm(`Bạn có chắc chắn muốn rời nhóm "${this.activeGroup.name}" không?`)) {
      this.groupService.leaveGroup(this.activeGroupId).subscribe({
        next: () => {
          alert('Đã rời nhóm thành công.');
          this.loadMyGroups();
        },
        error: (err) => {
          console.error('Error leaving group:', err);
          alert(err?.error?.message || 'Không thể rời nhóm.');
        }
      });
    }
  }

  toggleLeftSidebar(): void {
    this.isLeftSidebarCollapsed = !this.isLeftSidebarCollapsed;
  }

  toggleRightSidebar(): void {
    this.isRightSidebarCollapsed = !this.isRightSidebarCollapsed;
  }

  toggleGroupDropdown(): void {
    this.showGroupDropdown = !this.showGroupDropdown;
  }
}
