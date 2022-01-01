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
    public class PropertyChangedValidationEventArgs : EventArgs
    {
        public string Name { get; init; }
        public object? Value { get; init; }
        public PropertyChangedValidationEventArgs(string name, object? value)
        {
            Name = name;
            Value = value;
        }
    }

    public class NewRuleCollection<TModel> : IRuleCollection<TModel> where TModel : notnull
    {
        private readonly object target;
        private readonly ActionCollection<TModel> actionCollection;
        private readonly Dictionary<string, IEnumerable<IValidationRule>> validationRules = new();

        public event EventHandler<PropertyChangedValidationEventArgs>? ValidationResult;

        public NewRuleCollection(ActionCollection<TModel> actionCollection, object target)
        {
            this.actionCollection = actionCollection;
            this.target = target;
        }

        private void TargetInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.target == null)
                return;

            var eventName = nameof(INotifyPropertyChanged.PropertyChanged);
            var field = ReflectionHelper.GetField(target.GetType(), eventName);
            var eventDelegate = (MulticastDelegate)field!.GetValue(target);
            var properties = this.target.GetType().GetProperties();
            var propInfo = properties.FirstOrDefault(e => e.GetValue(target) == sender);
            var eventArgs = new PropertyChangedEventArgs(propInfo.Name);
            eventDelegate.DynamicInvoke(this.target, eventArgs);
        }

        public IRuleCollection<TModel> AddRule<TProperty>(
            Expression<Func<TModel, TProperty>> expression,
            params ValidationRule<TProperty>[] rules)
        {
            return AddRule(expression, null, rules);
        }

        public IRuleCollection<TModel> AddRule<TProperty>(
            [NotNull] string propertyName,
            params ValidationRule<TProperty>[] rules)
        {
            return AddRule(propertyName, null, rules);
        }

        public IRuleCollection<TModel> AddRule<TProperty>(
            Expression<Func<TModel, TProperty>> expression,
            string? errorMessageOverride,
            params ValidationRule<TProperty>[] rules)
        {
            var propertyName = expression.GetMemberName();
            var propInfo = expression.GetPropertyInfo();
            var result = AddRule(propertyName, errorMessageOverride, rules);
            var value = propInfo.GetValue(this.target, null);
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
            RegisterRuleFor(propertyName, rules);
            return this;
        }

        private void RegisterRuleFor<TProperty>(string propertyName, params ValidationRule<TProperty>[] rules)
        {
            Array.ForEach(rules, rule => rule.PropertyName = propertyName);
            this.validationRules[propertyName] = rules;
            this.actionCollection.When<TProperty>(propertyName, it =>
            {
                var eventArgs = new PropertyChangedValidationEventArgs(propertyName, it);
                ValidationResult?.Invoke(this, eventArgs);
            });
        }

        public IReadOnlyCollection<IValidationRule> GetRules()
        {
            return this.validationRules.Values.SelectMany(it => it).ToList();
        }

        public IReadOnlyDictionary<string, IEnumerable<IValidationRule>> GetNewRules()
        {
            return this.validationRules;
        }
    }
}
