using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class AtLeastOneNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && Regex.IsMatch(str, "[0-9]"))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult(ErrorMessage ?? "The password must contain at least one number.");

    }
}