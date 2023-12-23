using System;
using System.Collections.Generic;
using System.Linq;

namespace PropertyValidator.Models;

public class ValidationResultArgs(IDictionary<string, IEnumerable<string?>> errorDictionary) : EventArgs
{
    public IDictionary<string, IEnumerable<string?>> ErrorDictionary { get; } = errorDictionary;

    public IEnumerable<string?>? ErrorMessages => ErrorDictionary
        .Values
        .FirstOrDefault(errors => errors.Any(message => !string.IsNullOrEmpty(message)));

    public bool HasError => ErrorMessages?.Any() == true;

    public string? FirstError => ErrorMessages?
        .FirstOrDefault();
}
