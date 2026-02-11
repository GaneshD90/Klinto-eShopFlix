using eShopFlix.Web.Helpers;
using eShopFlix.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace eShopFlix.Web.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = "Admin")]
    [Area("Admin")]
    public class BaseController : Controller
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public UserModel? CurrentUser
        {
            get
            {
                if (User?.Identity?.IsAuthenticated == true)
                {
                    var claim = User.FindFirst(ClaimTypes.UserData);
                    if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
                    {
                        return JsonSerializer.Deserialize<UserModel>(claim.Value, SerializerOptions);
                    }
                }
                return null;
            }
        }

        protected static Guid CreateDeterministicGuid(long id)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(id).CopyTo(bytes, 0);
            bytes[8] = 0xE5;
            bytes[9] = 0x0F;
            return new Guid(bytes);
        }
    }
}
