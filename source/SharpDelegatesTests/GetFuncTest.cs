using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDelegates;

namespace SharpDelegatesTests
{
    [TestClass]
    public class GetFuncTest
    {
        [TestMethod]
        public void TestGetFunc()
        {
            Func<DateTime> func = () => DateTime.Now;

            var wrappedFunc = DelegateConverter.GetFunc(func, typeof (DateTime));

            Assert.AreEqual(func().Date, ((DateTime)wrappedFunc()).Date);
        }
        
        [TestMethod]
        public void TestGetFuncPerf()
        {
            Func<DateTime> func = () => DateTime.Now;
            Func<object> funcObj = () => (object)(func());

                // Raw
            Profile(() => func()); // warmup
            var rawTime = Profile(() => funcObj());
            
            // Converted
            var wrappedFunc = DelegateConverter.GetFunc(func, typeof(DateTime));
            var convertedTime = Profile(() => wrappedFunc());
            
            // Dynamic
            var dyn = (dynamic)DelegateConverter.CreateDelegate(func, typeof(DateTime));
            var dynamicTime = Profile(() => dyn.Invoke());

            // Expression tree
            var exprTree = DelegateConverter.GetFuncUsingExprTree(func);
            var exprTreeTime = Profile(() => exprTree());

            Console.WriteLine(rawTime + " " + convertedTime + " " + + dynamicTime + " " + exprTreeTime);
            Console.WriteLine("Converted: " + (double)convertedTime/rawTime);
            Console.WriteLine("Dynamic: " + (double)dynamicTime/rawTime);
            Console.WriteLine("Expr Tree: " + (double)exprTreeTime/rawTime);
        }

        [TestMethod]
        public void TestGetFuncPerfNoCache()
        {
            Func<DateTime> func = () => DateTime.Now;

            // Raw
            Profile(() => func()); // warmup
            var rawTime = Profile(() => ((Func<object>)(() => (object)(func())))());

            // Converted
            var convertedTime = Profile(() => DelegateConverter.GetFunc(func, typeof(DateTime))());

            // Dynamic
            var dynamicTime = Profile(() => ((dynamic)DelegateConverter.CreateDelegate(func, typeof(DateTime))).Invoke());

            // Expression tree
            var exprTreeTime = Profile(() => DelegateConverter.GetFuncUsingExprTreeCached(func.GetType())(func));

            Console.WriteLine(rawTime + " " + convertedTime + " " + +dynamicTime + " " + exprTreeTime);
            Console.WriteLine("Converted: " + (double)convertedTime / rawTime);
            Console.WriteLine("Dynamic: " + (double)dynamicTime / rawTime);
            Console.WriteLine("Expr Tree: " + (double)exprTreeTime / rawTime);
        }



        private static long Profile(Action action)
        {
            const int testSize = 200000;

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < testSize; i++)
                action();

            return sw.ElapsedTicks;
        }
    }
}
