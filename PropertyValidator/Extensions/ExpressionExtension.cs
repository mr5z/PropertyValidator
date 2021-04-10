using System;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertyValidator.Extensions
{
    public static class ExpressionExtension
    {
        public static string GetMemberName<T>(this Expression<T> expression)
        {
            return expression.Body switch
            {
                MemberExpression m => m.Member.Name,
                UnaryExpression u when u.Operand is MemberExpression m => m.Member.Name,
                _ => string.Empty
            };
        }

        public static PropertyInfo GetPropertyInfo<T>(this Expression<T> expression)
        {
            if (expression.Body is not MemberExpression body)
                throw new InvalidOperationException($"Expression must be a {nameof(MemberExpression)}");

            if (body.Member is not PropertyInfo propInfo)
                throw new InvalidOperationException($"Expression must be a property access");

            return propInfo;
        }
    }
}
