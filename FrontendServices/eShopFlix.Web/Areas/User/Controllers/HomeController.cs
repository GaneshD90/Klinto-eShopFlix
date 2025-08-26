using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.User.Controllers
{    
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
