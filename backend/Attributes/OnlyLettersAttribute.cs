using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class OnlyLettersAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && Regex.IsMatch(str, @"^[a-zA-Z\s]+$"))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult(ErrorMessage ?? "Only letters and blank spaces are accepted.");
    }
}
