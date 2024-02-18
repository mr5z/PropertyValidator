using System;
using System.Reflection;

namespace PropertyValidator.Helpers;

internal static class ReflectionHelper
{
    internal static FieldInfo? GetField(Type? fromType, string fieldName)
    {
        const BindingFlags flags = BindingFlags.Instance |
                                   BindingFlags.NonPublic |
                                   BindingFlags.DeclaredOnly;
        FieldInfo? field;
        // TODO find a way to break
        while ((field = fromType?.GetField(fieldName, flags)) == null && (fromType = fromType?.BaseType) != null)
        {
            // intentionally empty body
        }
        return field;
    }
}
