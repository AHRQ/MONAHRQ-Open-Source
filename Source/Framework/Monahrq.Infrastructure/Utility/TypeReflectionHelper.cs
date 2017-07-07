using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Utility
{
    public static class TypeReflectionHelper
    {
        public static MemberInfo GetMember<TModel, TReturn>(Expression<Func<TModel, TReturn>> expression)
        {
            return GetMember(expression.Body);
        }

        private static bool IsIndexedPropertyAccess(Expression expression)
        {
            return IsMethodExpression(expression) && expression.ToString().Contains("get_Item");
        }

        private static bool IsMethodExpression(Expression expression)
        {
            return expression is MethodCallExpression || (expression is UnaryExpression && IsMethodExpression((expression as UnaryExpression).Operand));
        }

        private static MemberInfo GetMember(Expression expression)
        {
            var expr = GetLambdaExpression(expression);
            dynamic body = expr.Body;
            dynamic member = body.Member;
            return member as PropertyInfo;
        }

  
        private static LambdaExpression GetLambdaExpression(Expression expression)
        {
            return GetLambdaExpression(expression, true);
        }

        private static LambdaExpression GetLambdaExpression(Expression expression, bool enforceCheck)
        {
            LambdaExpression lambdaExpression = null;
            if (expression.NodeType == ExpressionType.Lambda)
            {
                lambdaExpression = expression as LambdaExpression;
            }
            if (enforceCheck && lambdaExpression == null)
            {
                throw new ArgumentException("Not a static member", "expression");
            }

            return lambdaExpression;
        }

        public static TReturn StaticTarget<TReturn>(Expression<Func<object>> expr)
        {
            var prop = StaticProperty(expr);
            return (TReturn) Convert.ChangeType(prop.GetValue(null), typeof(TReturn)) ;
        }

        static MemberInfo ToMember<TReturn>(this Expression<Func<TReturn>> propertyExpression)
        {
            return TypeReflectionHelper.GetMember(propertyExpression);
        }


        public static PropertyInfo StaticProperty(this Expression<Func<object>> expr)
        {
            return expr.ToMember() as PropertyInfo;
        }

    }

   
}
