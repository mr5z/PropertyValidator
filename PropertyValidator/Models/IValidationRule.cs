namespace PropertyValidator.Models
{
    public interface IValidationRule
    {
        string? PropertyName { get; set; }
        bool HasError { get; }
        bool Validate(object value);
        string? ErrorMessage { get; }
        string? ErrorMessageOverride { get; set; }
        string? Error => ErrorMessageOverride ?? ErrorMessage;
    }
}
