using FluentValidation;
using recycle.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Validators
{
    public class RegisterationRequestValidator : AbstractValidator<RegisterationRequest>
    {
        public RegisterationRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .Length(2, 50).WithMessage("First name must be between 3 and 50 characters.");
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .Length(2, 50).WithMessage("Last name must be between 3 and 50 characters.");
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name is required.")
                .Length(3, 50).WithMessage("User name must be between 3 and 50 characters.")
                .Matches("^[a-zA-Z0-9]*$").WithMessage("User name can only contain letters and numbers.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(BeAValidAge).WithMessage("You must be at least 18 years old.");

            RuleFor(x=>x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => role == "User" || role == "Driver")
                .WithMessage("Role must be either 'User', or 'Driver'.");
        }

        private bool BeAValidAge(DateTime time)
        {
            var age = DateTime.Today.Year - time.Year;
            if (time > DateTime.Today.AddYears(-age)) age--;
            return age >= 18;
        }
    }
}
