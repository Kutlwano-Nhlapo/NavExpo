using Microsoft.AspNetCore.Mvc;

namespace NavExpo.Controllers
{
    public class AttendeesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
