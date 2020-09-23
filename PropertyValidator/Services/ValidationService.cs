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

        public RuleCollection<TNotifiableModel> For<TNotifiableModel>(TNotifiableModel notifiableModel) where TNotifiableModel : INotifyPropertyChanged
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
            Validate(e.PropertyName);
            var errorMessages = GetRules()
                .Where(x => x.HasError && x.PropertyName == e.PropertyName)
                .Select(x => x.ErrorMessageOverride ?? x.ErrorMessage);
            PropertyInvalid?.Invoke(this, new ValidationResultArgs(e.PropertyName, errorMessages));
        }

        public List<string> GetErrorMessages<TNotifiableModel>(TNotifiableModel _, Expression<Func<TNotifiableModel, object>> expression) where TNotifiableModel : INotifyPropertyChanged
        {
            var propertyName = expression.GetMemberName();
            var rules = GetRules();
            return rules
                .Where(x => x.HasError && x.PropertyName == propertyName)
                .Select(x => x.ErrorMessage)
                .ToList();
        }

        public string GetErrorMessage<TNotifiableModel>(TNotifiableModel notifiableProperty, Expression<Func<TNotifiableModel, object>> expression) where TNotifiableModel : INotifyPropertyChanged
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

        public static bool ValidateRuleCollection(List<IValidationRule> ruleCollection, object owner, string propertyName = null)
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
