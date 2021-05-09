using System;
using System.Reflection;

namespace PropertyValidator.Helpers
{
    static class ReflectionHelper
    {
        public static FieldInfo? GetField(Type fromType, string fieldName)
        {
            var flags = BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly;
            FieldInfo field;
            // TODO find a way to break
            while ((field = fromType.GetField(fieldName, flags)) == null && (fromType = fromType.BaseType) != null)
                ;
            return field;
        }
    }
}
