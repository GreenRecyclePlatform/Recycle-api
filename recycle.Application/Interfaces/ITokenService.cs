using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GetAccessToken(ApplicationUser user, string jwtTokenId);
        Task<string> CreateNewRefreshToken(Guid userId, string jwtTokenId);
        Task<Tokens> RefreshAccessToken(string refreshToken);
        Task<bool> RevokeRefreshToken(string refreshToken);
        Task<string> GeneratePasswordResetToken(Guid userId);
        Task<Guid?> ValidatePasswordResetTokenAsync(string token);
    }
}
