using System;
using Core.Models;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DeclarationAttribute : GuardAttribute
    {
        public DeclarationAttribute(Type type, SubsetExpressionType expressionType, object value) : base(type, expressionType, value)
        {
        }

        public DeclarationAttribute(Type type, string field, SubsetExpressionType expressionType, object value) : base(type, field, expressionType, value)
        {
        }
    }
}