using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace PropertyValidator.Models
{
    public interface IRuleCollection<TModel>
    {
        IRuleCollection<TModel> AddRule<TProperty>
            (Expression<Func<TModel, TProperty>> expression,
            params ValidationRule<TProperty>[] rules);

        IRuleCollection<TModel> AddRule<TProperty>(
            [NotNull]
            string propertyName,
            params ValidationRule<TProperty>[] rules);

        IRuleCollection<TModel> AddRule<TProperty>(
            Expression<Func<TModel, TProperty>> expression,
            string? errorMessageOverride,
            params ValidationRule<TProperty>[] rules);

        IRuleCollection<TModel> AddRule<TProperty>(
            [NotNull]
            string propertyName,
            string? errorMessageOverride,
            params ValidationRule<TProperty>[] rules);

        IReadOnlyDictionary<string, IEnumerable<IValidationRule>> GetRules();
    }
}
