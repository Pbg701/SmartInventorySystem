using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using SmartInventorySystem.Application.Interfaces;

namespace SmartInventorySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
