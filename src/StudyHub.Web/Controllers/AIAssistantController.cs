using Microsoft.AspNetCore.Mvc;

namespace StudyHub.Web.Controllers
{
    public class AIAssistantController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trợ lý học tập thông minh AI";
            return View();
        }
    }
}
