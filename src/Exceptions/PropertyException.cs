using PropertyValidator.Models;
using System;

namespace PropertyValidator.Exceptions;

public class PropertyException(ValidationResultArgs validationResultArgs) : Exception(validationResultArgs.FirstError)
{
    public ValidationResultArgs ValidationResultArgs { get; } = validationResultArgs;
}