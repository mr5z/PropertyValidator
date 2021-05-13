using CrossUtility.Extensions;
using ObservableProperty.Services;
using PropertyValidator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyValidator.Models
{
    public class NewRuleCollection<TModel> : IRuleCollection<TModel> where TModel : notnull
    {
        private readonly object target;
        private readonly ActionCollection<TModel> actionCollection;
        private readonly Dictionary<string, IEnumerable<IValidationRule>> validationRules = new();

        public NewRuleCollection(ActionCollection<TModel> actionCollection, object target)
        {
            this.actionCollection = actionCollection;
            this.target = target;
        }

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

        public IRuleCollection<TModel> AddRule<TProperty>(
            Expression<Func<TModel, TProperty>> expression,
            params ValidationRule<TProperty>[] rules)
            => AddRule(expression, null, rules);

        public IRuleCollection<TModel> AddRule<TProperty>(
            [NotNull] string propertyName,
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
            [NotNull] string propertyName,
            string? errorMessageOverride,
            params ValidationRule<TProperty>[] rules)
        {
            validationRules[propertyName] = rules;
            actionCollection.When<TProperty>(propertyName, it => AA(propertyName, it));
            return this;
        }

        private void AA<TProperty>(string propertyName, TProperty property)
        {
            if (validationRules.TryGetValue(propertyName, out var ruleList))
                throw new InvalidOperationException($"'{propertyName}' is not registered to validation rules.");

            foreach (var rule in ruleList)
            {
                var isValid = rule.Validate(property);
            }
        }

        public IReadOnlyCollection<IValidationRule> GetRules()
            => validationRules.Values.SelectMany(it => it).ToList();
    }
}
