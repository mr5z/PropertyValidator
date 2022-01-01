using PropertyValidator.Models;
using PropertyValidator.Test.Models;

namespace PropertyValidator.Test.Validation
{
    public class ItemRule : MultiValidationRule<Item>
    {
        protected override IRuleCollection<Item> ConfigureRules(IRuleCollection<Item> ruleCollection)
        {
            return ruleCollection
                .AddRule(e => e.Id, new RequiredRule())
                .AddRule(e => e.Text, new RequiredRule())
                .AddRule(e => e.Description, new MaxLengthRule(20));
        }
    }
}
