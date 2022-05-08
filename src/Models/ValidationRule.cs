namespace PropertyValidator.Models
{
    public abstract class ValidationRule<T> : IValidationRule
    {
        public string? PropertyName { get; set; }

        public bool Validate(object? value)
        {
            return IsValid(value == null ? default : (T)value);
        }

        // TODO support async validations
        //public virtual Task<bool> IsValidAsync(T value) => Task.FromResult(IsValid(value));

        public abstract bool IsValid(T? value);

        public abstract string ErrorMessage { get; }

        public string? ErrorMessageOverride { get; set; }
    }
}