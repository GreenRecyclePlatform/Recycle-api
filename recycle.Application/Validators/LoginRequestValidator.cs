using FluentValidation;
using recycle.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            //RuleFor(x => x.Email)
            //    .NotEmpty().WithMessage("Email is required.")
            //    .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.UserName))
            .WithMessage("You must provide either an email or a username.");

            // Email rules (only if provided)
            When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("Invalid email format.");
            });

            // Username rules (only if provided)
            When(x => !string.IsNullOrWhiteSpace(x.UserName), () =>
            {
                RuleFor(x => x.UserName)
                    .MinimumLength(3).WithMessage("Username must be at least 3 characters.");
            });
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
