using System.Net;

namespace StudyHub.Application.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) 
            : base(message, HttpStatusCode.NotFound)
        {
        }

        public NotFoundException(string name, object key) 
            : base($"Entity \"{name}\" ({key}) was not found.", HttpStatusCode.NotFound)
        {
        }
    }
}
