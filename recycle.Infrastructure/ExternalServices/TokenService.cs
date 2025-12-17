using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.ExternalServices
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private string secretKey;
        private readonly AppDbContext _context;

        public TokenService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            secretKey = configuration["ApiSettings:Secret"];
            _context = context;
        }

        public async Task<string> GeneratePasswordResetToken(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return null;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            return encodedToken;
        }

        public async Task<Guid?> ValidatePasswordResetTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
            
            if(resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }
            return resetToken.UserId;
        }


        public async Task<string> GetAccessToken(ApplicationUser user, string jwtTokenId)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            var roles = await _userManager.GetRolesAsync(applicationUser);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, applicationUser.UserName),
                    new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                    new Claim(ClaimTypes.Email, applicationUser.Email),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    //new Claim(JwtRegisteredClaimNames.Aud, "..."),
                }),
                Issuer = "recycle.API",
                //Audience = "recycle.web",
                Expires = DateTime.Now.AddMinutes(90),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateNewRefreshToken(Guid userId, string jwtTokenId)
        {
            await MarkAllTokenInChainAsInvalid(userId, jwtTokenId);
            RefreshToken refreshToken = new RefreshToken()
            {
                UserId = userId,
                JwtTokenId = jwtTokenId,
                IsValid = true,
                ExpiresAt = DateTime.Now.AddDays(10),
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        public async Task<Tokens> RefreshAccessToken(string refreshToken)
        {
            var existingRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Refresh_Token == refreshToken);
            if (existingRefreshToken == null || !existingRefreshToken.IsValid)
            {
                return new Tokens();
            }

           

            if (!existingRefreshToken.IsValid)
            {
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            }

            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new Tokens();
            }

            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            await MarkTokenAsInvalid(existingRefreshToken);

            var appuser = await _userManager.FindByIdAsync(existingRefreshToken.UserId.ToString());
            if (appuser == null)
            {
                return new Tokens();
            }
            var user = new ApplicationUser()
            {
                Id = appuser.Id,
                UserName = appuser.UserName,
                Email = appuser.Email,
            };

            var newAccessToken = await GetAccessToken(user, existingRefreshToken.JwtTokenId);
            return new Tokens()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }


        private async Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
             _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        private async Task MarkAllTokenInChainAsInvalid(Guid userId, string jwtTokenId)
        {
            await _context.RefreshTokens.Where(u => u.UserId == userId
              && u.JwtTokenId == jwtTokenId)
                  .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));

        }


        public async Task<bool> RevokeRefreshToken(string refreshToken)
        {
            var existingRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Refresh_Token == refreshToken);
            if (existingRefreshToken == null)
            {
                return false;
            }
          

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            return true;
        }
    }
}
