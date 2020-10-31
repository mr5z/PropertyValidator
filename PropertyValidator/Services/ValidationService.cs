using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.ComponentModel;
using System.Reflection;
using PropertyValidator.Extensions;
using PropertyValidator.Models;

namespace PropertyValidator.Services
{
    public class ValidationService : IValidationService
    {
        private INotifyPropertyChanged notifiableModel;
        private object ruleCollection;
        private MethodInfo methodInfo;

        public event EventHandler<ValidationResultArgs> PropertyInvalid;

        public RuleCollection<TNotifiableModel> For<TNotifiableModel>(TNotifiableModel notifiableModel) 
            where TNotifiableModel : INotifyPropertyChanged
        {
            this.notifiableModel = notifiableModel;
            ruleCollection = new RuleCollection<TNotifiableModel>(notifiableModel);
            notifiableModel.PropertyChanged += NotifiableModel_PropertyChanged;
            var type = typeof(RuleCollection<TNotifiableModel>);
            methodInfo = type.GetMethod(nameof(RuleCollection<INotifyPropertyChanged>.GetRules));
            return (RuleCollection<TNotifiableModel>)ruleCollection;
        }

        private List<IValidationRule> GetRules()
        {
            var returnValue = methodInfo.Invoke(ruleCollection, null);
            return (List<IValidationRule>)returnValue;
        }

        private void NotifiableModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var targetProperties = GetRules().Select(it => it.PropertyName);
            if (!targetProperties.Contains(e.PropertyName))
                return;

            Validate(e.PropertyName);

            var errorMessages = GetRules()
                .Where(it => it.HasError && it.PropertyName == e.PropertyName)
                .Select(it => new { it.PropertyName, ErrorMessage = it.ErrorMessageOverride ?? it.ErrorMessage })
                .GroupBy(it => it.PropertyName)
                .ToDictionary(group => group.Key, g => g.Select(it => it.ErrorMessage));

            PropertyInvalid?.Invoke(this, new ValidationResultArgs(e.PropertyName, errorMessages));
        }

        public List<string> GetErrorMessages<TNotifiableModel>(
            TNotifiableModel _, 
            Expression<Func<TNotifiableModel, object>> expression) 
            where TNotifiableModel : INotifyPropertyChanged
        {
            var propertyName = expression.GetMemberName();
            return GetRules()
                .Where(it => it.HasError && it.PropertyName == propertyName)
                .Select(it => it.ErrorMessage)
                .ToList();
        }

        public string GetErrorMessage<TNotifiableModel>(
            TNotifiableModel notifiableProperty, 
            Expression<Func<TNotifiableModel, object>> expression) 
            where TNotifiableModel : INotifyPropertyChanged
        {
            return GetErrorMessages(notifiableProperty, expression).FirstOrDefault();
        }

        public bool HasError()
        {
            return GetRules().Any(x => x.HasError);
        }

        public bool Validate()
        {
            return ValidateImpl();
        }

        public bool Validate(string propertyName)
        {
            return ValidateImpl(propertyName);
        }

        private bool ValidateImpl(string propertyName = null)
        {
            return ValidateRuleCollection(GetRules(), notifiableModel, propertyName);
        }

        public static bool ValidateRuleCollection(
            List<IValidationRule> ruleCollection,
            object owner,
            string propertyName = null)
        {
            bool noErrors = true;
            foreach (var rule in ruleCollection)
            {
                if (!string.IsNullOrEmpty(propertyName) && rule.PropertyName != propertyName)
                    continue;

                var property = owner.GetType().GetProperty(rule.PropertyName);
                var value = property.GetValue(owner, null);
                rule.Validate(value);
                noErrors = noErrors && !rule.HasError;
            }
            return noErrors;
        }
    }
}
