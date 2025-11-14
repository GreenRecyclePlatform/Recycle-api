using FluentValidation;
using recycle.Application.DTOs.RequestMaterials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Validators.RequestMaterials
{
    public class UpdateActualWeightValidator : AbstractValidator<UpdateActualWeightDto>
    {
        public UpdateActualWeightValidator()
        {
            RuleFor(x => x.ActualWeight)
                .GreaterThanOrEqualTo(0).WithMessage("Actual weight cannot be negative")
                .LessThanOrEqualTo(10000).WithMessage("Actual weight cannot exceed 10,000 kg");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
