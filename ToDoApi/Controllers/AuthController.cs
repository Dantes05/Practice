using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Filters;

namespace ToDoApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [ServiceFilter(typeof(ValidationFilter))]
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
            CancellationToken cancellationToken = default)
        {
            await _authService.RegisterAsync(userForRegistration, cancellationToken);
            return Ok();
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthResponseDto>> Authenticate(
            [FromBody] UserForAuthenticationDto userForAuthentication,
            CancellationToken cancellationToken = default)
        {
            var result = await _authService.AuthenticateAsync(userForAuthentication, cancellationToken);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _authService.RefreshTokenAsync(request, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
        {
            await _authService.LogoutAsync(User, cancellationToken);
            return Ok();
        }
    }
}