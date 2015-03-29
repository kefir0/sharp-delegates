using System;
using System.Linq.Expressions;

namespace SharpDelegates
{
    public class DelegateConverter
    {
        // http://elegantcode.com/2010/07/30/createdelegatet-an-exercise-in-using-expressions/

        private const string DefaultMethodName = "Invoke";

        public static Func<object> GetFunc(object target, Type resultType, string methodName = null)
        {
            var dg = CreateDelegate(target, resultType, methodName);

            return () => dg.DynamicInvoke();
        }

        public static Delegate CreateDelegate(object target, Type resultType, string methodName = null)
        {
            methodName = methodName ?? DefaultMethodName;

            var delegateType = typeof (Func<>).MakeGenericType(resultType);

            var dg = Delegate.CreateDelegate(delegateType, target, methodName);
            return dg;
        }

        public static Func<object> GetFuncUsingExprTree(object target, string methodName = null)
        {
            Expression<Func<object>> instExpr = () => target;
            var targetType = target.GetType();
            var convertedInstanceExpression = Expression.Convert(instExpr.Body, targetType);
            var method = targetType.GetMethod(methodName ?? DefaultMethodName);
            var callExpr = Expression.Call(convertedInstanceExpression, method);
            var convertResultExpr = Expression.Convert(callExpr, typeof (object));
            var lambda = Expression.Lambda<Func<object>>(convertResultExpr);
            return lambda.Compile();
        }
    }
}
