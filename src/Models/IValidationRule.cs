namespace PropertyValidator.Models
{
    public interface IValidationRule
    {
        string? PropertyName { get; set; }
        bool Validate(object? value);
        string ErrorMessage { get; }
        string? ErrorMessageOverride { get; set; }
        string Error => ErrorMessageOverride ?? ErrorMessage;
    }
}
