using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartInventorySystem.Application.Services
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration, IMapper mapper, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("User with this email already exists");
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Role = UserRole.User
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Registration failed", errors);
                }

                await _userManager.AddToRoleAsync(user, UserRole.User.ToString());

                var token = await GenerateJwtToken(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Role = user.Role.ToString()
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return ApiResponse<AuthResponseDto>.ErrorResponse("Registration failed");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
                }

                var token = await GenerateJwtToken(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Role = user.Role.ToString()
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return ApiResponse<AuthResponseDto>.ErrorResponse("Login failed");
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim("Role", user.Role.ToString())
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryInHours"] ?? "24"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
