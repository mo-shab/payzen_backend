using Microsoft.AspNetCore.Mvc;

namespace payzen_backend.Controllers
{
    public class GendersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
