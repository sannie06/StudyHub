import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ChatService, TinNhanDto, TypingNotificationDto } from '../../services/chat.service';
import { ChatSignalRService } from '../../services/chat-signalr.service';
import { GroupService, NhomHocTapDto, ThanhVienNhomDto } from '../../services/group.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  groups: NhomHocTapDto[] = [];
  selectedGroup: NhomHocTapDto | null = null;
  members: ThanhVienNhomDto[] = [];
  messages: TinNhanDto[] = [];

  loadingGroups = false;
  loadingMessages = false;
  error: string | null = null;

  newMessage = '';
  typingUsers: { [key: number]: string } = {};
  onlineUsers: { [key: number]: boolean } = {};
  
  private typingTimeout: any;
  private shouldScrollBottom = false;
  private subscriptions: Subscription[] = [];
  currentUserId: number | null = null;

  constructor(
    private chatService: ChatService,
    private chatSignalRService: ChatSignalRService,
    private groupService: GroupService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(u => {
      if (u) this.currentUserId = u.maNguoiDung;
    });

    this.chatSignalRService.startConnection();
    this.loadMyGroups();
    this.setupSignalRSubscriptions();
  }

  ngOnDestroy(): void {
    if (this.selectedGroup) {
      this.chatSignalRService.leaveGroupChat(this.selectedGroup.maNhom);
    }
    this.subscriptions.forEach(s => s.unsubscribe());
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollBottom) {
      this.scrollToBottom();
      this.shouldScrollBottom = false;
    }
  }

  private scrollToBottom(): void {
    try {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    } catch (err) {}
  }

  loadMyGroups(): void {
    this.loadingGroups = true;
    this.error = null;
    this.groupService.getMyGroups().subscribe({
      next: (list) => {
        this.groups = list;
        this.loadingGroups = false;

        this.route.paramMap.subscribe(params => {
          const groupIdParam = params.get('groupId');
          if (groupIdParam) {
            const g = list.find(x => x.maNhom === Number(groupIdParam));
            if (g) {
              this.selectGroup(g);
              return;
            }
          }
          if (list.length > 0) {
            this.selectGroup(list[0]);
          }
        });
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách nhóm:', err);
        this.error = 'Không thể tải danh sách nhóm học tập.';
        this.loadingGroups = false;
      }
    });
  }

  selectGroup(group: NhomHocTapDto): void {
    if (this.selectedGroup) {
      this.chatSignalRService.leaveGroupChat(this.selectedGroup.maNhom);
    }

    this.selectedGroup = group;
    this.typingUsers = {};
    this.messages = [];
    this.chatSignalRService.joinGroupChat(group.maNhom);

    this.loadGroupMembers(group.maNhom);
    this.loadMessages(group.maNhom);
  }

  loadGroupMembers(groupId: number): void {
    this.groupService.getMembers(groupId).subscribe({
      next: (members) => {
        this.members = members;
      },
      error: (err) => console.error('Lỗi tải thành viên nhóm:', err)
    });
  }

  loadMessages(groupId: number): void {
    this.loadingMessages = true;
    this.chatService.getGroupMessages(groupId).subscribe({
      next: (list) => {
        this.messages = list.map(m => ({
          ...m,
          isMine: m.maNguoiGui === this.currentUserId
        }));
        this.loadingMessages = false;
        this.shouldScrollBottom = true;
      },
      error: (err) => {
        console.error('Lỗi tải tin nhắn:', err);
        this.loadingMessages = false;
      }
    });
  }

  setupSignalRSubscriptions(): void {
    // Receive message
    this.subscriptions.push(
      this.chatSignalRService.message$.subscribe((msg) => {
        if (this.selectedGroup && msg.maNhom === this.selectedGroup.maNhom) {
          msg.isMine = msg.maNguoiGui === this.currentUserId;
          this.messages.push(msg);
          this.shouldScrollBottom = true;
        }
      })
    );

    // Typing notification
    this.subscriptions.push(
      this.chatSignalRService.typing$.subscribe((t: TypingNotificationDto) => {
        if (this.selectedGroup && t.maNhom === this.selectedGroup.maNhom && t.maNguoiDung !== this.currentUserId) {
          if (t.isTyping) {
            this.typingUsers[t.maNguoiDung] = t.tenNguoiDung;
          } else {
            delete this.typingUsers[t.maNguoiDung];
          }
        }
      })
    );

    // Online status
    this.subscriptions.push(
      this.chatSignalRService.onlineStatus$.subscribe((status) => {
        if (status) {
          this.onlineUsers[status.userId] = status.isOnline;
        }
      })
    );
  }

  onTyping(): void {
    if (!this.selectedGroup) return;
    this.chatSignalRService.sendTyping(this.selectedGroup.maNhom, true);

    clearTimeout(this.typingTimeout);
    this.typingTimeout = setTimeout(() => {
      if (this.selectedGroup) {
        this.chatSignalRService.sendTyping(this.selectedGroup.maNhom, false);
      }
    }, 2000);
  }

  sendMessage(): void {
    if (!this.selectedGroup || !this.newMessage.trim()) return;

    const content = this.newMessage.trim();
    this.newMessage = '';

    // Send via SignalR for immediate real-time broadcast
    this.chatSignalRService.sendMessage(this.selectedGroup.maNhom, content);
    this.chatSignalRService.sendTyping(this.selectedGroup.maNhom, false);
  }

  onDeleteMessage(msg: TinNhanDto): void {
    if (confirm('Bạn có chắc chắn muốn xóa tin nhắn này không?')) {
      this.chatService.deleteMessage(msg.maTinNhan).subscribe({
        next: () => {
          this.messages = this.messages.filter(m => m.maTinNhan !== msg.maTinNhan);
        },
        error: (err) => {
          console.error('Lỗi xóa tin nhắn:', err);
          alert('Không thể xóa tin nhắn.');
        }
      });
    }
  }

  getTypingText(): string {
    const names = Object.values(this.typingUsers);
    if (names.length === 0) return '';
    if (names.length === 1) return `${names[0]} đang nhập...`;
    return `${names.join(', ')} đang nhập...`;
  }

  formatTime(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
