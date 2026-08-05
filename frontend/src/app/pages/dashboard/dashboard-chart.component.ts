import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardStatistics, WeeklyProgress } from '../../services/dashboard.service';

@Component({
  selector: 'app-dashboard-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-chart.component.html',
  styleUrls: ['./dashboard-chart.component.scss']
})
export class DashboardChartComponent {
  @Input() statistics: DashboardStatistics | null = null;
  @Input() weeklyProgress: WeeklyProgress[] = [];

  getMaxValue(): number {
    if (!this.weeklyProgress || this.weeklyProgress.length === 0) return 10;
    const maxVal = Math.max(...this.weeklyProgress.map(w => Math.max(w.completedCount, w.createdCount)));
    return maxVal > 0 ? maxVal : 10;
  }

  getBarHeightPercent(count: number): number {
    const max = this.getMaxValue();
    return Math.min(100, Math.max(10, Math.round((count / max) * 100)));
  }

  getCompletionRate(): number {
    if (!this.statistics || this.statistics.tongSoCongViec === 0) return 0;
    return Math.round((this.statistics.congViecHoanThanh / this.statistics.tongSoCongViec) * 100);
  }
}
