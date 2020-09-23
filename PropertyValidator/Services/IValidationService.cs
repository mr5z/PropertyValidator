using PropertyValidator.Extensions;
using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace PropertyValidator.Services
{
    public interface IValidationService
    {
        RuleCollection<TNotifiableModel> For<TNotifiableModel>(TNotifiableModel notifiableModel) where TNotifiableModel : INotifyPropertyChanged;
        string GetErrorMessage<TNotifiableModel>(TNotifiableModel notifiableModel, Expression<Func<TNotifiableModel, object>> expression) where TNotifiableModel : INotifyPropertyChanged;
        bool Validate();
        event EventHandler<ValidationResultArgs> PropertyInvalid;
    }
}
