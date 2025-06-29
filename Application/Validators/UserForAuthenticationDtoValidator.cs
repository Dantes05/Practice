using FluentValidation;
using Application.DTOs;

namespace Application.Validators
{
    public class UserForAuthenticationDtoValidator : AbstractValidator<UserForAuthenticationDto>
    {
        public UserForAuthenticationDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}