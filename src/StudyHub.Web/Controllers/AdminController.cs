using Microsoft.AspNetCore.Mvc;

namespace StudyHub.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Bảng quản trị hệ thống";
            return View();
        }
    }
}
