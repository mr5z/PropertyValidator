using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PropertyValidator.Models
{
    public abstract class ValidationRule<T> : IValidationRule
    {
        public string PropertyName { get; set; }

        public bool HasError { get; private set; }

        public bool Validate(object value) => (HasError = !IsValid((T)value));

        // TODO support async validations
        //public virtual Task<bool> IsValidAsync(T value) => Task.FromResult(IsValid(value));

        public abstract bool IsValid(T value);

        public abstract string ErrorMessage { get; }

        public string ErrorMessageOverride { get; set; }
    }
}
