using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Infrastructure.Test.Utility.Extensions
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass]
    public class EnumerationExtensionsTests
    {
        [TestMethod]
        public void NonFlagComponentPositive()
        {
            NonFlag sut = NonFlag.Val1;
            var target = sut.Components().ToList();
            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.Contains(NonFlag.Val1));
        }


        [TestMethod]
        public void IsFlagComponentPositive2()
        {
            IsFlag sut = IsFlag.Val1 | IsFlag.Val3;
            var target = sut.Components().ToList();
            Assert.AreEqual(2, target.Count);
            Assert.IsTrue(target.Contains(IsFlag.Val1));
            Assert.IsTrue(target.Contains(IsFlag.Val3));
        }


        [TestMethod]
        public void IsFlagComponentPositive1()
        {
            IsFlag sut = IsFlag.Val1 ;
            var target = sut.Components().ToList();
            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.Contains(IsFlag.Val1)); 
        }
    }


    public enum NonFlag
    {
        [System.ComponentModel.Description(constants.Val1)]
        Val1 = 0x01,
        Val2 = 0x02,
    }

    [Flags]
    public enum IsFlag
    {
        [System.ComponentModel.Description(constants.Val1)]
        Val1 = 0x01,
        Val2 = 0x02,
        [System.ComponentModel.Description(constants.Val3)]
        Val3 = 0x04, 
    }

    class constants
    {
        public const string Val1 = "Value 1";
        public const string Val3 = "Value 3";
    }
}
