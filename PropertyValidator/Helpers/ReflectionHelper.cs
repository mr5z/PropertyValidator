using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PropertyValidator.Helpers
{
    public static class ReflectionHelper
    {
        public static FieldInfo GetField(Type fromType, string fieldName)
        {
            var flags = BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly;
            FieldInfo field;
            while ((field = fromType.GetField(fieldName, flags)) == null && (fromType = fromType.BaseType) != null)
                ;
            return field;
        }
    }
}
