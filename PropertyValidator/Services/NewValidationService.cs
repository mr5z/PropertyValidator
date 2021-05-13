using ObservableProperty.Services;
using ObservableProperty.Services.Implementation;
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
    public class NewValidationService : IValidationService
    {
        private IObservablePropertyChanged? observablePropertyChanged;
        private bool autofill;
        private TimeSpan? delay;
        private CancellationTokenSource? cts;
        private MethodInfo? methodInfo;
        private object? ruleCollection;
        private bool isInitialized;

        public event EventHandler<ValidationResultArgs>? PropertyInvalid;

        public IRuleCollection<TNotifiableModel> For<TNotifiableModel>(
            TNotifiableModel notifiableModel,
            bool autofill,
            TimeSpan? delay)
            where TNotifiableModel : INotifyPropertyChanged
        {
            if (isInitialized)
                throw new InvalidOperationException($"'{nameof(For)}' may only be called once.");

            this.autofill = autofill;
            this.delay = delay;
            notifiableModel.PropertyChanged += NotifiableModel_PropertyChanged;

            isInitialized = true;

            return BuildRuleCollection(notifiableModel);
        }

        private IRuleCollection<TNotifiableModel> BuildRuleCollection<TNotifiableModel>(TNotifiableModel model)
            where TNotifiableModel : INotifyPropertyChanged
        {
            observablePropertyChanged = new ObservablePropertyChanged();
            var type = typeof(NewRuleCollection<TNotifiableModel>);
            methodInfo = type.GetMethod(nameof(NewRuleCollection<INotifyPropertyChanged>.GetRules));
            var actionCollection = observablePropertyChanged.Observe(model);
            ruleCollection = new NewRuleCollection<TNotifiableModel>(actionCollection, model);
            return (NewRuleCollection<TNotifiableModel>)ruleCollection;
        }

        private async void NotifiableModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await PropertyChangedAsync(sender, e);
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

        private List<IValidationRule> GetRules()
        {
            var returnValue = methodInfo!.Invoke(ruleCollection, null);
            return (List<IValidationRule>)returnValue;
        }

        public void EnsurePropertiesAreValid()
        {
            throw new NotImplementedException();
        }

        public string? GetErrorMessage<TNotifiableModel>(TNotifiableModel _, Expression<Func<TNotifiableModel, object?>> expression) where TNotifiableModel : INotifyPropertyChanged
        {
            throw new NotImplementedException();
        }

        public string? GetErrorMessage<TNotifiableModel>(string propertyName)
        {
            throw new NotImplementedException();
        }

        public bool Validate()
        {
            throw new NotImplementedException();
        }
    }
}
