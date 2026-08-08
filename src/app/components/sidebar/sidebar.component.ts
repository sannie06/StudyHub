import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styles: [`
    .sidebar-scroll::-webkit-scrollbar { width: 4px; }
    .sidebar-scroll::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 9999px; }
  `]
})
export class SidebarComponent implements OnInit, OnDestroy {
  currentUrl: string = '';
  isTaskMenuOpen: boolean = false;
  unreadCount: number = 0;
  private notifySub?: Subscription;

  constructor(
    private router: Router,
    public authService: AuthService,
    private notificationService: NotificationService
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  ngOnInit() {
    this.currentUrl = this.router.url;
    this.checkTaskMenuState(this.currentUrl);

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.currentUrl = event.urlAfterRedirects || event.url;
      this.checkTaskMenuState(this.currentUrl);
    });

    this.notifySub = this.notificationService.unreadCount$.subscribe(count => {
      this.unreadCount = count;
    });
  }

  ngOnDestroy() {
    this.notifySub?.unsubscribe();
  }

  checkTaskMenuState(url: string) {
    if (url.includes('/tasks')) {
      this.isTaskMenuOpen = true;
    } else {
      this.isTaskMenuOpen = false;
    }
  }

  toggleTaskMenu() {
    this.isTaskMenuOpen = !this.isTaskMenuOpen;
  }

  onTaskParentClick() {
    this.toggleTaskMenu();
    if (!this.currentUrl.includes('/tasks')) {
      this.router.navigate(['/tasks']);
    }
  }

  closeTaskMenu() {
    this.isTaskMenuOpen = false;
  }

  isRouteActive(route: string): boolean {
    if (!this.currentUrl) return false;
    return this.currentUrl.startsWith(route);
  }

  isTasksViewActive(view: string): boolean {
    if (!this.currentUrl.includes('/tasks')) return false;
    if (view === 'kanban') {
      return this.currentUrl.includes('view=kanban');
    }
    return !this.currentUrl.includes('view=kanban') || this.currentUrl.includes('view=list');
  }

  logout() {
    this.authService.clearSession();
    window.location.href = '/login';
  }
}
