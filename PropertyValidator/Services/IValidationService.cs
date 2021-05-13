using PropertyValidator.Models;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PropertyValidator.Services
{
    public interface IValidationService
    {
        /// <summary>
        /// Registers the model for validation
        /// </summary>
        /// <typeparam name="TNotifiableModel"></typeparam>
        /// <param name="notifiableModel">The notifiable model</param>
        /// <param name="autofill">If true, it will automatically fills the error properties</param>
        /// <param name="delay">Delays the fill up of error properties</param>
        /// <returns>RuleCollection so that it can be chained</returns>
        IRuleCollection<TNotifiableModel> For<TNotifiableModel>(
            TNotifiableModel notifiableModel,
            bool autofill = false,
            TimeSpan? delay = null)
            where TNotifiableModel : INotifyPropertyChanged;

        /// <summary>
        /// Retrieve error messages per property
        /// </summary>
        /// <typeparam name="TNotifiableModel"></typeparam>
        /// <param name="_">Unused</param>
        /// <param name="expression">Expression to extract the property name from the target class</param>
        /// <returns>The first error message</returns>
        string? GetErrorMessage<TNotifiableModel>(
            TNotifiableModel _,
            Expression<Func<TNotifiableModel, object?>> expression)
            where TNotifiableModel : INotifyPropertyChanged;

        /// <summary>
        /// Retrieve error messages per property
        /// </summary>
        /// <typeparam name="TNotifiableModel"></typeparam>
        /// <param name="propertyName">The name of the property the exist within the registered notifiable model</param>
        /// <returns>The first error message</returns>
        string? GetErrorMessage<TNotifiableModel>(string propertyName);

        /// <summary>
        /// Ensure all properties are in a valid state based from the provided validation rules
        /// </summary>
        /// <throws>PropertyException if there is an error</throws>
        void EnsurePropertiesAreValid();

        /// <summary>
        /// Trigger manually the validation
        /// </summary>
        /// <returns></returns>
        bool Validate();

        /// <summary>
        /// Subscribe to error events (cleared/raised)
        /// </summary>
        event EventHandler<ValidationResultArgs>? PropertyInvalid;
    }
}
