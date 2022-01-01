using PropertyValidator.Models;

namespace PropertyValidator.Test.Validation
{
    public class RequiredRule : ValidationRule<string?>
    {
        public override string ErrorMessage => "Izz required!";

        public override bool IsValid(string? value)
            => !string.IsNullOrEmpty(value);
    }
}
