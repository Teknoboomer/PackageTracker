using NUnit.Framework;
using System;

namespace DAL_Unit_Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Console.WriteLine("Test1");
            Assert.Pass();
        }
    }
}