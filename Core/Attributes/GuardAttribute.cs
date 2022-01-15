using System;
using System.Linq.Expressions;
using System.Reflection;
using Core.Extensions;
using Core.Logic;
using Core.Models;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class GuardAttribute : Attribute
    {
        public Type Type { get; }
        public string Field { get; }
        public SubsetExpressionType ExpressionType { get; }
        public object Value { get; }
        public Func<object, bool> Validator { get; }

        public GuardAttribute(Type type, SubsetExpressionType expressionType, object value)
        {
            if (!value.GetType().IsAssignableFrom(type))
            {
                throw new ArgumentException("Provided value does not match the type.", nameof(value));
            }

            Type = type;
            ExpressionType = expressionType;
            Field = null;
            Value = value;

            var paramExpr = Expression.Parameter(typeof(object));
            var typedParamExpr = Expression.Convert(paramExpr, type);
            var bodyExpr = Expression.And(Expression.TypeIs(paramExpr, type),
                Expression.MakeBinary(expressionType.AsExpressionType(), typedParamExpr, Expression.Constant(value)));
            var lambdaExpr = Expression.Lambda<Func<object, bool>>(bodyExpr, paramExpr);
            Validator = lambdaExpr.Compile();
        }

        public GuardAttribute(Type type, string field, SubsetExpressionType expressionType, object value)
        {
            var property = type.GetProperty(field, BindingFlags.Instance | BindingFlags.Public);
            if (property == null)
            {
                throw new ArgumentException("Property does not exist in the type", nameof(field));
            }

            if (!value.GetType().IsAssignableFrom(property.PropertyType))
            {
                throw new ArgumentException("Provided value does not match the property type.", nameof(value));
            }

            Type = type;
            Field = field;
            ExpressionType = expressionType;
            Value = value;

            var paramExpr = Expression.Parameter(typeof(object));
            var typedParamExpr = Expression.Convert(paramExpr, type);
            var accessExpr = Expression.PropertyOrField(typedParamExpr, field);
            var bodyExpr = Expression.And(Expression.TypeIs(paramExpr, type),
                Expression.MakeBinary(expressionType.AsExpressionType(), accessExpr, Expression.Constant(value)));
            var lambdaExpr = Expression.Lambda<Func<object, bool>>(bodyExpr, paramExpr);
            Validator = lambdaExpr.Compile();
        }

        public bool IsRelatedTo(GuardAttribute guard)
        {
            return Field == guard.Field;
        }

        /// <summary>
        /// Checks whether A is a subset of B
        /// </summary>
        public bool IsSubsetOf(GuardAttribute guard)
        {
            var expressionAnalyzer = new ExpressionAnalyzer();
            
            // ReSharper disable once InvertIf
            if (Field == guard.Field)
            {
                if (Type == guard.Type && ExpressionAnalyzer.IsSubsetOf(ExpressionType, Value, guard.ExpressionType, guard.Value))
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return Field == null ? $"{Type.Name} ({ExpressionType}) {Value}" : $"{Type.Name}.{Field} ({ExpressionType}) {Value}";
        }
    }
}