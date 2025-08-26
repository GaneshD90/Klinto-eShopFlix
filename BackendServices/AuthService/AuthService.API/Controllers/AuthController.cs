using AuthService.Application.DTO;
using AuthService.Application.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IUserAppService _userAppService;
        public AuthController(IUserAppService userAppService)
        {
            _userAppService = userAppService;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginDTO loginDTO)

        {

            UserDTO user = _userAppService.LoginUser(loginDTO);
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }
            return Ok(user);

        }

        [HttpPost]
        public IActionResult SignUp([FromBody] SignUpDTO signUpDTO, string role)
        {
            bool isSuccess = _userAppService.SignUpUser(signUpDTO, role);
            if (isSuccess)
            {
                return Ok("User registered successfully");
            }
            return BadRequest("User registration failed");
        }
    }
}
