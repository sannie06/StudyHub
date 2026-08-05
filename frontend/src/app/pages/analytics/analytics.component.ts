import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService, AnalyticsDto, HeatMapEntryDto } from '../../services/analytics.service';
import dayjs from 'dayjs';

interface HeatMapWeek {
  days: {
    date: string;
    dayNum: number;
    value: number;
    colorClass: string;
    formattedDate: string;
  }[];
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.component.html',
  styleUrls: []
})
export class AnalyticsComponent implements OnInit {
  analyticsData: AnalyticsDto | null = null;
  loading = true;
  error = '';

  // Heat map structures
  weeks: HeatMapWeek[] = [];
  monthsHeader: { name: string; colSpan: number }[] = [];

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit() {
    this.loadAnalytics();
  }

  loadAnalytics() {
    this.loading = true;
    this.error = '';

    this.analyticsService.getAnalytics().subscribe({
      next: (data) => {
        this.analyticsData = data;
        this.buildHeatMapGrid(data.heatMap);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Không thể tải dữ liệu thống kê học tập.';
        console.error(err);
      }
    });
  }

  buildHeatMapGrid(entries: HeatMapEntryDto[]) {
    // We have exactly 365 entries from one year ago to today.
    // Let's align them into weeks (Sunday to Saturday)
    const weeksArray: HeatMapWeek[] = [];
    let currentWeek: HeatMapWeek = { days: [] };

    // Align start of grid to Sunday if necessary
    const firstDate = dayjs(entries[0].date);
    const startOffset = firstDate.day(); // 0 is Sunday, 1 is Monday...

    // Fill offset days with empty squares
    for (let i = 0; i < startOffset; i++) {
      currentWeek.days.push({
        date: '',
        dayNum: -1,
        value: 0,
        colorClass: 'bg-transparent border-0',
        formattedDate: ''
      });
    }

    entries.forEach((entry) => {
      const dateObj = dayjs(entry.date);
      const val = entry.value;

      let colorClass = 'bg-slate-100 hover:bg-slate-200'; // 0 mins
      if (val > 0 && val <= 15) colorClass = 'bg-indigo-100 hover:bg-indigo-200';
      else if (val > 15 && val <= 30) colorClass = 'bg-indigo-300 hover:bg-indigo-400';
      else if (val > 30 && val <= 60) colorClass = 'bg-indigo-500 hover:bg-indigo-600';
      else if (val > 60) colorClass = 'bg-indigo-700 hover:bg-indigo-800';

      currentWeek.days.push({
        date: entry.date,
        dayNum: dateObj.date(),
        value: val,
        colorClass,
        formattedDate: dateObj.format('DD/MM/YYYY')
      });

      // If week is full (7 days), push it and create a new one
      if (currentWeek.days.length === 7) {
        weeksArray.push(currentWeek);
        currentWeek = { days: [] };
      }
    });

    // Push trailing week
    if (currentWeek.days.length > 0) {
      while (currentWeek.days.length < 7) {
        currentWeek.days.push({
          date: '',
          dayNum: -1,
          value: 0,
          colorClass: 'bg-transparent border-0',
          formattedDate: ''
        });
      }
      weeksArray.push(currentWeek);
    }

    this.weeks = weeksArray;

    // Build months header labels
    this.buildMonthsHeader(entries);
  }

  private buildMonthsHeader(entries: HeatMapEntryDto[]) {
    const header: { name: string; colSpan: number }[] = [];
    let lastMonth = '';
    let spanCount = 0;

    entries.forEach((entry, idx) => {
      const dateObj = dayjs(entry.date);
      const monthName = dateObj.format('MMM');

      // Every 7 items represents a column/week roughly
      if (idx % 7 === 0) {
        if (monthName !== lastMonth) {
          if (spanCount > 0) {
            header.push({ name: lastMonth, colSpan: spanCount });
          }
          lastMonth = monthName;
          spanCount = 1;
        } else {
          spanCount++;
        }
      }
    });

    if (spanCount > 0) {
      header.push({ name: lastMonth, colSpan: spanCount });
    }

    this.monthsHeader = header;
  }

  // Helper to find the maximum focus time in weekly activity for charting
  getMaxWeeklyFocus(): number {
    if (!this.analyticsData || this.analyticsData.weeklyActivity.length === 0) return 60;
    const maxVal = Math.max(...this.analyticsData.weeklyActivity.map(w => w.focusMinutes));
    return maxVal > 0 ? maxVal : 60;
  }
}
