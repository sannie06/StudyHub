using Microsoft.AspNetCore.Mvc;

namespace StudyHub.Web.Controllers
{
    public class StudyGroupController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Nhóm học tập cộng tác";
            return View();
        }
    }
}
