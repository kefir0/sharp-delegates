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
            Func<object> funcObj = () => func();

            for (int i = 0; i < 3; i++)
            {
                // Raw
                var rawTime = Profile(() => funcObj());

                // Converted
                var wrappedFunc = DelegateConverter.GetFunc(func, typeof (DateTime));
                var convertedTime = Profile(() => wrappedFunc());

                // Dynamic
                var dyn = (dynamic) DelegateConverter.CreateDelegate(func, typeof (DateTime));
                var dynamicTime = Profile(() => dyn.Invoke());

                // Expression tree
                var exprTree = DelegateConverter.GetFuncUsingExprTree(func);
                var exprTreeTime = Profile(() => exprTree());

                if (i > 1)
                {
                    Console.WriteLine(rawTime + " " + convertedTime + " " + +dynamicTime + " " + exprTreeTime);
                    WriteStats(convertedTime, rawTime, "Converted");
                    WriteStats(dynamicTime, rawTime, "Dynamic");
                    WriteStats(exprTreeTime, rawTime, "Expr Tree");
                }
            }
        }

        [TestMethod]
        public void TestGetFuncPerfNoCache()
        {
            Func<DateTime> func = () => DateTime.Now;
            Func<object> func0 = () => func();
            object funcObj = func0;

            for (int i = 0; i < 3; i++)
            {
                DelegateConverter.ClearCache();

                // Raw
                var rawTime = Profile(() => ((Func<object>) funcObj)());

                // Converted
                var convertedTime = Profile(() => DelegateConverter.GetFunc(func, typeof(DateTime))());

                // Dynamic
                var dynamicTime =
                    Profile(() => ((dynamic)DelegateConverter.CreateDelegate(func, typeof(DateTime))).Invoke());

                // Dynamic
                var dynamicTime2 = Profile(() => ((dynamic) funcObj).Invoke());

                // Expression tree
                var exprTreeTime = Profile(() => DelegateConverter.GetFuncUsingExprTreeCached(func.GetType())(func));

                if (i > 1)
                {
                    Console.WriteLine(rawTime + " " + convertedTime + " " + +dynamicTime + " " + exprTreeTime);
                    WriteStats(convertedTime, rawTime, "Converted");
                    WriteStats(dynamicTime, rawTime, "Dynamic");
                    WriteStats(dynamicTime2, rawTime, "Dynamic Direct");
                    WriteStats(exprTreeTime, rawTime, "Expr Tree");
                }
            }
        }

        private static void WriteStats(long resultTicks, long referenceTicks, string name)
        {
            Console.WriteLine("{0}: {1} times slower; Overhead: {2} ms", name, (double) resultTicks/referenceTicks, (double)(resultTicks - referenceTicks)/TestSize/TimeSpan.TicksPerMillisecond);
        }

        const int TestSize = 50000;

        private static long Profile(Action action)
        {

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < TestSize; i++)
                action();

            return sw.ElapsedTicks;
        }
    }
}
