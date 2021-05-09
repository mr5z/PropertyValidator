using PropertyValidator.Extensions;
using PropertyValidator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyValidator.Models
{
    public class RuleCollection<TModel>
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
            var field = ReflectionHelper.GetField(target.GetType(), eventName);
            var eventDelegate = (MulticastDelegate)field!.GetValue(target);
            var properties = target.GetType().GetProperties();
            var propInfo = properties.FirstOrDefault(e => e.GetValue(target) == sender);
            var eventArgs = new PropertyChangedEventArgs(propInfo.Name);
            eventDelegate.DynamicInvoke(target, eventArgs);
        }

        public RuleCollection<TModel> AddRule<TProperty>
            (Expression<Func<TModel, TProperty>> expression,
            params ValidationRule<TProperty>[] rules)
            => AddRule(expression, null, rules);

        public RuleCollection<TModel> AddRule<TProperty>(
            [NotNull]
            string propertyName,
            params ValidationRule<TProperty>[] rules)
            => AddRule(propertyName, null, rules);

        public RuleCollection<TModel> AddRule<TProperty>(
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

        public RuleCollection<TModel> AddRule<TProperty>(
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

        public List<IValidationRule> GetRules()
            => validationRuleList;
    }
}
