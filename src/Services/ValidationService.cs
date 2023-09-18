using CrossUtility.Extensions;
using CrossUtility.Helpers;
using ObservableProperty.Services.Implementation;
using PropertyValidator.Exceptions;
using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PropertyValidator.Services
{
    public class ValidationService : IValidationService
    {
        private object? notifiableModel;
        private bool autofill;
        private TimeSpan? delay;

        private CancellationTokenSource? cts;
        private MethodInfo? methodInfo;
        private object? ruleCollection;
        private bool IsInitialized => this.notifiableModel != null;

        public event EventHandler<ValidationResultArgs>? PropertyInvalid;

        public IRuleCollection<TNotifiableModel> For<TNotifiableModel>(
            TNotifiableModel notifiableModel,
            bool autofill,
            TimeSpan? delay)
            where TNotifiableModel : INotifyPropertyChanged
        {
            if (IsInitialized)
                throw new InvalidOperationException($"'{nameof(For)}' may only be called once.");

            this.notifiableModel = notifiableModel;
            this.autofill = autofill;
            this.delay = delay;

            return BuildRuleCollection(notifiableModel);
        }

        private IRuleCollection<TNotifiableModel> BuildRuleCollection<TNotifiableModel>(TNotifiableModel model)
            where TNotifiableModel : INotifyPropertyChanged
        {
            var observable = new ObservablePropertyChanged();
            var action = observable.Observe(model);
            var collection = new RuleCollection<TNotifiableModel>(action, model);

            var type = typeof(RuleCollection<TNotifiableModel>);
            this.ruleCollection = collection;
            this.methodInfo = type.GetMethod(nameof(RuleCollection<INotifyPropertyChanged>.GetRules));

            collection.ValidationResult += (sender, e) => ValidateByProperty(e.Name, e.Value).FireAndForget();

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

            if (this.autofill)
            {
                var inpc = this.notifiableModel as INotifyPropertyChanged;
                resultArgs.FillErrorProperty(inpc!);
            }

            PropertyInvalid?.Invoke(this, resultArgs);
        }

        public static ValidationResultArgs GetValidationResultArgs(
            string propertyName,
            object? propertyValue,
            IEnumerable<IValidationRule> validatedRules)
        {
            var errorMessages = validatedRules
                .Where(it => !it.Validate(propertyValue))
                .Select(it => it.Error);

            var errorDictionary = new Dictionary<string, IEnumerable<string?>> {
                [propertyName] = errorMessages
            };

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
                var value = property.GetValue(target, null);

                foreach (var rule in rules)
                {
                    if (rule.Validate(value))
                        continue;

                    Debug.Log("property {{ name: {0}, value: {1} }}, rule: {2}", propertyName, value, rule);

                    errorDictionary.TryGetValue(propertyName, out var oldList);
                    var errorList = new List<string?>(oldList ?? Enumerable.Empty<string?>())
                    {
                        rule.ErrorMessage
                    };
                    errorDictionary[propertyName] = errorList;
                }
            }

            return new ValidationResultArgs(null, errorDictionary);
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

        private void EnsureEntryMethodInvoked()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"Please use '{nameof(For)}' before invoking this method.");
        }

        public void EnsurePropertiesAreValid()
        {
            EnsureEntryMethodInvoked();

            var eventArgs = GetValidationResultArgs(this.notifiableModel!, GetRules());
            if (eventArgs.FirstError != null)
                throw new PropertyException(eventArgs);
        }

        public bool Validate()
        {
            EnsureEntryMethodInvoked();
            return ValidateRuleCollection(GetRules(), this.notifiableModel!);
        }

        public static bool ValidateRuleCollection(
            IDictionary<string, IEnumerable<IValidationRule>> ruleCollection,
            object target)
        {
            var resultArgs = GetValidationResultArgs(target, ruleCollection);
            return resultArgs.ErrorMessages?.Any() != true;
        }
    }
}
