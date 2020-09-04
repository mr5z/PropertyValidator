using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyValidator.Models
{
    public class ValidationResultArgs : EventArgs
    {
        public ValidationResultArgs(string propertyName, IEnumerable<string> errorMessages)
        {
            PropertyName = propertyName;
            ErrorMessages = errorMessages;
        }

        public string PropertyName { get; }

        public IEnumerable<string> ErrorMessages { get; }

        public string FirstError => ErrorMessages?.FirstOrDefault();
    }
}
