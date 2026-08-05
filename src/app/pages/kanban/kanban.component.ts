import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { KanbanService, KanbanBoardDto, KanbanColumnDto, KanbanCardDto, CardPositionDto } from '../../services/kanban.service';
import { TaskService, TaskDto } from '../../services/task.service';

@Component({
  selector: 'app-kanban',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    DialogModule,
    DragDropModule
  ],
  templateUrl: './kanban.component.html',
  styles: []
})
export class KanbanComponent implements OnInit {
  boards: any[] = [];
  selectedBoardId: number | null = null;
  boardDetails: KanbanBoardDto | null = null;
  
  loading = true;
  error = '';

  // New Column dialog
  displayColDialog = false;
  newColName = '';
  newColColor = '#6366F1';
  colSubmitLoading = false;

  constructor(
    private kanbanService: KanbanService,
    private taskService: TaskService
  ) {}

  ngOnInit() {
    this.loadBoards();
  }

  loadBoards() {
    this.loading = true;
    this.error = '';
    
    this.kanbanService.getBoards().subscribe({
      next: (data) => {
        this.boards = data;
        if (data.length > 0) {
          // Select default board or first board
          const defaultBoard = data.find(b => b.macDinh) || data[0];
          this.selectedBoardId = defaultBoard.maBoard || null;
          if (this.selectedBoardId) {
            this.loadBoardDetails(this.selectedBoardId);
          }
        } else {
          this.loading = false;
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Không thể tải danh sách bảng Kanban. Vui lòng tải lại.';
        console.error(err);
      }
    });
  }

  loadBoardDetails(boardId: number) {
    this.loading = true;
    this.error = '';
    
    this.kanbanService.getBoardDetails(boardId).subscribe({
      next: (details) => {
        this.boardDetails = details;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Không thể tải chi tiết bảng Kanban.';
        console.error(err);
      }
    });
  }

  onBoardChange() {
    if (this.selectedBoardId) {
      this.loadBoardDetails(this.selectedBoardId);
    }
  }

  onCardDropped(event: CdkDragDrop<KanbanCardDto[]>, targetColumn: KanbanColumnDto) {
    if (!this.boardDetails) return;

    if (event.previousContainer === event.container) {
      // Reordering in the same column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Moving to a different column
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }

    const cardToMove = event.container.data[event.currentIndex];
    
    // Map target column name to appropriate task status byte
    let newStatus = cardToMove.task.trangThai;
    const colName = targetColumn.tenCot.toLowerCase();
    if (colName.includes('todo') || colName.includes('cần làm') || colName.includes('chưa bắt đầu')) {
      newStatus = 0;
    } else if (colName.includes('progress') || colName.includes('đang làm') || colName.includes('thực hiện')) {
      newStatus = 1;
    } else if (colName.includes('review') || colName.includes('tạm dừng')) {
      newStatus = 2;
    } else if (colName.includes('done') || colName.includes('hoàn thành')) {
      newStatus = 3;
    }

    cardToMove.maCot = targetColumn.maCot;
    cardToMove.task.trangThai = newStatus;

    // Recalculate orders inside all columns
    const positions: CardPositionDto[] = [];
    this.boardDetails.columns.forEach(col => {
      col.cards.forEach((c, index) => {
        c.thuTu = index + 1;
        positions.push({
          maThe: c.maThe,
          maCot: col.maCot,
          thuTu: c.thuTu,
          newTaskStatus: c.maThe === cardToMove.maThe ? newStatus : undefined
        });
      });
    });

    // Send transactional bulk update to backend
    this.kanbanService.moveCards({ cardPositions: positions }).subscribe({
      next: () => {
        if (this.selectedBoardId) {
          this.loadBoardDetails(this.selectedBoardId);
        }
      },
      error: (err) => {
        alert('Không thể lưu thay đổi vị trí thẻ Kanban.');
        console.error(err);
        if (this.selectedBoardId) {
          this.loadBoardDetails(this.selectedBoardId); // Rollback changes locally
        }
      }
    });
  }

  showAddColDialog() {
    this.newColName = '';
    this.newColColor = '#6366F1';
    this.displayColDialog = true;
  }

  onCreateColumn() {
    if (!this.newColName.trim() || !this.selectedBoardId) {
      return;
    }

    this.colSubmitLoading = true;
    this.kanbanService.createColumn(this.selectedBoardId, this.newColName, this.newColColor).subscribe({
      next: () => {
        this.colSubmitLoading = false;
        this.displayColDialog = false;
        if (this.selectedBoardId) {
          this.loadBoardDetails(this.selectedBoardId);
        }
      },
      error: (err) => {
        this.colSubmitLoading = false;
        alert(err.error?.title || 'Lỗi khi tạo cột.');
      }
    });
  }

  onDeleteColumn(columnId: number) {
    if (!confirm('Bạn có chắc chắn muốn xóa cột này không?')) {
      return;
    }

    this.kanbanService.deleteColumn(columnId).subscribe({
      next: () => {
        if (this.selectedBoardId) {
          this.loadBoardDetails(this.selectedBoardId);
        }
      },
      error: (err) => {
        // Display backend validation error if cards are still present
        alert(err.error?.title || 'Không thể xóa cột.');
      }
    });
  }

  getPriorityClass(priority: number): string {
    switch (priority) {
      case 0: return 'bg-slate-100 text-slate-500';
      case 1: return 'bg-blue-50 text-blue-600';
      case 2: return 'bg-amber-50 text-amber-600';
      case 3: return 'bg-red-50 text-red-600';
      default: return '';
    }
  }

  getPriorityLabel(priority: number): string {
    switch (priority) {
      case 0: return 'Thấp';
      case 1: return 'Trung bình';
      case 2: return 'Cao';
      case 3: return 'Khẩn cấp';
      default: return '';
    }
  }
}
