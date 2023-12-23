using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PropertyValidator.Services;

public interface IValidationService
{
    /// <summary>
    /// Registers the model for validation.
    /// </summary>
    /// <typeparam name="TNotifiableModel"></typeparam>
    /// <param name="notifiableModel">The notifiable model</param>
    /// <param name="autofill">If true, it will automatically fills the error properties</param>
    /// <param name="delay">Delays the fill up of error properties</param>
    /// <returns>RuleCollection so that it can be chained.</returns>
    IRuleCollection<TNotifiableModel> For<TNotifiableModel>(
        TNotifiableModel notifiableModel,
        bool autofill = true,
        TimeSpan? delay = null)
        where TNotifiableModel : INotifyPropertyChanged, INotifiableModel;

    /// <summary>
    /// Ensure all properties are in a valid state based from the provided validation rules.
    /// </summary>
    /// <throws>PropertyException if there is an error</throws>
    void EnsurePropertiesAreValid();

    /// <summary>
    /// Trigger manually the validation.
    /// </summary>
    /// <returns>True if validation is successful, false otherwise.</returns>
    bool Validate();

    /// <summary>
    /// Retrieves most recent errors after validating them.
    /// </summary>
    /// <returns>Dictionary where key is the property name and value are aggregated error messages.</returns>
    IDictionary<string, string?> GetErrors();

    /// <summary>
    /// Set error aggregation format. By default, it's using <see cref="string.Join(string, IEnumerable{string})"/> with ", " as separator.
    /// </summary>
    /// <param name="errorFormatter">Callback for formatting error messages.</param>
    void SetErrorFormatter(Func<IEnumerable<string?>, string> errorFormatter);

    /// <summary>
    /// Subscribe to error events (cleared/raised).
    /// </summary>
    event EventHandler<ValidationResultArgs>? PropertyInvalid;
}
