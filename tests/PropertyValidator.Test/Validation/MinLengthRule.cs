using PropertyValidator.Models;

namespace PropertyValidator.Test.Validation
{
    public class MinLengthRule : ValidationRule<string?>
    {
        private readonly int minLength;

        public MinLengthRule(int minLength)
        {
            this.minLength = minLength;
        }

        public override string ErrorMessage => $"Characters must be at least {minLength} characters long";

        public override bool IsValid(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            return value.Length >= minLength;
        }
    }
}
