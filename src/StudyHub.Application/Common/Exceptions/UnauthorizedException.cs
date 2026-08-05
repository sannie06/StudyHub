using System.Net;

namespace StudyHub.Application.Common.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) 
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }
}
