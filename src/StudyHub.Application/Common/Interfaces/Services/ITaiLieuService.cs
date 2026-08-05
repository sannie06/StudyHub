using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.TaiLieu;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface ITaiLieuService
    {
        Task<IEnumerable<TaiLieuDto>> GetDocumentsAsync(int userId, int maNhom, string? search);
        Task<TaiLieuDto> GetDocumentByIdAsync(int id, int userId);
        Task<TaiLieuDto> UploadDocumentAsync(int userId, UploadDocumentRequest request);
        Task<TaiLieuDto> UpdateDocumentAsync(int id, int userId, UpdateDocumentRequest request);
        Task DeleteDocumentAsync(int id, int userId);
        Task<(Stream fileStream, string contentType, string fileName)> DownloadDocumentAsync(int id, int userId);
        Task<IEnumerable<DocumentGroupDto>> GetMyGroupsAsync(int userId);
    }
}
