import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  public hubConnection: signalR.HubConnection | null = null;
  public pomodoroState$ = new Subject<any>();
  public notification$ = new Subject<any>();

  constructor(private authService: AuthService) {}

  public startConnection() {
    // If connection already exists, don't recreate it
    if (this.hubConnection) {
      return;
    }

    const token = this.authService.token;
    if (!token) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:5186/hubs/studyhub?access_token=${token}`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connection established.'))
      .catch(err => console.error('Error starting SignalR connection:', err));

    this.hubConnection.on('ReceivePomodoroState', (state) => {
      this.pomodoroState$.next(state);
    });

    this.hubConnection.on('ReceiveNotification', (notification) => {
      this.notification$.next(notification);
    });
  }

  public syncPomodoro(state: any) {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('SyncPomodoroState', state)
        .catch(err => console.error('Error invoking SyncPomodoroState:', err));
    }
  }

  public stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => {
          console.log('SignalR connection stopped.');
          this.hubConnection = null;
        })
        .catch(err => console.error('Error stopping SignalR connection:', err));
    }
  }
}
