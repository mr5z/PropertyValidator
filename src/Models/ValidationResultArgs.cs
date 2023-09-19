using System;
using System.Collections.Generic;
using System.Linq;

namespace PropertyValidator.Models
{
    public class ValidationResultArgs : EventArgs
    {
        public ValidationResultArgs(string? propertyName, IDictionary<string, IEnumerable<string?>> errorDictionary)
        {
            PropertyName = propertyName;
            ErrorDictionary = errorDictionary;
        }

        public string? PropertyName { get; }

        public IDictionary<string, IEnumerable<string?>> ErrorDictionary { get; }

        public IEnumerable<string?>? ErrorMessages => ErrorDictionary
            .Values
            .FirstOrDefault(errors => errors.Any(message => !string.IsNullOrEmpty(message)));

        public bool HasError => ErrorMessages?.Any() == true;

        public string? FirstError => ErrorMessages?
            .FirstOrDefault();
    }
}
