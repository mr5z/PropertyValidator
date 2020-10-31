using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PropertyValidator.Models
{
    public class ValidationResultArgs : EventArgs
    {
        private const string KnownErrorPropertyPattern = "Error";

        public ValidationResultArgs(string propertyName, IDictionary<string, IEnumerable<string>> errorDictionary)
        {
            PropertyName = propertyName;
            ErrorDictionary = errorDictionary;
        }

        public string PropertyName { get; }

        public IEnumerable<string> ErrorMessages => ErrorDictionary.Values.FirstOrDefault();

        public IDictionary<string, IEnumerable<string>> ErrorDictionary { get; }

        public string FirstError => ErrorDictionary.Values.FirstOrDefault()?.FirstOrDefault();

        public void FillErrorProperty<T>(T notifiableModel) where T : INotifyPropertyChanged
        {
            var errorProperty = PropertyName + KnownErrorPropertyPattern;
            var propInfo = notifiableModel.GetType().GetProperty(errorProperty);
            if (propInfo == null)
                throw new TargetException($"{errorProperty} is not found for target '{notifiableModel.GetType().Name}'");
            propInfo.SetValue(notifiableModel, FirstError);
        }
    }
}
