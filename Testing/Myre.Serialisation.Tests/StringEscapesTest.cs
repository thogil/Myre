﻿using Myre.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Myre.Serialisation.Tests
{
    
    
    /// <summary>
    ///This is a test class for StringEscapesTest and is intended
    ///to contain all StringEscapesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StringEscapesTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Escape
        ///</summary>
        [TestMethod()]
        public void EscapeTest()
        {
            string value = "\"\tHello\\World!\n\r\"";
            string expected = "\\\"" + @"\tHello\World!\n\r" + "\\\"";
            string actual = value.Escape();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Unescape
        ///</summary>
        [TestMethod()]
        public void UnescapeTest()
        {
            string value = @"\tHello\\World!\n\r";
            string expected = "\tHello\\World!\n\r";
            string actual = value.Unescape();
            Assert.AreEqual(expected, actual);
        }
    }
}
