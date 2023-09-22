using PropertyValidator.Models;
using PropertyValidator.Test.Models;
using PropertyValidator.ValidationPack;

namespace PropertyValidator.Test.Validation
{
    public class ItemRule : MultiValidationRule<Item>
    {
        protected override IRuleCollection<Item> ConfigureRules(IRuleCollection<Item> ruleCollection)
        {
            return ruleCollection
                .AddRule(e => e.Id, new StringRequiredRule())
                .AddRule(e => e.Text, new StringRequiredRule())
                .AddRule(e => e.Description, new MaxLengthRule(20));
        }
    }
}
