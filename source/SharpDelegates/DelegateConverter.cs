using System;

namespace SharpDelegates
{
    public class DelegateConverter
    {
        /// <summary>
        /// Gets the Func delegate that calls specified method on specified target object.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="resultType">Type of the method result.</param>
        /// <param name="methodName">Name of the method; defaults to "Invoke".</param>
        /// <returns>Func delegate that calls specified method on specified target object.</returns>
        public static Func<object> GetFunc(object target, Type resultType, string methodName = null)
        {
            methodName = methodName ?? "Invoke";

            var delegateType = typeof(Func<>).MakeGenericType(resultType);

            var dg = Delegate.CreateDelegate(delegateType, target, methodName);

            return () => dg.DynamicInvoke();
        }

    }
}
