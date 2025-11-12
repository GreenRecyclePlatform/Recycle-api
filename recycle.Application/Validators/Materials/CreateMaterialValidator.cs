using FluentValidation;
using recycle.Application.DTOs.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Validators.Materials
{
    public class CreateMaterialValidator : AbstractValidator<CreateMaterialDto>
    {
        public CreateMaterialValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Material name is required")
                .MaximumLength(100).WithMessage("Material name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-]+$").WithMessage("Material name can only contain letters, numbers, spaces, and hyphens");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Unit)
                .MaximumLength(20).WithMessage("Unit cannot exceed 20 characters");

            RuleFor(x => x.PricePerKg)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .LessThanOrEqualTo(10000).WithMessage("Price cannot exceed 10,000 per kg");
        }
    }

    public class UpdateMaterialValidator : AbstractValidator<UpdateMaterialDto>
    {
        public UpdateMaterialValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Material name is required")
                .MaximumLength(100).WithMessage("Material name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-]+$").WithMessage("Material name can only contain letters, numbers, spaces, and hyphens");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Unit)
                .MaximumLength(20).WithMessage("Unit cannot exceed 20 characters");

            RuleFor(x => x.PricePerKg)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .LessThanOrEqualTo(10000).WithMessage("Price cannot exceed 10,000 per kg");
        }
    }
}
