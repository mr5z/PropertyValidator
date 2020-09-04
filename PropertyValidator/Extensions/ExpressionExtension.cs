using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

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
    }
}
