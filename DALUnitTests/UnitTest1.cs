using NUnit.Framework;
using System;

namespace DALUnitTests
{
    public class DAL_Unit_Tests
    {
        [SetUp]
        public void Initialize()
        {
            Console.WriteLine("Inside SetUp");
        }

        [TearDown]
        public void DeInitialize()
        {
            Console.WriteLine("Inside TearDown");
        }

        public class TestClass1
        {
            [OneTimeSetUp]
            public static void ClassInitialize()
            {
                Console.WriteLine("Inside OneTimeSetUp");
            }

            [OneTimeTearDown]
            public static void ClassCleanup()
            {
                Console.WriteLine("Inside OneTimeTearDown");
            }
        }

        [Test, Order(1)]
        public void Test_1()
        {
            Console.WriteLine("Inside TestMethod Test_1");
        }

        [Test, Order(2)]
        public void Test_2()
        {
            Console.WriteLine("Inside TestMethod Test_2");
        }
    }
}