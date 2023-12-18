using CrossUtility.Extensions;
using ObservableProperty.Services.Implementation;
using PropertyValidator.Exceptions;
using PropertyValidator.Helpers;
using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PropertyValidator.Services;

public class ValidationService : IValidationService
{
    private object? notifiableModel;
    private TimeSpan? delay;

    private CancellationTokenSource? cts;
    private MethodInfo? methodInfo;
    private object? ruleCollection;
    private bool IsInitialized => this.notifiableModel != null;

    private IDictionary<string, string?> recentErrors = null!;
    private Func<IEnumerable<string?>, string>? errorFormatter = null;

    public event EventHandler<ValidationResultArgs>? PropertyInvalid;

    public IRuleCollection<TNotifiableModel> For<TNotifiableModel>(
        TNotifiableModel notifiableModel,
        TimeSpan? delay)
        where TNotifiableModel : INotifyPropertyChanged, INotifiableModel
    {
        if (IsInitialized)
            throw new InvalidOperationException($"'{nameof(For)}' may only be called once.");

        this.notifiableModel = notifiableModel;
        this.delay = delay;

        return BuildRuleCollection(notifiableModel);
    }

    private IRuleCollection<TNotifiableModel> BuildRuleCollection<TNotifiableModel>(TNotifiableModel model)
        where TNotifiableModel : INotifyPropertyChanged, INotifiableModel
    {
        var observable = new ObservablePropertyChanged();
        var action = observable.Observe(model);
        var collection = new RuleCollection<TNotifiableModel>(action, model);
        var recentErrors = new ObservableDictionary<string, string?>();

        var type = typeof(RuleCollection<TNotifiableModel>);
        this.ruleCollection = collection;
        this.methodInfo = type.GetMethod(nameof(RuleCollection<INotifyPropertyChanged>.GetRules));
        this.recentErrors = recentErrors;

        collection.ValidationResult += (sender, e) => ValidateByProperty(e.Name, e.Value).FireAndForget();
        recentErrors.CollectionChanged += (sender, e) => model.NotifyErrorPropertyChanged();

        return collection;
    }

    private async Task ValidateByProperty<TValue>(string propertyName, TValue? value)
    {
        if (await ShouldCancel())
            return;

        var validationRules = GetRules();

        if (!validationRules.TryGetValue(propertyName, out var propertyRules))
            throw new InvalidOperationException($"'{propertyName}' is not registered to validation rules.");

        var resultArgs = GetValidationResultArgs(propertyName, value, propertyRules);
        this.recentErrors[propertyName] = null;

        UpdateRecentErrors(resultArgs);

        PropertyInvalid?.Invoke(this, resultArgs);
    }

    internal static ValidationResultArgs GetValidationResultArgs(
        string propertyName,
        object? propertyValue,
        IEnumerable<IValidationRule> validatedRules)
    {
        var errorMessages = ValidatePropertyValue(validatedRules, propertyValue);

        var errorDictionary = new Dictionary<string, IEnumerable<string?>>();

        if (errorMessages.Any())
        {
            errorDictionary[propertyName] = errorMessages;
        }

        return new ValidationResultArgs(propertyName, errorDictionary);
    }

    private static ValidationResultArgs GetValidationResultArgs(
        object target,
        IDictionary<string, IEnumerable<IValidationRule>> validationRules)
    {
        var type = target.GetType();
        var errorDictionary = new Dictionary<string, IEnumerable<string?>>();

        foreach (var entry in validationRules)
        {
            var (propertyName, rules) = entry;
            var property = type.GetProperty(propertyName);
            var value = property.GetValue(target);

            var errorMessages = ValidatePropertyValue(rules, value);

            if (!errorMessages.Any())
            {
                continue;
            }

            if (errorDictionary.TryGetValue(propertyName, out var oldList))
            {
                errorDictionary[propertyName] = oldList.Concat(errorMessages);
            }
            else
            {
                errorDictionary[propertyName] = errorMessages;
            }
        }

        return new ValidationResultArgs(null, errorDictionary);
    }

    private static IEnumerable<string> ValidatePropertyValue(IEnumerable<IValidationRule> rules, object? value)
    {
        return rules
            .Where(rule => !rule.Validate(value))
            .Select(rule => rule.Error);
    }

    private async Task<bool> ShouldCancel()
    {
        if (this.delay == null)
            return false;

        this.cts?.Cancel();
        this.cts = new CancellationTokenSource();
        try
        {
            await Task.Delay(this.delay.Value, this.cts.Token);
            return false;
        }
        catch (TaskCanceledException)
        {
            return true;
        }
    }

    private IDictionary<string, IEnumerable<IValidationRule>> GetRules()
    {
        var returnValue = this.methodInfo!.Invoke(this.ruleCollection!, null);
        return (IDictionary<string, IEnumerable<IValidationRule>>)returnValue;
    }

    private void EnsureEntryMethodInvoked([CallerMemberName]string? methodCaller = null)
    {
        if (!IsInitialized)
            throw new InvalidOperationException($"Please use '{nameof(For)}(<target object>)' before invoking {methodCaller}.");
    }

    private void UpdateRecentErrors(ValidationResultArgs resultArgs)
    {
        foreach (var entry in resultArgs.ErrorDictionary)
        {
            var formattedMessage = (this.errorFormatter != null) ? this.errorFormatter.Invoke(entry.Value) : string.Join(", ", entry.Value);
            this.recentErrors[entry.Key] = formattedMessage;
        }
    }

    public void EnsurePropertiesAreValid()
    {
        EnsureEntryMethodInvoked();

        var resultArgs = GetValidationResultArgs(this.notifiableModel!, GetRules());
        this.recentErrors.Clear();

        UpdateRecentErrors(resultArgs);

        if (resultArgs.FirstError != null)
            throw new PropertyException(resultArgs);
    }

    public bool Validate()
    {
        EnsureEntryMethodInvoked();
        return ValidateRuleCollection(GetRules(), this.notifiableModel!);
    }

    internal static bool ValidateRuleCollection(
        IDictionary<string, IEnumerable<IValidationRule>> ruleCollection,
        object target)
    {
        var resultArgs = GetValidationResultArgs(target, ruleCollection);
        return !resultArgs.HasError;
    }

    public IDictionary<string, string?> GetErrors()
    {
        return this.recentErrors;
    }

    public void SetErrorFormatter(Func<IEnumerable<string?>, string> errorFormatter)
    {
        this.errorFormatter = errorFormatter;
    }
}
