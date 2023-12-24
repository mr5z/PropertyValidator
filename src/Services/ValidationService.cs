using CrossUtility.Extensions;
using ObservableProperty.Services.Implementation;
using PropertyValidator.Exceptions;
using PropertyValidator.Helpers;
using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PropertyValidator.Services;

public class ValidationService : IValidationService
{
    private object? notifiableModel;
    private bool autofill;
    private TimeSpan? delay;

    private CancellationTokenSource? cts;
    private MethodInfo? methodInfo;
    private object? ruleCollection;
    private bool IsInitialized => this.notifiableModel != null;

    private IDictionary<string, string?> recentErrors = null!;
    private Func<IEnumerable<string?>, string>? errorFormatter;

    public event EventHandler<ValidationResultArgs>? PropertyInvalid;

    public IRuleCollection<TNotifiableModel> For<TNotifiableModel>(
        TNotifiableModel notifiableModel,
        bool autofill,
        TimeSpan? delay)
        where TNotifiableModel : INotifyPropertyChanged, INotifiableModel
    {
        if (IsInitialized)
            throw new InvalidOperationException($"'{nameof(For)}' may only be called once.");

        this.notifiableModel = notifiableModel;
        this.autofill = autofill;
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

        collection.ValidationResult += (sender, e) =>
        {
            if (autofill)
            {
                ValidateByProperty(e.Name, e.Value).FireAndForget();
            }
        };
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

        var resultArgs = ValidateRuleCollection(propertyName, value, propertyRules);

        this.recentErrors[propertyName] = null;

        UpdateRecentErrors(resultArgs);

        PropertyInvalid?.Invoke(this, resultArgs);
    }

    internal static ValidationResultArgs ValidateRuleCollection(
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

        return new ValidationResultArgs(errorDictionary);
    }

    internal static ValidationResultArgs ValidateRuleCollection(
        object target,
        IDictionary<string, IEnumerable<IValidationRule>> validationRules)
    {
        var errorDictionary = new Dictionary<string, IEnumerable<string?>>();

        foreach (var (propertyName, rules) in validationRules)
        {
            var property = target.GetType().GetProperty(propertyName)
                ?? throw new InvalidOperationException($"'{target.GetType().Name}.{propertyName}' cannot be accessed.");

            var value = property.GetValue(target);
            var errorMessages = ValidatePropertyValue(rules, value);

            if (errorMessages.Any())
            {
                errorDictionary[propertyName] = errorDictionary.TryGetValue(propertyName, out var oldList)
                    ? oldList.Concat(errorMessages)
                    : errorMessages;
            }
        }

        return new ValidationResultArgs(errorDictionary);
    }

    private static IReadOnlyCollection<string> ValidatePropertyValue(IEnumerable<IValidationRule> rules, object? value)
    {
        return rules
            .Where(rule => !rule.Validate(value))
            .Select(rule => rule.Error)
            .ToArray();
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
            var formattedMessage = (this.errorFormatter != null)
                ? this.errorFormatter.Invoke(entry.Value)
                : string.Join(", ", entry.Value);
            this.recentErrors[entry.Key] = formattedMessage;
        }
    }

    public void EnsurePropertiesAreValid()
    {
        EnsureEntryMethodInvoked();

        var resultArgs = ValidateRuleCollection(this.notifiableModel!, GetRules());
        this.recentErrors.Clear();

        UpdateRecentErrors(resultArgs);

        if (resultArgs.FirstError != null)
            throw new PropertyException(resultArgs);
    }

    public bool Validate()
    {
        EnsureEntryMethodInvoked();
        
        var result = ValidateRuleCollection(this.notifiableModel!, GetRules());
        UpdateRecentErrors(result);
        
        return !result.HasError;
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
