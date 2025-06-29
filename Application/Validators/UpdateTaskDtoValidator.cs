using FluentValidation;
using Application.DTOs;
using Domain.Enums;

namespace Application.Validators
{
    public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
    {
        public UpdateTaskDtoValidator()
        {
            When(x => x.Title != null, () => {
                RuleFor(x => x.Title)
                    .MaximumLength(200).WithMessage("Title cannot be longer than 200 characters");
            });

            When(x => x.Description != null, () => {
                RuleFor(x => x.Description)
                    .MaximumLength(1000).WithMessage("Description cannot be longer than 1000 characters");
            });

            When(x => x.Priority != null, () => {
                RuleFor(x => x.Priority)
                    .Must(BeValidPriority).WithMessage("Invalid priority value");
            });

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required")
                .Must(d => d > DateTime.UtcNow).WithMessage("Date must be in the future");
        }

        private bool BeValidPriority(string? priority)
        {
            if (priority == null) return true;
            return Enum.TryParse(typeof(TaskPriority), priority, true, out _);
        }
    }
}