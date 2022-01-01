namespace PropertyValidator.Models
{
    public abstract class ValidationRule<T> : IValidationRule
    {
        public string? PropertyName { get; set; }

        public bool HasError { get; private set; }

        public bool Validate(object? value)
        {
            HasError = !IsValid(value == null ? default : (T)value);
            return !HasError;
        }

        // TODO support async validations
        //public virtual Task<bool> IsValidAsync(T value) => Task.FromResult(IsValid(value));

        public abstract bool IsValid(T? value);

        public abstract string ErrorMessage { get; }

        public string? ErrorMessageOverride { get; set; }
    }
}