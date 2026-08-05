namespace StudyHub.Application.Common.Interfaces.Security
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Email { get; }
    }
}
