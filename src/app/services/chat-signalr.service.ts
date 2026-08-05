import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { TinNhanDto, TypingNotificationDto } from './chat.service';

@Injectable({
  providedIn: 'root'
})
export class ChatSignalRService {
  private hubConnection?: signalR.HubConnection;
  
  public message$ = new Subject<TinNhanDto>();
  public typing$ = new Subject<TypingNotificationDto>();
  public onlineStatus$ = new BehaviorSubject<{ userId: number; isOnline: boolean } | null>(null);

  async startConnection(): Promise<void> {
    if (this.hubConnection && (this.hubConnection.state === signalR.HubConnectionState.Connected || this.hubConnection.state === signalR.HubConnectionState.Connecting)) {
      return;
    }

    const token = localStorage.getItem('sh_token');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5186/hubs/chat', {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveMessage', (message: TinNhanDto) => {
      this.message$.next(message);
    });

    this.hubConnection.on('UserTyping', (typingInfo: TypingNotificationDto) => {
      this.typing$.next(typingInfo);
    });

    this.hubConnection.on('UserOnlineStatus', (userId: number, isOnline: boolean) => {
      this.onlineStatus$.next({ userId, isOnline });
    });

    try {
      await this.hubConnection.start();
      console.log('Chat SignalR connected successfully.');
    } catch (err) {
      console.error('Error starting Chat SignalR connection:', err);
    }
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = undefined;
    }
  }

  async joinGroupChat(groupId: number): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      await this.startConnection();
    }
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinGroupChat', groupId).catch(err => console.error('JoinGroupChat error:', err));
    }
  }

  async leaveGroupChat(groupId: number): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveGroupChat', groupId).catch(err => console.error('LeaveGroupChat error:', err));
    }
  }

  async sendMessage(groupId: number, content: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      await this.startConnection();
    }
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendMessage', groupId, content).catch(err => console.error('SendMessage error:', err));
    }
  }

  async sendTyping(groupId: number, isTyping: boolean): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendTyping', groupId, isTyping).catch(err => console.error('SendTyping error:', err));
    }
  }
}
