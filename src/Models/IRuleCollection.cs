using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace PropertyValidator.Models;

public interface IRuleCollection<TModel>
{
    IRuleCollection<TModel> AddRule<TProperty>(
        Expression<Func<TModel, TProperty>> expression,
        params IValidationRule[] rules);

    IRuleCollection<TModel> AddRule<TProperty>(
        [NotNull]
        string propertyName,
        params IValidationRule[] rules);

    IRuleCollection<TModel> AddRule<TProperty>(
        Expression<Func<TModel, TProperty>> expression,
        string? errorMessageOverride,
        params IValidationRule[] rules);

    IRuleCollection<TModel> AddRule<TProperty>(
        [NotNull]
        string propertyName,
        string? errorMessageOverride,
        params IValidationRule[] rules);

    IReadOnlyDictionary<string, IEnumerable<IValidationRule>> GetRules();
}
