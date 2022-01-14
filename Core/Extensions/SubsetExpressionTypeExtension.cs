using System;
using System.Linq.Expressions;
using Core.Models;

namespace Core.Extensions
{
    internal static class SubsetExpressionTypeExtension
    {
        public static ExpressionType AsExpressionType(this SubsetExpressionType expressionType)
        {
            return expressionType switch
            {
                SubsetExpressionType.Equal => ExpressionType.Equal,
                _ => throw new ArgumentOutOfRangeException(nameof(expressionType), expressionType, null)
            };
        }
    }
}