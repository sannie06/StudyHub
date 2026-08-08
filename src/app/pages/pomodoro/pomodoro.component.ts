import { Component, OnInit, OnDestroy, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PomodoroService, PomodoroSessionDto } from '../../services/pomodoro.service';
import { TaskService, TaskDto } from '../../services/task.service';
import { SubjectService, SubjectDto, SubjectTag } from '../../services/subject.service';

export interface PomodoroSession {
  id: number;
  type: 'pomodoro' | 'short_break' | 'long_break';
  name: string;
  duration: string;
  timeRange: string;
  icon: string;
  iconColor: string;
}

@Component({
  selector: 'app-pomodoro',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './pomodoro.component.html',
  styles: [`
    .timer-ring {
      transition: stroke-dashoffset 0.5s linear;
    }
  `]
})
export class PomodoroComponent implements OnInit, OnDestroy {

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const el = this.elRef.nativeElement;
    const subjectWrapper = el.querySelector('#dropdown-subject-wrapper');
    const priorityWrapper = el.querySelector('#dropdown-priority-wrapper');
    const statusWrapper = el.querySelector('#dropdown-status-wrapper');

    if (this.openSubjectDropdown && subjectWrapper && !subjectWrapper.contains(event.target as Node)) {
      this.openSubjectDropdown = false;
    }
    if (this.openPriorityDropdown && priorityWrapper && !priorityWrapper.contains(event.target as Node)) {
      this.openPriorityDropdown = false;
    }
    if (this.openStatusDropdown && statusWrapper && !statusWrapper.contains(event.target as Node)) {
      this.openStatusDropdown = false;
    }
  }
  activeMode: 'pomodoro' | 'short_break' | 'long_break' = 'pomodoro';

  // Timer State
  totalSeconds: number = 25 * 60; // 25 minutes default
  timeLeft: number = 25 * 60;
  isRunning: boolean = false;
  timerInterval: any = null;

  // Session API Tracking State
  activeSessionId: number | null = null;
  pauseCount: number = 0;
  pauseStartTime: number | null = null;
  totalPauseSeconds: number = 0;

  // Toggle Switches State
  toggleMuteNotifications: boolean = true;
  toggleBlockWebsites: boolean = true;
  toggleBackgroundSound: boolean = false;
  toggleFullscreen: boolean = false;

  // Web Audio & Notification / Toast State
  private audioCtx: AudioContext | null = null;
  private noiseNode: AudioNode | null = null;
  private gainNode: GainNode | null = null;
  toastMessage: string | null = null;
  private toastTimeout: any = null;

  @HostListener('document:fullscreenchange', ['$event'])
  onFullscreenChange() {
    this.toggleFullscreen = !!document.fullscreenElement;
  }

  // Tasks Integration
  tasks: TaskDto[] = [];
  focusedTask: TaskDto | null = null;
  selectedOtherTaskId: number = 0;

  // Return all unfinished tasks excluding the currently focused task
  get otherTasks(): TaskDto[] {
    const unfinished = (this.tasks || []).filter(t => t.trangThai !== 3 && t.tiLeHoanThanh !== 100);
    if (!this.focusedTask) return unfinished;
    return unfinished.filter(t => t.maCongViec !== this.focusedTask?.maCongViec);
  }

  // Return maximum 5 tasks for main screen display
  get displayOtherTasks(): TaskDto[] {
    return this.otherTasks.slice(0, 5);
  }

  // All Tasks Modal
  showAllTasksModal: boolean = false;
  allTasksPage: number = 1;
  allTasksPageSize: number = 6;
  allTasksTotal: number = 0;
  allTasksList: TaskDto[] = [];
  isLoadingAllTasks: boolean = false;

  // Edit Task Modal & Custom Subject Dropdown State
  showEditTaskModal: boolean = false;
  isSavingTask: boolean = false;
  subjects: SubjectTag[] = [];

  openSubjectDropdown: boolean = false;
  openStatusDropdown: boolean = false;
  openPriorityDropdown: boolean = false;

  // New Tag inline form
  showAddTagInput: boolean = false;
  newTagName: string = '';
  newTagColor: string = '#6366F1';
  presetTagColors: string[] = ['#F97316', '#14B8A6', '#3B82F6', '#8B5CF6', '#EF4444', '#10B981', '#6366F1'];

  editingTaskForm: {
    maCongViec?: number;
    tieuDe: string;
    moTa: string;
    maMonHoc?: number | null;
    doUuTien: number;
    trangThai: number;
    hanHoanThanh: string;
    tiLeHoanThanh: number;
  } = {
    tieuDe: '',
    moTa: '',
    maMonHoc: null,
    doUuTien: 1,
    trangThai: 0,
    hanHoanThanh: '',
    tiLeHoanThanh: 0
  };

  // Completed sessions history
  sessionHistory: PomodoroSession[] = [];

  constructor(
    private pomodoroService: PomodoroService,
    private taskService: TaskService,
    private subjectService: SubjectService,
    private elRef: ElementRef
  ) {}

  ngOnInit() {
    this.setMode('pomodoro');
    this.loadTasks();
    this.loadSubjects();
    this.checkActiveSession();
    this.loadSessionHistoryFromStorage();
  }

  loadSessionHistoryFromStorage() {
    const saved = localStorage.getItem('studyhub_pomodoro_session_history');
    if (saved) {
      try {
        this.sessionHistory = JSON.parse(saved);
      } catch (e) {
        this.sessionHistory = [];
      }
    } else {
      this.sessionHistory = [];
    }
  }

  saveSessionHistoryToStorage() {
    localStorage.setItem('studyhub_pomodoro_session_history', JSON.stringify(this.sessionHistory));
  }

  deleteHistoryItem(id: number, event?: Event) {
    if (event) event.stopPropagation();
    this.sessionHistory = this.sessionHistory.filter(h => h.id !== id);
    this.saveSessionHistoryToStorage();
  }

  getCompletedPomoCount(): number {
    return this.sessionHistory.filter(s => s.type === 'pomodoro').length;
  }

  ngOnDestroy() {
    this.stopTimer();
    this.stopBackgroundSound();
  }

  showToast(msg: string) {
    this.toastMessage = msg;
    if (this.toastTimeout) clearTimeout(this.toastTimeout);
    this.toastTimeout = setTimeout(() => {
      this.toastMessage = null;
    }, 3200);
  }

  onToggleMuteNotifications() {
    this.toggleMuteNotifications = !this.toggleMuteNotifications;
    if (this.toggleMuteNotifications) {
      if ('Notification' in window && Notification.permission !== 'granted') {
        Notification.requestPermission();
      }
      this.showToast('🔇 Đã bật chế độ tắt thông báo khi tập trung');
    } else {
      this.showToast('🔔 Đã khôi phục nhận thông báo bình thường');
    }
  }

  onToggleBlockWebsites() {
    this.toggleBlockWebsites = !this.toggleBlockWebsites;
    if (this.toggleBlockWebsites) {
      this.showToast('🛡️ Đã kích hoạt chặn các trang web phân tâm (Facebook, YouTube, TikTok...)');
    } else {
      this.showToast('🔓 Đã tắt chế độ chặn trang web');
    }
  }

  private pianoAudio: HTMLAudioElement | null = null;
  private pianoInterval: any = null;

  onToggleBackgroundSound() {
    this.toggleBackgroundSound = !this.toggleBackgroundSound;
    if (this.toggleBackgroundSound) {
      this.playBackgroundSound();
      this.showToast('🎹 Đang phát nhạc Piano thư giãn tập trung');
    } else {
      this.stopBackgroundSound();
      this.showToast('🔇 Đã tắt nhạc Piano nền');
    }
  }

  private pianoAudioSources: string[] = [
    'https://actions.google.com/sounds/v1/science_fiction/ambient_piano.ogg',
    'https://raw.githubusercontent.com/rafaelreis-hotmart/Audio-Samples/master/piano.mp3'
  ];
  private currentAudioIndex = 0;

  private playBackgroundSound() {
    try {
      if (!this.pianoAudio) {
        this.pianoAudio = new Audio();
        this.pianoAudio.crossOrigin = 'anonymous';
        this.pianoAudio.loop = true;
        this.pianoAudio.volume = 0.22; // Low, gentle, non-distracting volume
      }

      this.pianoAudio.src = this.pianoAudioSources[this.currentAudioIndex];
      
      const playPromise = this.pianoAudio.play();
      if (playPromise !== undefined) {
        playPromise.catch(err => {
          console.warn('Primary audio stream failed, trying next track:', err);
          this.currentAudioIndex = (this.currentAudioIndex + 1) % this.pianoAudioSources.length;
          if (this.currentAudioIndex !== 0) {
            this.playBackgroundSound();
          } else {
            this.playWebAudioPianoSynth();
          }
        });
      }
    } catch (e) {
      this.playWebAudioPianoSynth();
    }
  }

  private playWebAudioPianoSynth() {
    try {
      if (!this.audioCtx) {
        const AudioContextClass = window.AudioContext || (window as any).webkitAudioContext;
        this.audioCtx = new AudioContextClass();
      }
      if (this.audioCtx.state === 'suspended') {
        this.audioCtx.resume();
      }

      // Slow, ultra-soft gentle ambient piano chords (Cmaj7 -> Fmaj7 -> Am7 -> G6)
      const chordSequences = [
        { chord: [261.63, 329.63, 392.00, 493.88] }, // Cmaj7
        { chord: [174.61, 261.63, 329.63, 440.00] }, // Fmaj7
        { chord: [220.00, 261.63, 329.63, 392.00] }, // Am7
        { chord: [196.00, 246.94, 293.66, 392.00] }  // G6
      ];
      let seqIndex = 0;

      const playPianoNote = (freq: number, delay: number, vol: number = 0.025, duration: number = 4.5) => {
        if (!this.audioCtx || !this.toggleBackgroundSound) return;
        const osc = this.audioCtx.createOscillator();
        const gain = this.audioCtx.createGain();

        // Ultra smooth pure sine wave note for peaceful ambient vibe
        osc.type = 'sine';
        osc.frequency.setValueAtTime(freq, this.audioCtx.currentTime + delay);

        const now = this.audioCtx.currentTime + delay;
        gain.gain.setValueAtTime(0, now);
        gain.gain.linearRampToValueAtTime(vol, now + 0.3); // Very slow, gentle attack
        gain.gain.exponentialRampToValueAtTime(0.00001, now + duration); // Long, smooth fade-out

        osc.connect(gain);
        gain.connect(this.audioCtx.destination);

        osc.start(now);
        osc.stop(now + duration + 0.2);
      };

      const triggerPattern = () => {
        if (!this.toggleBackgroundSound) return;
        const current = chordSequences[seqIndex];
        
        // Play soft ambient arpeggio notes with wide spacing
        current.chord.forEach((noteFreq, i) => {
          playPianoNote(noteFreq, i * 0.6, 0.025, 5.0);
        });

        seqIndex = (seqIndex + 1) % chordSequences.length;
      };

      triggerPattern();
      this.pianoInterval = setInterval(() => triggerPattern(), 6000); // Slow tempo (6 seconds per chord)

    } catch (e) {
      console.error('Web Audio Ambient Piano error:', e);
    }
  }

  private stopBackgroundSound() {
    if (this.pianoAudio) {
      try {
        this.pianoAudio.pause();
        this.pianoAudio.currentTime = 0;
      } catch (e) {}
    }
    if (this.pianoInterval) {
      clearInterval(this.pianoInterval);
      this.pianoInterval = null;
    }
  }

  onToggleFullscreen() {
    this.toggleFullscreen = !this.toggleFullscreen;
    if (this.toggleFullscreen) {
      if (document.documentElement.requestFullscreen) {
        document.documentElement.requestFullscreen().catch(() => {
          this.toggleFullscreen = false;
        });
      }
      this.showToast('🖥️ Đã mở chế độ toàn màn hình');
    } else {
      if (document.fullscreenElement && document.exitFullscreen) {
        document.exitFullscreen().catch(() => {});
      }
      this.showToast('🔲 Đã thoát chế độ toàn màn hình');
    }
  }

  loadTasks() {
    this.taskService.getTasks({ pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.items && res.items.length > 0) {
          this.tasks = res.items;
          const unfinished = this.tasks.filter(t => t.trangThai !== 3 && t.tiLeHoanThanh !== 100);
          if (unfinished.length > 0) {
            this.focusedTask = unfinished[0];
            this.selectedOtherTaskId = unfinished.length > 1 ? unfinished[1].maCongViec : unfinished[0].maCongViec;
          } else {
            this.focusedTask = this.tasks[0];
          }
        }
      },
      error: (err) => console.error('Error loading tasks for Pomodoro:', err)
    });
  }

  selectOtherTask(task: TaskDto) {
    // Đổi focused task — không thêm/bớt mảng, chỉ đổi reference
    this.focusedTask = task;
    this.selectedOtherTaskId = task.maCongViec;
  }

  openAllTasksModal() {
    this.allTasksPage = 1;
    this.showAllTasksModal = true;
    this.loadAllTasksPage();
  }

  closeAllTasksModal() {
    this.showAllTasksModal = false;
  }

  loadAllTasksPage() {
    this.isLoadingAllTasks = true;
    this.taskService.getTasks({ pageSize: this.allTasksPageSize, pageNumber: this.allTasksPage }).subscribe({
      next: (res) => {
        // Filter out completed tasks (trangThai === 3 or tiLeHoanThanh === 100)
        this.allTasksList = (res.items || []).filter(
          t => t.trangThai !== 3 && t.tiLeHoanThanh !== 100
        );
        this.allTasksTotal = res.totalCount || res.items?.length || 0;
        this.isLoadingAllTasks = false;
      },
      error: () => { this.isLoadingAllTasks = false; }
    });
  }

  get allTasksTotalPages(): number {
    return Math.max(1, Math.ceil(this.allTasksTotal / this.allTasksPageSize));
  }

  get allTasksPages(): number[] {
    return Array.from({ length: this.allTasksTotalPages }, (_, i) => i + 1);
  }

  goToAllTasksPage(page: number) {
    if (page < 1 || page > this.allTasksTotalPages) return;
    this.allTasksPage = page;
    this.loadAllTasksPage();
  }

  selectFocusTask(task: TaskDto) {
    this.focusedTask = task;
    this.selectedOtherTaskId = task.maCongViec;
    // Also update in tasks array if present, otherwise prepend
    const idx = this.tasks.findIndex(t => t.maCongViec === task.maCongViec);
    if (idx === -1) {
      this.tasks = [task, ...this.tasks];
    }
    this.closeAllTasksModal();
  }

  checkActiveSession() {
    this.pomodoroService.getActiveSession().subscribe({
      next: (session: PomodoroSessionDto) => {
        if (session) {
          this.activeSessionId = session.maSession;
          this.totalPauseSeconds = session.tongThoiGianTamDung || 0;
          this.pauseCount = session.soLanTamDung || 0;

          // Map loaiSession byte to mode
          if (session.loaiSession === 1) this.activeMode = 'short_break';
          else if (session.loaiSession === 2) this.activeMode = 'long_break';
          else this.activeMode = 'pomodoro';

          this.totalSeconds = session.thoiLuong * 60;

          // Calculate remaining time
          const startMs = new Date(session.thoiGianBatDau).getTime();
          const elapsedSec = Math.floor((Date.now() - startMs) / 1000) - this.totalPauseSeconds;
          const remainingSec = this.totalSeconds - elapsedSec;

          this.timeLeft = remainingSec > 0 ? remainingSec : 0;

          if (session.trangThai === 2) { // Running
            this.startTimer();
          }
        }
      },
      error: (err) => {
        // 404 means no active session, which is normal
        if (err.status !== 404) {
          console.error('Error checking active Pomodoro session:', err);
        }
      }
    });
  }

  setMode(mode: 'pomodoro' | 'short_break' | 'long_break') {
    this.activeMode = mode;
    this.stopTimer();
    if (mode === 'pomodoro') {
      this.totalSeconds = 25 * 60;
    } else if (mode === 'short_break') {
      this.totalSeconds = 5 * 60;
    } else if (mode === 'long_break') {
      this.totalSeconds = 15 * 60;
    }
    this.timeLeft = this.totalSeconds;
  }

  togglePlay() {
    if (this.isRunning) {
      this.pauseTimer();
    } else {
      this.startTimer();
    }
  }

  startTimer() {
    if (this.isRunning) return;

    // Accumulate pause time if resuming
    if (this.pauseStartTime) {
      const pauseDuration = Math.floor((Date.now() - this.pauseStartTime) / 1000);
      this.totalPauseSeconds += pauseDuration;
      this.pauseStartTime = null;
    }

    // Call API to start session if not started yet
    if (!this.activeSessionId) {
      const loaiSessionCode = this.activeMode === 'short_break' ? 1 : this.activeMode === 'long_break' ? 2 : 0;
      const minutesDuration = Math.round(this.totalSeconds / 60);

      this.pomodoroService.startSession({
        loaiSession: loaiSessionCode,
        thoiLuong: minutesDuration,
        tieuDe: this.focusedTask ? this.focusedTask.tieuDe : `Phiên ${this.activeMode}`,
        maCongViec: this.focusedTask ? this.focusedTask.maCongViec : undefined
      }).subscribe({
        next: (session: PomodoroSessionDto) => {
          this.activeSessionId = session.maSession;
        },
        error: (err) => console.error('Error starting Pomodoro session API:', err)
      });
    }

    this.isRunning = true;
    this.timerInterval = setInterval(() => {
      if (this.timeLeft > 0) {
        this.timeLeft--;
      } else {
        this.onTimerComplete();
      }
    }, 1000);
  }

  pauseTimer() {
    this.isRunning = false;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }

    this.pauseCount++;
    this.pauseStartTime = Date.now();

    if (this.activeSessionId) {
      this.pomodoroService.pauseSession(this.activeSessionId, {
        tongThoiGianTamDung: this.totalPauseSeconds
      }).subscribe({
        error: (err) => console.error('Error pausing Pomodoro session API:', err)
      });
    }
  }

  stopTimer() {
    this.isRunning = false;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  resetTimer() {
    if (this.activeSessionId) {
      this.pomodoroService.cancelSession(this.activeSessionId).subscribe({
        next: () => {
          this.activeSessionId = null;
        },
        error: (err) => console.error('Error canceling Pomodoro session API:', err)
      });
    }

    this.stopTimer();
    this.activeSessionId = null;
    this.pauseCount = 0;
    this.pauseStartTime = null;
    this.totalPauseSeconds = 0;
    this.timeLeft = this.totalSeconds;
  }

  onTimerComplete() {
    this.stopTimer();

    const now = new Date();
    const startTime = new Date(now.getTime() - (this.totalSeconds * 1000));
    const startStr = `${startTime.getHours().toString().padStart(2, '0')}:${startTime.getMinutes().toString().padStart(2, '0')}`;
    const endStr = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;

    if (this.activeSessionId) {
      this.pomodoroService.finishSession(this.activeSessionId, {
        tongThoiGianTamDung: this.totalPauseSeconds,
        soLanTamDung: this.pauseCount
      }).subscribe({
        next: () => {
          this.addHistoryItem(startStr, endStr);
          this.activeSessionId = null;
          this.pauseCount = 0;
          this.totalPauseSeconds = 0;
        },
        error: (err) => console.error('Error finishing Pomodoro session API:', err)
      });
    } else {
      this.addHistoryItem(startStr, endStr);
    }

    alert('🎉 Phiên tập trung đã hoàn thành! Hãy nghỉ giải lao nhé.');
  }

  private addHistoryItem(startStr: string, endStr: string) {
    const isPomo = this.activeMode === 'pomodoro';
    const isShort = this.activeMode === 'short_break';
    const minutes = Math.round(this.totalSeconds / 60);

    const pomoCount = this.sessionHistory.filter(s => s.type === 'pomodoro').length + 1;

    const newHistory: PomodoroSession = {
      id: Date.now(),
      type: this.activeMode,
      name: isPomo ? `Pomodoro ${pomoCount}` : (isShort ? 'Short Break' : 'Long Break'),
      duration: `${minutes} phút`,
      timeRange: `${startStr} - ${endStr}`,
      icon: isPomo ? '🍅' : (isShort ? '☕' : '🌴'),
      iconColor: isPomo ? 'text-red-500' : 'text-blue-500'
    };

    this.sessionHistory.unshift(newHistory);
    this.saveSessionHistoryToStorage();
  }

  loadSubjects() {
    this.subjectService.getSubjectTags().subscribe({
      next: (tags) => this.subjects = tags,
      error: (err) => console.error('Error loading subject tags for Pomodoro Edit Modal:', err)
    });
  }

  get selectedSubjectTag(): SubjectTag | undefined {
    if (this.editingTaskForm.maMonHoc) {
      const found = this.subjects.find(s => s.id === Number(this.editingTaskForm.maMonHoc));
      if (found) return found;
    }
    if (this.focusedTask?.tenMonHoc) {
      return this.subjects.find(s => s.name.toLowerCase() === this.focusedTask?.tenMonHoc?.toLowerCase());
    }
    return undefined;
  }

  toggleSubjectDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.openSubjectDropdown = !this.openSubjectDropdown;
    this.openStatusDropdown = false;
    this.openPriorityDropdown = false;
  }

  toggleStatusDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.openStatusDropdown = !this.openStatusDropdown;
    this.openSubjectDropdown = false;
    this.openPriorityDropdown = false;
  }

  togglePriorityDropdown(event?: Event) {
    if (event) event.stopPropagation();
    this.openPriorityDropdown = !this.openPriorityDropdown;
    this.openSubjectDropdown = false;
    this.openStatusDropdown = false;
  }

  closeAllDropdowns() {
    this.openSubjectDropdown = false;
    this.openStatusDropdown = false;
    this.openPriorityDropdown = false;
    this.showAddTagInput = false;
  }

  selectSubject(tag: SubjectTag | null) {
    this.editingTaskForm.maMonHoc = tag ? tag.id : null;
    this.openSubjectDropdown = false;
  }

  selectStatus(val: number) {
    this.editingTaskForm.trangThai = val;
    this.openStatusDropdown = false;
    // Auto-sync tiến độ theo trạng thái
    if (val === 0) {
      this.editingTaskForm.tiLeHoanThanh = 0;    // Cần thực hiện → 0%
    } else if (val === 1) {
      this.editingTaskForm.tiLeHoanThanh = 50;   // Đang thực hiện → 50%
    } else if (val === 3) {
      this.editingTaskForm.tiLeHoanThanh = 100;  // Hoàn thành → 100%
    }
  }

  selectPriority(val: number) {
    this.editingTaskForm.doUuTien = val;
    this.openPriorityDropdown = false;
  }

  onProgressChange() {
    const p = Number(this.editingTaskForm.tiLeHoanThanh);
    if (p === 0) {
      this.editingTaskForm.trangThai = 0;   // Cần thực hiện
    } else if (p === 100) {
      this.editingTaskForm.trangThai = 3;   // Hoàn thành
    } else {
      this.editingTaskForm.trangThai = 1;   // Đang thực hiện
    }
  }

  createNewSubjectTag() {
    if (!this.newTagName.trim()) return;
    const createdTag = this.subjectService.addSubjectTag(this.newTagName.trim(), this.newTagColor);
    
    const existingIdx = this.subjects.findIndex(s => s.id === createdTag.id || s.name.toLowerCase() === createdTag.name.toLowerCase());
    if (existingIdx !== -1) {
      this.subjects[existingIdx] = createdTag;
    } else {
      this.subjects.push(createdTag);
    }

    this.editingTaskForm.maMonHoc = createdTag.id;
    this.newTagName = '';
    this.showAddTagInput = false;
    this.openSubjectDropdown = false;
  }

  openEditTaskModal(task?: TaskDto) {
    const targetTask = task || this.focusedTask;
    if (!targetTask) return;

    this.closeAllDropdowns();

    let formattedDate = '';
    if (targetTask.hanHoanThanh) {
      const d = new Date(targetTask.hanHoanThanh);
      if (!isNaN(d.getTime())) {
        formattedDate = d.toISOString().split('T')[0];
      }
    }

    let validStatus = targetTask.trangThai ?? 0;
    if (validStatus === 2) {
      validStatus = 1;
    }

    this.editingTaskForm = {
      maCongViec: targetTask.maCongViec,
      tieuDe: targetTask.tieuDe || '',
      moTa: targetTask.moTa || '',
      maMonHoc: targetTask.maMonHoc || null,
      doUuTien: targetTask.doUuTien ?? 1,
      trangThai: validStatus,
      hanHoanThanh: formattedDate,
      tiLeHoanThanh: targetTask.tiLeHoanThanh ?? 0
    };

    this.showEditTaskModal = true;
  }

  closeEditTaskModal() {
    this.closeAllDropdowns();
    this.showEditTaskModal = false;
  }

  saveEditedTask() {
    if (!this.editingTaskForm.maCongViec) return;
    if (!this.editingTaskForm.tieuDe.trim()) {
      alert('Vui lòng nhập tiêu đề công việc!');
      return;
    }

    this.isSavingTask = true;
    const selectedSubject = this.selectedSubjectTag;

    const payload: Partial<TaskDto> = {
      tieuDe: this.editingTaskForm.tieuDe.trim(),
      moTa: this.editingTaskForm.moTa ? this.editingTaskForm.moTa.trim() : '',
      maMonHoc: this.editingTaskForm.maMonHoc ? Number(this.editingTaskForm.maMonHoc) : undefined,
      tenMonHoc: selectedSubject ? selectedSubject.name : this.focusedTask?.tenMonHoc,
      doUuTien: Number(this.editingTaskForm.doUuTien),
      trangThai: Number(this.editingTaskForm.trangThai),
      hanHoanThanh: this.editingTaskForm.hanHoanThanh ? new Date(this.editingTaskForm.hanHoanThanh).toISOString() : undefined,
      tiLeHoanThanh: this.editingTaskForm.trangThai === 3 ? 100 : Number(this.editingTaskForm.tiLeHoanThanh)
    };

    this.taskService.updateTask(this.editingTaskForm.maCongViec, payload).subscribe({
      next: (updatedTask: TaskDto) => {
        this.isSavingTask = false;
        this.closeEditTaskModal();

        const updatedSubjectName = selectedSubject ? selectedSubject.name : (updatedTask.tenMonHoc || this.focusedTask?.tenMonHoc);

        // Update focusedTask in memory
        if (this.focusedTask && this.focusedTask.maCongViec === this.editingTaskForm.maCongViec) {
          this.focusedTask = {
            ...this.focusedTask,
            ...updatedTask,
            tieuDe: payload.tieuDe!,
            moTa: payload.moTa,
            maMonHoc: payload.maMonHoc,
            tenMonHoc: updatedSubjectName,
            doUuTien: payload.doUuTien!,
            trangThai: payload.trangThai!,
            hanHoanThanh: payload.hanHoanThanh,
            tiLeHoanThanh: payload.tiLeHoanThanh!
          };
        }

        // Update task list item if present
        const idx = this.tasks.findIndex(t => t.maCongViec === this.editingTaskForm.maCongViec);
        if (idx !== -1 && this.focusedTask) {
          this.tasks[idx] = { ...this.tasks[idx], ...this.focusedTask };
        }
      },
      error: (err) => {
        this.isSavingTask = false;
        console.error('Error updating task:', err);
        alert('Lỗi khi cập nhật công việc. Vui lòng thử lại!');
      }
    });
  }


  get formattedTime(): string {
    const minutes = Math.floor(this.timeLeft / 60);
    const seconds = this.timeLeft % 60;
    const mStr = minutes < 10 ? '0' + minutes : '' + minutes;
    const sStr = seconds < 10 ? '0' + seconds : '' + seconds;
    return `${mStr}:${sStr}`;
  }

  // SVG Ring Progress offset calculation
  // Radius = 142, circumference = 2 * PI * 142 ≈ 892.21
  readonly circumference = 2 * Math.PI * 142;

  get strokeDashoffset(): number {
    const progress = (this.totalSeconds - this.timeLeft) / this.totalSeconds;
    return this.circumference * (1 - progress);
  }

  // Dial ticks for clock marks (12 hours / 60 mins ticks)
  get clockTicks() {
    const ticks = [];
    for (let i = 0; i < 60; i++) {
      const angle = (i * 6) * (Math.PI / 180);
      const isMajor = i % 5 === 0;
      const innerR = isMajor ? 128 : 132;
      const outerR = 136;
      const x1 = 150 + innerR * Math.sin(angle);
      const y1 = 150 - innerR * Math.cos(angle);
      const x2 = 150 + outerR * Math.sin(angle);
      const y2 = 150 - outerR * Math.cos(angle);
      ticks.push({ x1, y1, x2, y2, isMajor });
    }
    return ticks;
  }
}
