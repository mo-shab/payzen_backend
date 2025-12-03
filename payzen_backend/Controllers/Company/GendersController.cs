using Microsoft.AspNetCore.Mvc;

namespace payzen_backend.Controllers.Company
{
    public class GendersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
