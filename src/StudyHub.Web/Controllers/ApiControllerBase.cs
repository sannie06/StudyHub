using Microsoft.AspNetCore.Mvc;

namespace StudyHub.Web.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
