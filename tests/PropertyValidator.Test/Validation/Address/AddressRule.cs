using PropertyValidator.Models;
using PropertyValidator.Test.Models;

namespace PropertyValidator.Test.Validation
{
    public class AddressRule : MultiValidationRule<Address>
    {
        protected override IRuleCollection<Address> ConfigureRules(IRuleCollection<Address> ruleCollection)
        {
            return ruleCollection
                .AddRule(e => e.City, new RequiredRule())
                .AddRule(e => e.CountryIsoCode, new CountryIsoCodeRule())
                .AddRule(e => e.PostalCode, new PostalCodeRule())
                .AddRule(e => e.StreetAddress, new RequiredRule(), new MaxLengthRule(100));
        }
    }
}
