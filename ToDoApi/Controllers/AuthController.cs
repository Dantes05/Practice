using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryApp.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] UserForRegistrationDto userForRegistration,
            CancellationToken cancellationToken)
        {
            await _authService.RegisterAsync(userForRegistration, cancellationToken);
            return Ok();
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthResponseDto>> Authenticate(
            [FromBody] UserForAuthenticationDto userForAuthentication,
            CancellationToken cancellationToken)
        {
            var result = await _authService.AuthenticateAsync(userForAuthentication, cancellationToken);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(User, cancellationToken);
            return Ok();
        }
    }
}