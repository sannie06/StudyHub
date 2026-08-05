using System.Collections.Generic;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.StudyGroup;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IStudyGroupService
    {
        Task<IEnumerable<NhomHocTapDto>> GetMyGroupsAsync(int userId, string? search);
        Task<NhomHocTapDto> GetGroupByIdAsync(int id, int userId);
        Task<NhomHocTapDto> CreateGroupAsync(int userId, CreateStudyGroupRequest request);
        Task<NhomHocTapDto> UpdateGroupAsync(int id, int userId, UpdateStudyGroupRequest request);
        Task DeleteGroupAsync(int id, int userId);
        Task<NhomHocTapDto> JoinGroupViaCodeAsync(int userId, JoinGroupRequest request);
        Task LeaveGroupAsync(int id, int userId);
        Task<IEnumerable<ThanhVienNhomDto>> GetGroupMembersAsync(int id, int userId);
        Task<ThanhVienNhomDto> AddMemberAsync(int id, int memberUserId, int currentUserId);
        Task RemoveMemberAsync(int id, int memberUserId, int currentUserId);

        // Group Tasks
        Task<List<GroupTaskDto>> GetGroupTasksAsync(int groupId, int userId);
        Task<GroupTaskDto> CreateGroupTaskAsync(int groupId, int userId, CreateGroupTaskRequest request);
        Task<GroupTaskDto> UpdateGroupTaskStatusAsync(int groupId, int taskId, int userId, byte status);
        Task DeleteGroupTaskAsync(int groupId, int taskId, int userId);

        // Group Meetings
        Task<List<LichHopNhomDto>> GetGroupMeetingsAsync(int groupId, int userId);
        Task<LichHopNhomDto> CreateGroupMeetingAsync(int groupId, int userId, CreateLichHopRequest request);
        Task<LichHopNhomDto> UpdateGroupMeetingAsync(int groupId, int meetingId, int userId, CreateLichHopRequest request);
        Task DeleteGroupMeetingAsync(int groupId, int meetingId, int userId);

        // Group Folders & Documents
        Task<List<ThuMucTaiLieuDto>> GetGroupFoldersAsync(int groupId, int userId);
        Task<ThuMucTaiLieuDto> CreateGroupFolderAsync(int groupId, int userId, CreateThuMucRequest request);
        Task<ThuMucTaiLieuDto> UpdateGroupFolderAsync(int groupId, int folderId, int userId, UpdateThuMucRequest request);
        Task DeleteGroupFolderAsync(int groupId, int folderId, int userId);
        Task<List<GroupDocumentDto>> GetGroupDocumentsAsync(int groupId, int? folderId, int userId);
        Task<GroupDocumentDto> CreateGroupDocumentAsync(int groupId, int userId, CreateGroupDocumentRequest request);
        Task DeleteGroupDocumentAsync(int groupId, int documentId, int userId);
        Task<(byte[] fileBytes, string contentType, string fileName)> DownloadGroupDocumentAsync(int groupId, int documentId, int userId);
    }
}
