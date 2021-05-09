using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PropertyValidator.Models
{
    public class ValidationResultArgs : EventArgs
    {
        private const string KnownErrorPropertyPattern = "Error";

        public ValidationResultArgs(string? propertyName, IDictionary<string, IEnumerable<string?>> errorDictionary)
        {
            PropertyName = propertyName;
            ErrorDictionary = errorDictionary;
        }

        public string? PropertyName { get; }

        public IDictionary<string, IEnumerable<string?>> ErrorDictionary { get; }

        public IEnumerable<string?> ErrorMessages => ErrorDictionary
            .Values
            .FirstOrDefault(errors => errors.Any(message => !string.IsNullOrEmpty(message)));

        public string? FirstError => ErrorMessages
            .FirstOrDefault();

        public void FillErrorProperty<T>(T notifiableModel) where T : INotifyPropertyChanged
        {
            if (PropertyName == null)
            {
                foreach (var entry in ErrorDictionary)
                {
                    FillError(notifiableModel, entry.Key, entry.Value.FirstOrDefault());
                }
            }
            else
            {
                FillError(notifiableModel, PropertyName, FirstError);
            }
        }

        private void FillError<T>(T notifiableModel, string propertyName, string? errorMessage) where T : INotifyPropertyChanged
        {
            var errorProperty = propertyName + KnownErrorPropertyPattern;
            var propInfo = notifiableModel.GetType().GetProperty(errorProperty);
            if (propInfo == null)
                throw new TargetException($"Property '{errorProperty}' not found in target '{notifiableModel.GetType().Name}'");
            propInfo.SetValue(notifiableModel, errorMessage);
        }
    }
}
