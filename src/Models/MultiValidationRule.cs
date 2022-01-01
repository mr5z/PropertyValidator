using PropertyValidator.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PropertyValidator.Models
{
    public abstract class MultiValidationRule<T> : ValidationRule<T> where T : notnull
    {
        private IReadOnlyCollection<IValidationRule>? validationRules;

        protected abstract IRuleCollection<T> ConfigureRules(IRuleCollection<T> ruleCollection);

        public sealed override bool IsValid(T? value)
        {
            if (value == null)
            {
                // TODO what to do?
                return false;
            }

            if (validationRules == null)
            {
                var ruleCollection = new RuleCollection<T>(value);
                validationRules = ConfigureRules(ruleCollection).GetRules();
                if (value is INotifyPropertyChanged target)
                {
                    target.PropertyChanged -= Target_PropertyChanged;
                    target.PropertyChanged += Target_PropertyChanged;
                }
            }
            return NewValidationService.ValidateRuleCollection(validationRules, value);
        }

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidationService.ValidateRuleCollection(validationRules!, sender);
        }

        // TODO convert to List<string>
        public override sealed string ErrorMessage => $"[{string.Join(",", ErrorsAsJsonString())}]";

        private IEnumerable<string?> ErrorsAsJsonString()
            => validationRules
                .Where(e => e.HasError)
                .Select(e => $"{e.PropertyName}:\"{e.ErrorMessage}\"");
    }
}
