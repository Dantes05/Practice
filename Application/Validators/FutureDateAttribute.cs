using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.UtcNow
                    ? ValidationResult.Success
                    : new ValidationResult("Date must be in the future");
            }
            return new ValidationResult("Invalid date format");
        }
    }
}