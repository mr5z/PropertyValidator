using PropertyValidator.Models;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PropertyValidator.Services
{
    public interface IValidationService
    {
        // For registration
        RuleCollection<TNotifiableModel> For<TNotifiableModel>(
            TNotifiableModel notifiableModel,
            bool autofill = false,
            TimeSpan? delay = null)
            where TNotifiableModel : INotifyPropertyChanged;

        // Retrieve error messages per property
        string GetErrorMessage<TNotifiableModel>(
            TNotifiableModel notifiableModel,
            Expression<Func<TNotifiableModel, object>> expression)
            where TNotifiableModel : INotifyPropertyChanged;

        // Manually trigger the validation
        bool Validate();

        // Subscribe to error events (cleared/raised)
        event EventHandler<ValidationResultArgs> PropertyInvalid;
    }
}
