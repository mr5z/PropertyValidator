using ObservableProperty.Services;
using PropertyValidator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PropertyValidator.Models;

public abstract class MultiValidationRule<T> : ValidationRule<T> where T : notnull
{
    private IReadOnlyDictionary<string, IEnumerable<IValidationRule>>? validationRules;

    private string? lastErrorMessage;

    protected abstract IRuleCollection<T> ConfigureRules(IRuleCollection<T> ruleCollection);

    public sealed override bool IsValid(T? value)
    {
        if (value == null)
        {
            // TODO what to do?
            return false;
        }

        if (this.validationRules != null)
        {
            return ValidationService.ValidateRuleCollection(
                value,
                GetValidationRules()
            ).HasError == false;
        }

        var actionCollection = new ActionCollection<T>();
        var ruleCollection = new RuleCollection<T>(actionCollection, value);
        this.validationRules = ConfigureRules(ruleCollection).GetRules();

        if (value is not INotifyPropertyChanged inpc)
        {
            return ValidationService.ValidateRuleCollection(
                value,
                GetValidationRules()
            ).HasError == false;
        }

        inpc.PropertyChanged -= Target_PropertyChanged;
        inpc.PropertyChanged += Target_PropertyChanged;

        return ValidationService.ValidateRuleCollection(
            value,
            GetValidationRules()
        ).HasError == false;
    }

    private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var dictionaryRules = GetValidationRules().Where(r => r.Key == e.PropertyName);
        var listRules = dictionaryRules.SelectMany(it => it.Value);

        var resultArgs = ValidationService.ValidateRuleCollection(e.PropertyName, null, listRules!);

        this.lastErrorMessage = resultArgs.HasError ? $"{e.PropertyName}: {resultArgs.FirstError}" : null;
    }

    private IDictionary<string, IEnumerable<IValidationRule>> GetValidationRules()
    {
        return this.validationRules!.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    // TODO convert to List<string>
    public sealed override string ErrorMessage => this.lastErrorMessage ?? $"{{{string.Join(", ", ErrorsAsJsonString())}}}";

    private IEnumerable<string?> ErrorsAsJsonString()
    {
        return this.validationRules!
            .SelectMany(it => it.Value)
            .Select(e => $"\"{e.PropertyName}\":\"{e.Error}\"");
    }
}
