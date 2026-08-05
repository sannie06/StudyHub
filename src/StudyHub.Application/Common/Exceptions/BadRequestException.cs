using System.Net;

namespace StudyHub.Application.Common.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) 
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
