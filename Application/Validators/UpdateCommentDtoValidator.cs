using FluentValidation;
using Application.DTOs;

namespace Application.Validators
{
    public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentDtoValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Text is required")
                .MaximumLength(1000).WithMessage("Text cannot exceed 1000 characters");
        }
    }
}