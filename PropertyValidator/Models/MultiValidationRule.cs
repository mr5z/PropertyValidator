using PropertyValidator.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PropertyValidator.Models
{
    public abstract class MultiValidationRule<T> : ValidationRule<T>
    {
        private List<IValidationRule>? validationRules;

        protected abstract RuleCollection<T> ConfigureRules(RuleCollection<T> ruleCollection);

        public sealed override bool IsValid(T? value)
        {
            if (value == null)
            {
                // TODO what to do?
                return false;
            }

            if (validationRules == null)
            {
                validationRules = ConfigureRules(new RuleCollection<T>(value)).GetRules();
                if (value is INotifyPropertyChanged target)
                {
                    target.PropertyChanged -= Target_PropertyChanged;
                    target.PropertyChanged += Target_PropertyChanged;
                }
            }
            return ValidationService.ValidateRuleCollection(validationRules, value);
        }

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidationService.ValidateRuleCollection(validationRules!, sender);
        }

        // TODO convert to List<string>
        public override sealed string ErrorMessage => $"[{string.Join(",", ErrorsAsJsonString())}]";

        private IEnumerable<string> ErrorsAsJsonString()
            => validationRules
                .Where(e => e.HasError)
                .Select(e => $"{e.PropertyName}:\"{e.ErrorMessage}\"");
    }
}
