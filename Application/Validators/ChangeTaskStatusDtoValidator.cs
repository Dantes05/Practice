using FluentValidation;
using Application.DTOs;
using Domain.Enums;

namespace Application.Validators
{
    public class ChangeTaskStatusDtoValidator : AbstractValidator<ChangeTaskStatusDto>
    {
        public ChangeTaskStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid task status");
        }
    }
}