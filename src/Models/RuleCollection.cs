using CrossUtility.Extensions;
using PropertyValidator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyValidator.Models
{
    public class RuleCollection<TModel> : IRuleCollection<TModel>
    {
        private readonly List<IValidationRule> validationRuleList = new();
        private readonly object? target;

        public RuleCollection(object? target)
            => this.target = target;

        private void TargetInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (target == null)
                return;

            var eventName = nameof(INotifyPropertyChanged.PropertyChanged);
            var properties = target.GetType().GetProperties();
            var propInfo = properties.FirstOrDefault(e => e.GetValue(target) == sender);
            var eventArgs = new PropertyChangedEventArgs(propInfo.Name);
            InvokePropertyEvent(eventName, target, eventArgs);
        }

        private void InvokePropertyEvent(string eventName, object sender, EventArgs eventArgs)
        {
            var field = ReflectionHelper.GetField(sender.GetType(), eventName);
            var eventDelegate = (MulticastDelegate)field!.GetValue(sender);
            eventDelegate.DynamicInvoke(sender, eventArgs);
        }

        public IRuleCollection<TModel> AddRule<TProperty>
            (Expression<Func<TModel, TProperty>> expression,
            params ValidationRule<TProperty>[] rules)
            => AddRule(expression, null, rules);

        public IRuleCollection<TModel> AddRule<TProperty>(
            [NotNull]
            string propertyName,
            params ValidationRule<TProperty>[] rules)
            => AddRule(propertyName, null, rules);

        public IRuleCollection<TModel> AddRule<TProperty>(
            Expression<Func<TModel, TProperty>> expression,
            string? errorMessageOverride,
            params ValidationRule<TProperty>[] rules)
        {
            var propertyName = expression.GetMemberName();
            var propInfo = expression.GetPropertyInfo();
            var result = AddRule(propertyName, errorMessageOverride, rules);
            var value = propInfo.GetValue(target, null);
            if (value is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= TargetInstance_PropertyChanged;
                notifyPropertyChanged.PropertyChanged += TargetInstance_PropertyChanged;
            }
            return result;
        }

        public IRuleCollection<TModel> AddRule<TProperty>(
            [NotNull]
            string propertyName, 
            string? errorMessageOverride, 
            params ValidationRule<TProperty>[] rules)
        {
            foreach (var rule in rules)
            {
                rule.PropertyName = propertyName;
                rule.ErrorMessageOverride = errorMessageOverride;
                validationRuleList.Add(rule);
            }
            return this;
        }

        public IReadOnlyCollection<IValidationRule> GetRules()
            => validationRuleList;

        public IReadOnlyDictionary<string, IEnumerable<IValidationRule>> GetNewRules()
        {
            throw new NotImplementedException();
        }
    }
}
