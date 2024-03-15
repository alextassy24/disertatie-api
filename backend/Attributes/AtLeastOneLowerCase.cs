using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class AtLeastOneLowerCaseAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && Regex.IsMatch(str, "[a-zăîâșț]"))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult(ErrorMessage ?? "The password must contain at least one lower case letter.");

    }
}