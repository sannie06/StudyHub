using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Kanban;
using StudyHub.Application.DTOs.Task;
using StudyHub.Domain.Entities;
using StudyHub.Persistence;

namespace StudyHub.Infrastructure.Services
{
    public class KanbanService : IKanbanService
    {
        private readonly StudyHubDbContext _context;
        private readonly IGenericRepository<KanbanBoard> _boardRepository;
        private readonly IGenericRepository<KanbanCot> _columnRepository;
        private readonly IGenericRepository<KanbanThe> _cardRepository;

        public KanbanService(
            StudyHubDbContext context,
            IGenericRepository<KanbanBoard> boardRepository,
            IGenericRepository<KanbanCot> columnRepository,
            IGenericRepository<KanbanThe> cardRepository)
        {
            _context = context;
            _boardRepository = boardRepository;
            _columnRepository = columnRepository;
            _cardRepository = cardRepository;
        }

        public async Task<List<KanbanBoardDto>> GetBoardsAsync(int userId)
        {
            return await _boardRepository.GetQueryable()
                .Where(b => b.MaNguoiDung == userId)
                .Select(b => new KanbanBoardDto
                {
                    MaBoard = b.MaBoard,
                    TenBoard = b.TenBoard,
                    MoTa = b.MoTa,
                    MauSac = b.MauSac,
                    MacDinh = b.MacDinh
                })
                .ToListAsync();
        }

        public async Task<KanbanBoardDto> GetBoardDetailsAsync(int boardId, int userId)
        {
            var board = await _boardRepository.GetQueryable()
                .Where(b => b.MaBoard == boardId && b.MaNguoiDung == userId)
                .Select(b => new KanbanBoardDto
                {
                    MaBoard = b.MaBoard,
                    TenBoard = b.TenBoard,
                    MoTa = b.MoTa,
                    MauSac = b.MauSac,
                    MacDinh = b.MacDinh,
                    Columns = b.KanbanCot.OrderBy(c => c.ThuTu).Select(c => new KanbanColumnDto
                    {
                        MaCot = c.MaCot,
                        TenCot = c.TenCot,
                        MauSac = c.MauSac,
                        ThuTu = c.ThuTu,
                        GioiHanThe = c.GioiHanThe,
                        Cards = c.KanbanThe.OrderBy(card => card.ThuTu).Select(card => new KanbanCardDto
                        {
                            MaThe = card.MaThe,
                            MaCot = card.MaCot,
                            MaCongViec = card.MaCongViec,
                            ThuTu = card.ThuTu,
                            Task = new TaskDto
                            {
                                MaCongViec = card.CongViec.MaCongViec,
                                TieuDe = card.CongViec.TieuDe,
                                MoTa = card.CongViec.MoTa,
                                DoUuTien = card.CongViec.DoUuTien,
                                TrangThai = card.CongViec.TrangThai,
                                NgayBatDau = card.CongViec.NgayBatDau,
                                HanHoanThanh = card.CongViec.HanHoanThanh,
                                NgayHoanThanh = card.CongViec.NgayHoanThanh,
                                TiLeHoanThanh = card.CongViec.TiLeHoanThanh,
                                MauSac = card.CongViec.MauSac,
                                DanhDauQuanTrong = card.CongViec.DanhDauQuanTrong,
                                DanhDauYeuThich = card.CongViec.DanhDauYeuThich,
                                GhiChu = card.CongViec.GhiChu,
                                TenMonHoc = card.CongViec.MonHoc != null ? card.CongViec.MonHoc.TenMonHoc : null,
                                MaMon = card.CongViec.MonHoc != null ? card.CongViec.MonHoc.MaMon : null
                            }
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (board == null)
            {
                throw new NotFoundException("Bảng Kanban không tồn tại.");
            }

            return board;
        }

        public async Task MoveCardsAsync(int userId, MoveCardRequest request)
        {
            if (request.CardPositions == null || !request.CardPositions.Any())
            {
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cardIds = request.CardPositions.Select(cp => cp.MaThe).ToList();
                
                // Fetch the cards and verify that the associated tasks belong to the user
                var cards = await _context.Set<KanbanThe>()
                    .Include(c => c.CongViec)
                    .Where(c => cardIds.Contains(c.MaThe) && c.CongViec.MaNguoiDung == userId)
                    .ToListAsync();

                foreach (var position in request.CardPositions)
                {
                    var card = cards.FirstOrDefault(c => c.MaThe == position.MaThe);
                    if (card != null)
                    {
                        card.MaCot = position.MaCot;
                        card.ThuTu = position.ThuTu;
                        card.NgayCapNhat = DateTime.UtcNow;

                        // Synchronize task status if requested
                        if (position.NewTaskStatus.HasValue)
                        {
                            card.CongViec.TrangThai = position.NewTaskStatus.Value;
                            card.CongViec.NgayCapNhat = DateTime.UtcNow;

                            if (position.NewTaskStatus.Value == 3)
                            {
                                card.CongViec.NgayHoanThanh = DateTime.UtcNow;
                                card.CongViec.TiLeHoanThanh = 100;
                            }
                            else
                            {
                                card.CongViec.NgayHoanThanh = null;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Có lỗi xảy ra khi di chuyển các thẻ Kanban.");
            }
        }

        public async Task<KanbanColumnDto> CreateColumnAsync(int boardId, string name, string? color)
        {
            var board = await _boardRepository.GetByIdAsync(boardId);
            if (board == null)
            {
                throw new NotFoundException("Bảng Kanban không tồn tại.");
            }

            var columns = await _columnRepository.FindAsync(c => c.MaBoard == boardId);
            var maxOrder = columns.Any() ? columns.Max(c => c.ThuTu) : 0;

            var column = new KanbanCot
            {
                MaBoard = boardId,
                TenCot = name,
                MauSac = color,
                ThuTu = maxOrder + 1,
                NgayTao = DateTime.UtcNow
            };

            await _columnRepository.AddAsync(column);
            await _columnRepository.SaveAsync();

            return new KanbanColumnDto
            {
                MaCot = column.MaCot,
                TenCot = column.TenCot,
                MauSac = column.MauSac,
                ThuTu = column.ThuTu,
                GioiHanThe = column.GioiHanThe,
                Cards = new List<KanbanCardDto>()
            };
        }

        public async Task DeleteColumnAsync(int columnId)
        {
            var column = await _columnRepository.GetByIdAsync(columnId);
            if (column == null)
            {
                throw new NotFoundException("Cột Kanban không tồn tại.");
            }

            // Verify if the column has cards linked
            var cardsList = await _cardRepository.FindAsync(c => c.MaCot == columnId);
            if (cardsList.Any())
            {
                throw new BadRequestException("Không thể xóa cột đang chứa thẻ công việc.");
            }

            _columnRepository.Delete(column);
            await _columnRepository.SaveAsync();
        }
    }
}
