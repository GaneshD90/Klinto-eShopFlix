using eShopFlix.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.Admin.Controllers
{

    [CustomAuthorize(Roles = "Admin")]
    [Area("Admin")]
    public class BaseController : Controller
    {
       
    }
}
