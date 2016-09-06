﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class TestInputTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestInput_Constructor_WhenNullVariable_ShouldThrowException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new TestInput(null, "someValue");
            //------------Assert Results-------------------------
        } 

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_Constructor_WhenValidParameters_ShouldSetProperties()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var input = new TestInput("someVar", "someValue");
            //------------Assert Results-------------------------
            Assert.AreEqual("someVar",input.Variable);
            Assert.AreEqual("someValue",input.Value);
            Assert.IsTrue(input.EmptyIsNull);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_Variable_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new TestInput("someVar", "someValue");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
              {
                  if (args.PropertyName == "Variable")
                  {
                      _wasCalled = true;
                  }
              };
            //------------Execute Test---------------------------
            input.Variable = "var";
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_Value_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new TestInput("someVar", "someValue");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Value")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Value = "val";
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_EmptyIsNull_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new TestInput("someVar", "someValue");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "EmptyIsNull")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.EmptyIsNull = false;
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }
        
    }
}