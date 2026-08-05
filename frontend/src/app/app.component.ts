import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { HeaderComponent } from './components/header/header.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, HeaderComponent],
  templateUrl: './app.component.html',
  styles: []
})
export class AppComponent implements OnInit {
  title = 'StudyHub';
  isAuthPage: boolean = false;

  constructor(private router: Router) {}

  ngOnInit() {
    this.checkAuthPage(this.router.url);
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.checkAuthPage(event.urlAfterRedirects || event.url);
    });
  }

  private checkAuthPage(url: string) {
    this.isAuthPage = url.includes('/login') || url.includes('/register');
  }
}
