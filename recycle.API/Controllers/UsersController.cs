using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs;
using recycle.Application.Interfaces;
using System.Net;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        //private readonly APIResponse _response;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IValidator<RegisterationRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public UsersController(IUserRepository userRepository, IAuthService authService, ITokenService tokenService, IValidator<RegisterationRequest> registerValidator,
            IValidator<LoginRequest> loginValidator)
        
        {
            _userRepo = userRepository;
            //_response = new APIResponse();
            _authService = authService;
            _tokenService = tokenService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
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
                
                return BadRequest("Username already exists");
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

            var jwtToken = await _authService.Login(model);

            if (jwtToken == null || string.IsNullOrEmpty(jwtToken))
            {
               
                return BadRequest("Username or Email or password is incorrect");
            }

           
            return Ok(new { Token = jwtToken } );
        }
    }
}
