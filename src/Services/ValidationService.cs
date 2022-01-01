using CrossUtility.Extensions;
using PropertyValidator.Exceptions;
using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PropertyValidator.Services
{
    public class ValidationService : IValidationService
    {
        private INotifyPropertyChanged? notifiableModel;
        private object? ruleCollection;
        private bool autofill;
        private TimeSpan? delay;
        private MethodInfo? methodInfo;
        private CancellationTokenSource? cts;

        public event EventHandler<ValidationResultArgs>? PropertyInvalid;

        public IRuleCollection<TNotifiableModel> For<TNotifiableModel>(
            [NotNull]
            TNotifiableModel notifiableModel,
            bool autofill,
            TimeSpan? delay) where TNotifiableModel : INotifyPropertyChanged
        {
            this.notifiableModel = notifiableModel;
            this.autofill = autofill;
            this.delay = delay;

            ruleCollection = new RuleCollection<TNotifiableModel>(notifiableModel);
            notifiableModel.PropertyChanged += NotifiableModel_PropertyChanged;
            var type = typeof(RuleCollection<TNotifiableModel>);
            methodInfo = type.GetMethod(nameof(RuleCollection<INotifyPropertyChanged>.GetRules));
            return (RuleCollection<TNotifiableModel>)ruleCollection;
        }

        private List<IValidationRule> GetRules()
        {
            EnsureEntryMethodInvoked();

            var returnValue = methodInfo!.Invoke(ruleCollection, null);
            return (List<IValidationRule>)returnValue;
        }

        private async void NotifiableModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var containsTarget = GetRules().Any(it => it.PropertyName == e.PropertyName);
            if (!containsTarget)
                return;

            // I really don't like to use `async void` but 
            // I can't make the .ContinueWith to propagate
            // the exception back to original context i.e., main thread
            // but to back up this solution, C# overlords say it's okay
            // to have async void but for top-level event handlers only
            // link: https://stackoverflow.com/a/19415703/2304737
            await PropertyChangedAsync(sender, e);

            // TODO not working but preferrable to use this
            //_ = PropertyChangedAsync(sender, e).ContinueWith(task =>
            //{
            //    //if (task.Exception != null)
            //    //    throw task.Exception;

            //    var threadId = Thread.CurrentThread.ManagedThreadId;
            //    Exception ex = task.Exception;
            //    while (ex is AggregateException && ex.InnerException != null)
            //        ex = ex.InnerException;
            //    throw ex;
            //},
            //cancellationToken: default,
            //continuationOptions: TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent,
            //scheduler: TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            if (delay != null)
            {
                cts?.Cancel();
                cts = new CancellationTokenSource();
                try
                {
                    await Task.Delay(delay.Value, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }

            Validate(e.PropertyName);

            var eventArgs = GetValidationResultArgs(e.PropertyName);
            if (autofill)
            {
                EnsureEntryMethodInvoked();
                eventArgs.FillErrorProperty(notifiableModel!);
            }
            PropertyInvalid?.Invoke(this, eventArgs);
        }

        private List<string?> GetErrorMessages(string propertyName) 
        {
            return GetRules()
                .Where(it => it.HasError && it.PropertyName == propertyName)
                .Select(it => it.Error)
                .ToList();
        }

        public string? GetErrorMessage<TNotifiableModel>(
            TNotifiableModel notifiableProperty, 
            Expression<Func<TNotifiableModel, object?>> expression) 
            where TNotifiableModel : INotifyPropertyChanged
        {
            var propertyName = expression.GetMemberName();
            return GetErrorMessages(propertyName).FirstOrDefault();
        }

        public string? GetErrorMessage<TNotifiableModel>(string propertyName)
        {
            return GetErrorMessages(propertyName).FirstOrDefault();
        }

        public bool HasError()
        {
            return GetRules().Any(x => x.HasError);
        }

        public bool Validate()
        {
            return ValidateImpl();
        }

        public bool Validate([NotNull] string propertyName)
        {
            return ValidateImpl(propertyName);
        }

        public void EnsurePropertiesAreValid()
        {
            ValidateAllProperties();
            var resultArgs = GetValidationResultArgs();
            var firstError = resultArgs.FirstError;
            if (!string.IsNullOrEmpty(firstError))
                throw new PropertyException(resultArgs);
        }

        private void ValidateAllProperties()
        {
            var ruleCollection = GetRules();
            foreach (var rule in ruleCollection)
            {
                Validate(rule.PropertyName!);
            }
        }

        private ValidationResultArgs GetValidationResultArgs(string? propertyName = null)
        {
            var enumerable = GetRules().AsEnumerable();

            if (!string.IsNullOrEmpty(propertyName))
            {
                enumerable = enumerable.Where(it => it.PropertyName == propertyName);
            }

            var errorMessages = enumerable
                .Select(it => new { it.PropertyName, ErrorMessage = it.HasError ? it.Error : null })
                .GroupBy(it => it.PropertyName)
                .ToDictionary(group => group.Key, g => g.Select(it => it.ErrorMessage));

            return new ValidationResultArgs(propertyName, errorMessages!);
        }

        private bool ValidateImpl(string? propertyName = null)
        {
            EnsureEntryMethodInvoked();
            return ValidateRuleCollection(GetRules(), notifiableModel!, propertyName);
        }

        private void EnsureEntryMethodInvoked()
        {
            if (notifiableModel == null)
                throw new InvalidOperationException($"Please use '{nameof(For)}' before invoking this method.");
        }

        public static bool ValidateRuleCollection(
            IEnumerable<IValidationRule> ruleCollection,
            object target,
            string? propertyName = null)
        {
            bool noErrors = true;
            var type = target.GetType();
            var filteredCollection = ruleCollection.Where(it =>
                 it.PropertyName != propertyName || string.IsNullOrEmpty(propertyName)
            );
            foreach (var rule in filteredCollection)
            {
                // if propertyName ay may laman
                // at magkaiba ng property value
                // then skip
                var property = type.GetProperty(rule.PropertyName);
                var value = property.GetValue(target, null);
                var isValid = rule.Validate(value);
                noErrors = noErrors && isValid;
            }
            return noErrors;
        }
    }
}
