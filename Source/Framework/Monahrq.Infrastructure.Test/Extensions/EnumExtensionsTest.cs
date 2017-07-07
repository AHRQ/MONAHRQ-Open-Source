namespace Monahrq.Infrastructure.Test.EnumExtensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monahrq.Infrastructure.Entities.Domain.Reports;
    using Monahrq.Infrastructure.Extensions;            // this needs to be INSIDE the namespace

    [TestClass]
    public class EnumExtensionsTest
    {
        [System.ComponentModel.Description("enum for testing only")]
        public enum MyEnum
        {
            NoDescription,
            [System.ComponentModel.Description("WithDescription")]
            WithDescription
        }

        [TestMethod]
        public void EnumDescriptionTest()
        {
            // test one of our existing enums without a default description
            var actual = EnumExtensions.GetEnumFieldDescription(Audience.None);
            Assert.AreEqual("Select Audience", actual);

            actual = EnumExtensions.GetEnumFieldDescription(Audience.Consumers);
            Assert.AreEqual("Consumers", actual);

            // test a local enum with and without descriptions
            actual = EnumExtensions.GetEnumFieldDescription(MyEnum.NoDescription, "");
            Assert.AreEqual("", actual);

            actual = EnumExtensions.GetEnumFieldDescription(MyEnum.NoDescription, "default string");
            Assert.AreEqual("default string", actual);

            actual = EnumExtensions.GetEnumFieldDescription(MyEnum.WithDescription, "Wrong Description");
            Assert.AreNotEqual("Wrong Description", actual);
        }
    }
}
