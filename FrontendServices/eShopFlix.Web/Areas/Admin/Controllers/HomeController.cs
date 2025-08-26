using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
