using Application.DTOs;
using Application.Extensions;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthService(IMapper mapper, UserManager<User> userManager,
            SignInManager<User> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
            _mapper = mapper;
        }

        public async Task<RegistrationResponseDto> RegisterAsync(
            UserForRegistrationDto userForRegistration,
            CancellationToken cancellationToken = default)
        {
            if (userForRegistration == null)
            {
                throw new Extensions.ValidationException("User data is required");
            }

            var user = _mapper.Map<User>(userForRegistration);
            var result = await _userManager.CreateAsync(user, userForRegistration.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new Extensions.ValidationException($"Registration failed: {string.Join(", ", errors)}");
            }

            await _userManager.AddToRoleAsync(user, "User");
            return new RegistrationResponseDto { IsSuccessfulRegistration = true };
        }

        public async Task<AuthResponseDto> AuthenticateAsync(
            UserForAuthenticationDto userForAuthentication,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(userForAuthentication.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, userForAuthentication.Password))
            {
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

            return authResponse;
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(
            RefreshTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException("Invalid or expired refresh token");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = GenerateJwtToken(user, roles);

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
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                throw new UnauthorizedException("User not found");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userManager.UpdateAsync(user);
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