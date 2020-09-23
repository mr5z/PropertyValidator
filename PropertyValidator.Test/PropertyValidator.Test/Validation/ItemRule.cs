using PropertyValidator.Models;
using PropertyValidator.Test.Models;
using System;

namespace PropertyValidator.Test.Validation
{
    public class ItemRule : MultiValidationRule<Item>
    {
        protected override RuleCollection<Item> ConfigureRules(RuleCollection<Item> ruleCollection)
        {
            return ruleCollection
                .AddRule(e => e.Id, new RequiredRule())
                .AddRule(e => e.Text, new RequiredRule())
                .AddRule(e => e.Description, new LengthRule(20));
        }
    }
}
