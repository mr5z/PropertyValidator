using CrossUtility.Extensions;
using ObservableProperty.Services;
using PropertyValidator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyValidator.Models;

public record PropertyChangedValidationEventArgs(string Name, object? Value);

public class RuleCollection<TModel>(ActionCollection<TModel> actionCollection, object target) : IRuleCollection<TModel> where TModel : notnull
{
    private readonly object target = target;
    private readonly ActionCollection<TModel> actionCollection = actionCollection;
    private readonly Dictionary<string, IEnumerable<IValidationRule>> validationRules = [];

    public event EventHandler<PropertyChangedValidationEventArgs>? ValidationResult;

    private void TargetInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (this.target == null)
            return;

        var eventName = nameof(INotifyPropertyChanged.PropertyChanged);
        var field = ReflectionHelper.GetField(this.target.GetType(), eventName);
        var eventDelegate = (MulticastDelegate)field!.GetValue(this.target);
        var properties = this.target.GetType().GetProperties();
        var propInfo = properties.First(e => e.GetValue(this.target) == sender);
        var eventArgs = new PropertyChangedEventArgs(propInfo.Name);
        eventDelegate.DynamicInvoke(this.target, eventArgs);
    }

    public IRuleCollection<TModel> AddRule<TProperty>(
        Expression<Func<TModel, TProperty>> expression,
        params IValidationRule[] rules)
    {
        return AddRule(expression, null, rules);
    }

    public IRuleCollection<TModel> AddRule<TProperty>(
        [NotNull] string propertyName,
        params IValidationRule[] rules)
    {
        return AddRule<TProperty>(propertyName, null, rules);
    }

    public IRuleCollection<TModel> AddRule<TProperty>(
        Expression<Func<TModel, TProperty>> expression,
        string? errorMessageOverride,
        params IValidationRule[] rules)
    {
        var propertyName = expression.GetMemberName();
        var propInfo = expression.GetPropertyInfo();
        var result = AddRule<TProperty>(propertyName, errorMessageOverride, rules);
        var value = propInfo.GetValue(this.target, null);
        if (value is INotifyPropertyChanged notifiableObject)
        {
            notifiableObject.PropertyChanged -= TargetInstance_PropertyChanged;
            notifiableObject.PropertyChanged += TargetInstance_PropertyChanged;
        }
        return result;
    }

    public IRuleCollection<TModel> AddRule<TProperty>(
        [NotNull] string propertyName,
        string? errorMessageOverride,
        params IValidationRule[] rules)
    {
        RegisterRuleFor<TProperty>(propertyName, errorMessageOverride, rules);
        return this;
    }

    private void RegisterRuleFor<TProperty>(string propertyName, string? errorMessageOverride, params IValidationRule[] rules)
    {
        Array.ForEach(rules, rule => {
            rule.PropertyName = propertyName;
            rule.ErrorMessageOverride = errorMessageOverride;
        });
        this.validationRules[propertyName] = rules;
        this.actionCollection.When<TProperty>(propertyName, it => {
            var eventArgs = new PropertyChangedValidationEventArgs(propertyName, it);
            ValidationResult?.Invoke(this, eventArgs);
        });
    }

    public IReadOnlyDictionary<string, IEnumerable<IValidationRule>> GetRules()
    {
        return this.validationRules;
    }
}
