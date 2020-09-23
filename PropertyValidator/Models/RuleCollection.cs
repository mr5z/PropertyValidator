using PropertyValidator.Extensions;
using PropertyValidator.Helpers;
using PropertyValidator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertyValidator.Models
{
    public class RuleCollection<TModel>
    {
        private readonly List<IValidationRule> validationRuleList = new List<IValidationRule>();
        private readonly object owner;

        public event EventHandler<RuleAddedEventArgs> RuleAdded;

        public RuleCollection(object owner)
        {
            this.owner = owner;
        }

        private void OwnerProperty_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var eventName = nameof(INotifyPropertyChanged.PropertyChanged);
            var field = ReflectionHelper.GetField(owner.GetType(), eventName);
            var eventDelegate = field.GetValue(owner) as MulticastDelegate;
            var propInfo = owner.GetType().GetProperties().FirstOrDefault(e => e.GetValue(owner) == sender);
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[] { owner, new PropertyChangedEventArgs(propInfo.Name) });
                }
            }
        }

        public RuleCollection<TModel> AddRule<TProperty>(Expression<Func<TModel, TProperty>> expression, params ValidationRule<TProperty>[] rules)
        {
            return AddRule(expression, null, rules);
        }

        public RuleCollection<TModel> AddRule<TProperty>(string propertyName, params ValidationRule<TProperty>[] rules)
        {
            return AddRule(propertyName, null, rules);
        }

        public RuleCollection<TModel> AddRule<TProperty>(Expression<Func<TModel, TProperty>> expression, string errorMessage, params ValidationRule<TProperty>[] rules)
        {
            var propertyName = expression.GetMemberName();
            var body = expression.Body as MemberExpression;
            var result = AddRule(propertyName, errorMessage, rules);
            var propInfo = body.Member as PropertyInfo;
            var value = propInfo.GetValue(owner, null);
            if (value is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= OwnerProperty_PropertyChanged;
                notifyPropertyChanged.PropertyChanged += OwnerProperty_PropertyChanged;
            }
            RuleAdded?.Invoke(this, new RuleAddedEventArgs());
            return result;
        }

        public RuleCollection<TModel> AddRule<TProperty>(string propertyName, string errorMessage, params ValidationRule<TProperty>[] rules)
        {
            foreach (var rule in rules)
            {
                rule.PropertyName = propertyName;
                rule.ErrorMessageOverride = errorMessage;
                validationRuleList.Add(rule);
            }
            return this;
        }

        public List<IValidationRule> GetRules()
        {
            return validationRuleList;
        }
    }
}
