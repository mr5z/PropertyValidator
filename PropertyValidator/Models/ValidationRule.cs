using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyValidator.Models
{
    public abstract class ValidationRule<T> : IValidationRule
    {
        public string PropertyName { get; set; }

        public bool HasError { get; private set; }

        public bool Validate(object value)
        {
            return HasError = !IsValid((T)value);
        }

        public abstract bool IsValid(T value);

        public abstract string ErrorMessage { get; }

        public string ErrorMessageOverride { get; set; }
    }
}
