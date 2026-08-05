using Microsoft.AspNetCore.Mvc;

namespace StudyHub.Web.Controllers
{
    public class TaskController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý công việc & Kanban";
            return View();
        }

        public IActionResult List()
        {
            ViewData["Title"] = "Danh sách công việc";
            return View();
        }
    }
}
