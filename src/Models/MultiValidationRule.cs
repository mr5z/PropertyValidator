﻿using ObservableProperty.Services;
using PropertyValidator.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PropertyValidator.Models
{
    public abstract class MultiValidationRule<T> : ValidationRule<T> where T : notnull
    {
        private IReadOnlyDictionary<string, IEnumerable<IValidationRule>>? validationRules;

        private readonly IDictionary<string, string?> lastErroredProperty = new Dictionary<string, string?>();

        protected abstract IRuleCollection<T> ConfigureRules(IRuleCollection<T> ruleCollection);

        public sealed override bool IsValid(T? value)
        {
            if (value == null)
            {
                // TODO what to do?
                return false;
            }

            if (this.validationRules == null)
            {
                var actionCollection = new ActionCollection<T>();
                var ruleCollection = new RuleCollection<T>(actionCollection, value);
                this.validationRules = ConfigureRules(ruleCollection).GetRules();

                if (value is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged -= Target_PropertyChanged;
                    inpc.PropertyChanged += Target_PropertyChanged;
                }
            }

            return ValidationService.ValidateRuleCollection(
                GetValidationRules(),
                value
            );
        }

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dictionaryRules = GetValidationRules();
            var listRules = dictionaryRules.Values.SelectMany(it => it);
            var resultArgs = ValidationService.GetValidationResultArgs(e.PropertyName, null, listRules!);

            this.lastErroredProperty[e.PropertyName] = resultArgs.HasError ? e.PropertyName : null;
        }

        private IDictionary<string, IEnumerable<IValidationRule>> GetValidationRules()
        {
            return this.validationRules.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // TODO convert to List<string>
        public sealed override string ErrorMessage => $"{{{string.Join(", ", ErrorsAsJsonString())}}}";

        private IEnumerable<string?> ErrorsAsJsonString()
        {
            return this.validationRules
                .SelectMany(it => it.Value)
                .Select(e => $"\"{e.PropertyName}\":\"{e.Error}\"");
        }
    }
}
