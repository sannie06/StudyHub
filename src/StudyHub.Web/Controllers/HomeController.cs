using Microsoft.AspNetCore.Mvc;
using System;

namespace StudyHub.Web.Controllers
{
    public class HomeController : ApiControllerBase
    {
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("~/swagger");
        }
    }
}
