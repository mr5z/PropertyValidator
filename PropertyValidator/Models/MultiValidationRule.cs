using PropertyValidator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PropertyValidator.Models
{
    public abstract class MultiValidationRule<T> : ValidationRule<T>
    {
        private List<IValidationRule> validationRules;

        protected abstract RuleCollection<T> ConfigureRules(RuleCollection<T> ruleCollection);

        public sealed override bool IsValid(T value)
        {
            if (validationRules == null)
            {
                validationRules = ConfigureRules(new RuleCollection<T>(value)).GetRules();
                if (value is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= NotifyPropertyChanged_PropertyChanged;
                    notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
                }
            }
            return ValidationService.ValidateRuleCollection(validationRules, value);
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidationService.ValidateRuleCollection(validationRules, sender);
        }

        public override sealed string ErrorMessage => $"[{string.Join(", ", validationRules.Where(e => e.HasError).Select(e => e.ErrorMessage))}]";
    }
}
