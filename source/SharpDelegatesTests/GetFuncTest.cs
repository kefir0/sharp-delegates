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

            const int testSize = 100000;

            // Raw
            var sw = Stopwatch.StartNew();

            for (var i = 0; i < testSize; i++)
                Assert.IsNotNull(func());

            var rawTime = sw.ElapsedTicks;

            var wrappedFunc = DelegateConverter.GetFunc(func, typeof (DateTime));


            // Converted
            sw = Stopwatch.StartNew();

            for (var i = 0; i < testSize; i++)
                Assert.IsNotNull(wrappedFunc());

            var convertedTime = sw.ElapsedTicks;

            Console.WriteLine(rawTime);
            Console.WriteLine(convertedTime);
            Console.WriteLine(convertedTime/rawTime);
        }
    }
}
