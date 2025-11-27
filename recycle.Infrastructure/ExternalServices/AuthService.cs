using Microsoft.AspNetCore.Identity;
using recycle.Application.DTOs;
using recycle.Application.Interfaces;
using recycle.Application.Services;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.ExternalServices
{
    public class AuthService: IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly AddressService _addressService;
        public AuthService(IUserRepository userRepository, ITokenService tokenService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IEmailService emailService,AddressService addressService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _addressService = addressService;
        }

        public async Task<bool> InitiatePasswordResetAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return false;
                
            }
            var token = await _tokenService.GeneratePasswordResetToken(user.Id);
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };
            await _userRepository.SavePasswordResetTokenAsync(resetToken);

            //Here you would typically send the token to the user's email.
            var resetLink = $"http://localhost:4200/reset-password?token={token}";

            await _emailService.SendEmail(user.Email, "Password Reset", $"Click the link to reset your password: {resetLink}");

            //var resetLink = $"{token}";

            //await _emailService.SendEmail(user.Email, "Password Reset",resetLink);


            return true;
           
        }

        public async Task<bool> ResetPasswordAsync(string token,string newPassword)
        {
           var userId = await _tokenService.ValidatePasswordResetTokenAsync(token);
            if (userId == null)
            {
                return false;
            }


            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return false;
            }

            var hashedNewPassword = _userManager.PasswordHasher.HashPassword(user, newPassword);

            var result = await _userRepository.UpdatePasswordAsync(user.Id, hashedNewPassword);
            if (result)
            {             

                await _userRepository.MarkTokenAsUsedAsync(user.Id);
            }
            return result;
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            var resetToken = await _userRepository.GetPasswordResetTokenAsync(token);
            if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }



        public async Task<Tokens> Login(LoginRequest request)
        {
            ApplicationUser user = null;
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _userRepository.GetByUserNameAsync(request.UserName);
                
            }
            else
            {
                 user = await _userRepository.GetByEmailAsync(request.Email);
               
            }
            if (user == null)
            {
                return new Tokens();
            }
            var isValid = await _userRepository.CheckPasswordAsync(user, request.Password);
            if (!isValid)
            {
                return new Tokens();
            }
            var jwtTokenId = Guid.NewGuid().ToString();
            var accessToken = await _tokenService.GetAccessToken(user, jwtTokenId);
            var refreshToken = await _tokenService.CreateNewRefreshToken(user.Id, jwtTokenId);

            Tokens tokens = new Tokens
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return tokens;

        }

        public async Task<ApplicationUser> Register(RegisterationRequest request)
        {
            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth,
                UserName = request.UserName
            };
            var result = await _userRepository.IsUniqueAsync(request.Email, request.UserName);

            if (result)
            {
                user = await _userRepository.AddUser(user, request.Password);
                if (user == null)
                {
                    throw new Exception("User creation failed");
                }
                else
                {
                    if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
                        await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));
                        await _roleManager.CreateAsync(new IdentityRole<Guid>("Driver"));

                    }

                    var applicationUser = await _userManager.FindByEmailAsync(user.Email);
                    if (applicationUser != null)
                    {
                        await _userManager.AddToRoleAsync(applicationUser, request.Role);
                    }

                    await _addressService.CreateAddress(request.Address, user.Id);

                    return user;
                }

            }
            return null;
        }
    }
}

