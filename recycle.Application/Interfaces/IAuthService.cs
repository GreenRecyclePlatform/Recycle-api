using recycle.Application.DTOs;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApplicationUser> Register(RegisterationRequest request);
        Task<Tokens> Login(LoginRequest request);
        Task<bool> InitiatePasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token,string newPassword);
        Task<bool> ValidateResetTokenAsync(string token);
    }
}
