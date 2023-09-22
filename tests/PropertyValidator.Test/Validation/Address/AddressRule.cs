using PropertyValidator.Models;
using PropertyValidator.Test.Models;
using PropertyValidator.ValidationPack;

namespace PropertyValidator.Test.Validation
{
    public class AddressRule : MultiValidationRule<Address>
    {
        protected override IRuleCollection<Address> ConfigureRules(IRuleCollection<Address> ruleCollection)
        {
            return ruleCollection
                .AddRule(e => e.City, new StringRequiredRule())
                .AddRule(e => e.CountryIsoCode, new CountryIsoCodeRule())
                .AddRule(e => e.PostalCode, new PostalCodeRule())
                .AddRule(e => e.StreetAddress, new StringRequiredRule(), new MaxLengthRule(100));
        }
    }
}
