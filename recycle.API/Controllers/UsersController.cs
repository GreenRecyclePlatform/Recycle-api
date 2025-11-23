using Azure;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System.Net;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IValidator<RegisterationRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public UsersController(IUserRepository userRepository, IAuthService authService, ITokenService tokenService, IValidator<RegisterationRequest> registerValidator,
            IValidator<LoginRequest> loginValidator)

        {
            _userRepo = userRepository;
            _authService = authService;
            _tokenService = tokenService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }


        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.InitiatePasswordResetAsync(email);
                if (!result)
                {
                    return BadRequest("Error while processing forgot password request");
                }
                return Ok("Password reset link has been sent to your email, if the email exists");
            }
            else
            {
                return BadRequest("Invalid request");
            }

        }
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] string token,string newPassword)
        {
           
            var result = await _authService.ResetPasswordAsync(token,newPassword);
            if (!result)
            {
                return BadRequest("Error while resetting password. The token may be invalid or expired.");
            }
            return Ok("Password has been reset successfully");
        }

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] RegisterationRequest model,
            [FromServices] IValidator<RegisterationRequest> validator)
        {
            var validationResult = await _registerValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            bool ifUserNameUnique = await _userRepo.IsUniqueAsync(model.Email, model.UserName);
            if (!ifUserNameUnique)
            {

              
                
                return BadRequest("Username or Email already exists");
            }

            var user = await _authService.Register(model);
            if (user == null)
            {

                return BadRequest("Error while registering");
            }

            return Ok($"Welcome {model.FirstName}");
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest model,
            [FromServices] IValidator<LoginRequest> validator)
        {
            var validationResult = await _loginValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var Tokens = await _authService.Login(model);

            if (Tokens == null || string.IsNullOrEmpty(Tokens.AccessToken))
            {

                return BadRequest("Username or Email or password is incorrect");
            }

            SetRefreshTokenInCookie(Tokens.RefreshToken);

            return Ok(Tokens.AccessToken);
        }

        [HttpPost("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNewTokenFromRefreshToken()
        {
            if (ModelState.IsValid)
            {
                var refreshToken = Request.Cookies["refreshToken"];
               
                var tokens = await _tokenService.RefreshAccessToken(refreshToken);

                if (tokens == null || string.IsNullOrEmpty(tokens.AccessToken))
                {
                    return BadRequest("Invalid token");
                }
                SetRefreshTokenInCookie(tokens.RefreshToken);
                return Ok(tokens.AccessToken);
            }
            else
            {
                return BadRequest("Invalid request");
            }

        }

        [HttpPost("Logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeRefreshToken()
        {
            if (ModelState.IsValid)
            {
                var refreshToken = Request.Cookies["refreshToken"];
               
                var result = await _tokenService.RevokeRefreshToken(refreshToken);
                if (!result)
                {
                    return BadRequest("Invalid token");
                }
                DeleteRefreshTokenCookie();

                return Ok("Logout successful");
            }
            else
            {
                return BadRequest("Invalid request");
            }
        }

        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
        private void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1) // Set expiration to past date
            };
            Response.Cookies.Append("refreshToken", "", cookieOptions);
        }

    }
}
