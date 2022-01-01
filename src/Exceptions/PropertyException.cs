using PropertyValidator.Models;
using System;

namespace PropertyValidator.Exceptions
{
    public class PropertyException : Exception
    {
        public ValidationResultArgs ValidationResultArgs { get; }

        public PropertyException(ValidationResultArgs validationResultArgs) : base(validationResultArgs.FirstError)
        {
            ValidationResultArgs = validationResultArgs;
        }
    }
}
