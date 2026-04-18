using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    }

}
