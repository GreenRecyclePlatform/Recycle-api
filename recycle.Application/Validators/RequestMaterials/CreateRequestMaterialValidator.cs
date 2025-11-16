using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using recycle.Application.DTOs.RequestMaterials;
using System.Threading.Tasks;

namespace recycle.Application.Validators.RequestMaterials
{
    
    public class CreateRequestMaterialValidator : AbstractValidator<RequestMaterialDto>
    {
        public CreateRequestMaterialValidator()
        {
            RuleFor(x => x.MaterialId)
                .NotEmpty().WithMessage("Material ID is required");

            RuleFor(x => x.EstimatedWeight)
                .GreaterThan(0).WithMessage("Estimated weight must be greater than zero")
                .LessThanOrEqualTo(10000).WithMessage("Estimated weight cannot exceed 10,000 kg");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
