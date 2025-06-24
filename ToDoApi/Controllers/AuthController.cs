using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ToDoApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] UserForRegistrationDto userForRegistration,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Register request for user: {Email}", userForRegistration.Email);

            try
            {
                await _authService.RegisterAsync(userForRegistration, cancellationToken);
                _logger.LogInformation("User registered successfully: {Email}", userForRegistration.Email);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Email}", userForRegistration.Email);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthResponseDto>> Authenticate(
            [FromBody] UserForAuthenticationDto userForAuthentication,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Authentication request for user: {Email}", userForAuthentication.Email);

            try
            {
                var result = await _authService.AuthenticateAsync(userForAuthentication, cancellationToken);
                _logger.LogInformation("User authenticated: {Email}", userForAuthentication.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user: {Email}", userForAuthentication.Email);
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Refresh token request");

            try
            {
                var result = await _authService.RefreshTokenAsync(request, cancellationToken);
                _logger.LogDebug("Token refreshed successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token failed");
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Logout request for user: {UserId}", userId);

            try
            {
                await _authService.LogoutAsync(User, cancellationToken);
                _logger.LogInformation("User logged out: {UserId}", userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed for user: {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }
    }
}