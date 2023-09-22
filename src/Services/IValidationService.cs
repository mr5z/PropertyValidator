using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
        IRuleCollection<T> For<TNotifiableModel>(
            TNotifiableModel notifiableModel,
            TimeSpan? delay = null)
            where TNotifiableModel : INotifyPropertyChanged, INotifiableModel;

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
        /// Retrieves most recent errors after validating them
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string?> GetErrors();

        /// <summary>
        /// Subscribe to error events (cleared/raised)
        /// </summary>
        event EventHandler<ValidationResultArgs>? PropertyInvalid;
    }
}
