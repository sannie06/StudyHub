import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/student-dashboard.component').then(m => m.StudentDashboardComponent)
  },
  {
    path: 'phan-tich-hoc-tap',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/analytics-dashboard.component').then(m => m.AnalyticsDashboardComponent)
  },
  {
    path: 'tasks',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/tasks/tasks.component').then(m => m.TasksComponent)
  },
  {
    path: 'calendar/create',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/calendar/create-calendar-event.component').then(m => m.CreateCalendarEventComponent)
  },
  {
    path: 'calendar',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/calendar/calendar.component').then(m => m.CalendarComponent)
  },
  {
    path: 'pomodoro',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/pomodoro/pomodoro.component').then(m => m.PomodoroComponent)
  },
  {
    path: 'groups',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/groups/groups.component').then(m => m.GroupsComponent)
  },
  {
    path: 'ai',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/ai/ai-assistant.component').then(m => m.AiAssistantComponent)
  },
  {
    path: 'notifications',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/notifications/notifications.component').then(m => m.NotificationsComponent)
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/user/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/auth/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'confirm-email',
    loadComponent: () => import('./pages/auth/confirm-email.component').then(m => m.ConfirmEmailComponent)
  },
  {
    path: 'verify-otp',
    loadComponent: () => import('./pages/auth/verify-otp.component').then(m => m.VerifyOtpComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./pages/auth/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./pages/auth/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
