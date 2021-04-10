using PropertyValidator.Models;
using System;

namespace PropertyValidator.Exceptions
{
    public class PropertyException : Exception
    {
        public PropertyException(ValidationResultArgs validationResultArgs) : base(validationResultArgs.FirstError)
        {

        }
    }
}
