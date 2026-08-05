using Microsoft.AspNetCore.Mvc;

namespace StudyHub.Web.Controllers
{
    public class ScheduleController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Thời khóa biểu & Lịch trình";
            return View();
        }
    }
}
