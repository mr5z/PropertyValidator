using PropertyValidator.Models;
using System;

namespace PropertyValidator.Exceptions;

/// <summary>
/// Exception thrown when some property violated a certain <see cref="IValidationRule"/>.
/// </summary>
/// <param name="validationResultArgs">Result obtained after validation.</param>
public class PropertyException(ValidationResultArgs validationResultArgs) : Exception(validationResultArgs.FirstError)
{
    public ValidationResultArgs ValidationResultArgs { get; } = validationResultArgs;
}