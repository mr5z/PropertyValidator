using PropertyValidator.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace PropertyValidator.Models
{
    public class RuleCollection<TNotifiableProperty> where TNotifiableProperty : INotifyPropertyChanged
    {
        private readonly List<IValidationRule> validationRuleList = new List<IValidationRule>();

        public RuleCollection<TNotifiableProperty> AddRule<T>(Expression<Func<TNotifiableProperty, object>> expression, params ValidationRule<T>[] rules)
        {
            var propertyName = expression.GetMemberName();
            return AddRule(propertyName, rules);
        }

        public RuleCollection<TNotifiableProperty> AddRule<T>(string propertyName, params ValidationRule<T>[] rules)
        {
            foreach (var rule in rules)
            {
                rule.PropertyName = propertyName;
                validationRuleList.Add(rule);
            }
            return this;
        }

        public List<IValidationRule> GetRules()
        {
            return validationRuleList;
        }
    }
}
