using FluentValidation;
using System;

namespace Application.Validators
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, DateTime> MustBeFutureDate<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
        {
            return ruleBuilder.Must(date => date > DateTime.UtcNow)
                .WithMessage("Date must be in the future");
        }
    }
}