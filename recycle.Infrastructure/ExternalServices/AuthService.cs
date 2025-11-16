using Microsoft.AspNetCore.Identity;
using recycle.Application.DTOs;
using recycle.Application.Interfaces;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        public AuthService(IUserRepository userRepository, ITokenService tokenService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> Login(LoginRequest request)
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
                return string.Empty;
            }
            var isValid = await _userRepository.CheckPasswordAsync(user, request.Password);
            if (!isValid)
            {
                return string.Empty;
            }
            var jwtTokenId = Guid.NewGuid().ToString();
            var accessToken = await _tokenService.GetAccessToken(user, jwtTokenId);

           return accessToken;

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
                    return user;
                }

            }
            return null;
        }
    }
}

