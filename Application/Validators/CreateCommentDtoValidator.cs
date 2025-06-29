using FluentValidation;
using Application.DTOs;

namespace Application.Validators
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Text is required")
                .MaximumLength(100).WithMessage("Text cannot exceed 100 characters");

            RuleFor(x => x.TaskaId)
                .NotEmpty().WithMessage("Task ID is required")
                .Must(BeValidGuid).WithMessage("Invalid Task ID format");
        }

        private bool BeValidGuid(string id)
        {
            return Guid.TryParse(id, out _);
        }
    }
}