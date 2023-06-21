using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string? userName, string? password);
        Task<bool> CheckAsync(string token);
        Task<bool> LogoutAsync(string token);
    }

    public class AuthService : IAuthService
    {
        public Task<bool> LoginAsync(string? userName, string? password)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CheckAsync(string token)
        {
            return Task.FromResult(true);
        }

        public Task<bool> LogoutAsync(string token)
        {
            return Task.FromResult(true);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthRequest request)
        {
            Console.WriteLine($"==> AuthDll: {Type.GetType("Microsoft.AspNetCore.Authentication.AuthenticationService")?.Assembly.Location}");

            _logger.LogDebug("Login: {user}/{password}", request.UserName, request.Password);
            if (await _authService.LoginAsync(request.UserName, request.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim("user", request.UserName!),
                    new Claim("role", "general")
                };

                await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));
                return Ok();
            }

            return Unauthorized();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("check")]
        public Task<IActionResult> CheckAuth()
        {
            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
