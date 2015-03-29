using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace SharpDelegates
{
    public class DelegateConverter
    {
        // http://elegantcode.com/2010/07/30/createdelegatet-an-exercise-in-using-expressions/
        // TODO: PredicateBuilder??

        private const string DefaultMethodName = "Invoke";

        public static Func<object> GetFunc(object target, Type resultType, string methodName = null)
        {
            var dg = CreateDelegate(target, resultType, methodName);

            return () => dg.DynamicInvoke();
        }

        public static Func<object, object> GetFunc(object target, Type argType, Type resultType,
            string methodName = null)
        {
            methodName = methodName ?? DefaultMethodName;

            var delegateType = typeof(Func<,>).MakeGenericType(argType, resultType);

            var dg = Delegate.CreateDelegate(delegateType, target, methodName);

            return x => dg.DynamicInvoke(x);
        }


        public static Delegate CreateDelegate(object target, Type resultType, string methodName = null)
        {
            methodName = methodName ?? DefaultMethodName;

            var delegateType = typeof (Func<>).MakeGenericType(resultType);

            var dg = Delegate.CreateDelegate(delegateType, target, methodName);
            return dg;
        }

        public static Func<object> GetFuncUsingExprTree0(object target, string methodName = null)
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

        public static Func<object> GetFuncUsingExprTree(object target, string methodName = null)
        {
            var targetType = target.GetType();
            var method = targetType.GetMethod(methodName ?? DefaultMethodName);
            var callExpr = Expression.Call(Expression.Constant(target), method);
            var convertResultExpr = Expression.Convert(callExpr, typeof (object));
            var lambda = Expression.Lambda<Func<object>>(convertResultExpr);
            return lambda.Compile();
        }

        public static Func<object, object> GetFuncUsingExprTreeCached(Type targetType)
        {
            return FuncCache.GetOrAdd(targetType, CreateFuncNoParams);
        }

        public static Func<object, object> CreateFuncNoParams(Type targetType)
        {
            var method = targetType.GetMethod(DefaultMethodName);
            var targetParam = Expression.Parameter(typeof (object), "targetFunc");
            var targetParamConverted = Expression.Convert(targetParam, targetType);
            var callExpr = Expression.Call(targetParamConverted, method);
            var convertResultExpr = Expression.Convert(callExpr, typeof (object));

            var lambda = Expression.Lambda<Func<object, object>>(convertResultExpr, targetParam);
            return lambda.Compile();
        }

        public static void ClearCache()
        {
            FuncCache.Clear();
        }

        private static readonly ConcurrentDictionary<Type, Func<object, object>> FuncCache = new ConcurrentDictionary<Type, Func<object, object>>();
    }
}
