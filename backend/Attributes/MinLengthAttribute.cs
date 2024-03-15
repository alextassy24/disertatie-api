using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace backend.Attributes
{
    public class MinLengthAttribute : ValidationAttribute
    {
        private readonly int _minLength;
        public MinLengthAttribute(int minLength)
        {
            _minLength = minLength;
        }
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && str.Length >= _minLength)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? $"The password must be at least {_minLength} characters long.");
        }
    }
}