using Application.DTOs;
using Application.Extensions;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IMapper mapper,
            UserManager<User> userManager,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _config = config;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task RegisterAsync(
            UserForRegistrationDto userForRegistration,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Register request for user: {Email}", userForRegistration.Email);

            if (userForRegistration == null)
            {
                _logger.LogError("User data is required");
                throw new ValidationException("User data is required");
            }

            var user = _mapper.Map<User>(userForRegistration);
            var result = await _userManager.CreateAsync(user, userForRegistration.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogError("Registration failed for user {Email}: {Errors}", userForRegistration.Email, string.Join(", ", errors));
                throw new ValidationException($"Registration failed: {string.Join(", ", errors)}");
            }

            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("User registered successfully: {Email}", userForRegistration.Email);
        }

        public async Task<AuthResponseDto> AuthenticateAsync(
            UserForAuthenticationDto userForAuthentication,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Authentication request for user: {Email}", userForAuthentication.Email);

            var user = await _userManager.FindByEmailAsync(userForAuthentication.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, userForAuthentication.Password))
            {
                _logger.LogError("Authentication failed for user: {Email}", userForAuthentication.Email);
                throw new UnauthorizedException("Invalid email or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            var authResponse = _mapper.Map<AuthResponseDto>(user);
            authResponse.Token = token;
            authResponse.RefreshToken = refreshToken;
            authResponse.IsAuthSuccessful = true;

            _logger.LogInformation("User authenticated: {Email}", userForAuthentication.Email);
            return authResponse;
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(
            RefreshTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Refresh token request");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogError("Refresh token failed - invalid or expired token");
                throw new UnauthorizedException("Invalid or expired refresh token");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = GenerateJwtToken(user, roles);

            _logger.LogDebug("Token refreshed successfully");
            return new AuthResponseDto
            {
                IsAuthSuccessful = true,
                Token = newAccessToken,
                RefreshToken = request.RefreshToken
            };
        }

        public async Task LogoutAsync(
            ClaimsPrincipal userPrincipal,
            CancellationToken cancellationToken = default)
        {
            var userId = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Logout request for user: {UserId}", userId);

            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                _logger.LogError("Logout failed for user: {UserId} - user not found", userId);
                throw new UnauthorizedException("User not found");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userManager.UpdateAsync(user);
            _logger.LogInformation("User logged out: {UserId}", userId);
        }

        private string GenerateJwtToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:securityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:expiryInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:validIssuer"],
                audience: _config["JwtSettings:validAudience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}