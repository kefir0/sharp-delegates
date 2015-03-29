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

            // Raw
            Profile(() => func()); // warmup
            var rawTime = Profile(() => func());
            
            // Converted
            var wrappedFunc = DelegateConverter.GetFunc(func, typeof(DateTime));
            var convertedTime = Profile(() => wrappedFunc());
            
            // Converted and cast
            var dlgate = DelegateConverter.CreateDelegate(func, typeof(DateTime));
            var castFunc = (Func<DateTime>)dlgate;
            var castTime = Profile(() => castFunc());
            
            // Dynamic
            var dyn = (dynamic) dlgate;
            var dynamicTime = Profile(() => dyn.Invoke());

            // Expression tree
            var exprTree = DelegateConverter.GetFuncUsingExprTree(func);
            var exprTreeTime = Profile(() => exprTree());

            Console.WriteLine(rawTime + " " + convertedTime + " " + castTime + " " + dynamicTime + " " + exprTreeTime);
            Console.WriteLine("Converted: " + (double)convertedTime/rawTime);
            Console.WriteLine("Cast: " + (double)castTime/rawTime);
            Console.WriteLine("Dynamic: " + (double)dynamicTime/rawTime);
            Console.WriteLine("Expr Tree: " + (double)exprTreeTime/rawTime);
        }

        private static long Profile(Action action)
        {
            const int testSize = 100000;

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < testSize; i++)
                action();

            return sw.ElapsedTicks;
        }
    }
}
