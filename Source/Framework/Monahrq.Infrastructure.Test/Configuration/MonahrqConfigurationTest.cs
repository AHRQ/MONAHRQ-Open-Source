using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Configuration;
using Monahrq.TestSupport.Configuration;

namespace Monahrq.Infrastructure.Test.Configuration
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class MonahrqConfigurationTest : MonahrqCleanConfigurationFixture
    {
        [TestMethod]
        public void SetConnectionStringPositive()
        {
            var settings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            Assert.IsTrue(string.IsNullOrEmpty(settings.EntityConnectionSettings.ConnectionString));
            var builder = new System.Data.OleDb.OleDbConnectionStringBuilder();
            builder.Provider = "foobar";
            settings.EntityConnectionSettings.ConnectionString = builder.ConnectionString;
            MonahrqConfiguration.Save(settings);
            var newSettings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            Assert.AreNotSame(settings, newSettings);
            Assert.AreEqual(builder.ConnectionString, newSettings.EntityConnectionSettings.ConnectionString); 
        }

        [TestMethod]
        public void AddNamedConnectionPositive()
        {
            var settings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            Assert.AreEqual(0, settings.NamedConnections.Count);
            var expected = new NamedConnectionElement();
            var builder = new System.Data.OleDb.OleDbConnectionStringBuilder();
            builder.Provider = "foobar";
            expected.ConnectionString = builder.ConnectionString;
            expected.Name = "SomeName";
            expected.SelectFrom = "Some Table";
            expected.ControllerType = typeof(object).AssemblyQualifiedName;
            settings.NamedConnections.Add(expected);
            MonahrqConfiguration.Save(settings);
            var newSettings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            Assert.AreEqual(1, newSettings.NamedConnections.Count);
            var target = newSettings.NamedConnections[expected.Name];
            Assert.AreEqual(expected.Name, target.Name);
            Assert.AreEqual(expected.ControllerType, target.ControllerType);
            Assert.AreEqual(expected.SelectFrom, target.SelectFrom);
            Assert.AreEqual(expected.ConnectionString, target.ConnectionString);
        }

        [TestMethod]
        public void TestDefaultTimeouts()
        {
            var settings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();

            const double ExpectedShortTimeout = 30d;
            const double ExpectedLongTimeout = 300d;

            Assert.AreEqual(ExpectedShortTimeout, settings.ShortTimeout.TotalSeconds);
            Assert.AreEqual(ExpectedLongTimeout, settings.LongTimeout.TotalSeconds);
        }

        [TestMethod]
        public void TestSetTimeouts()
        {
            var settings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();

            double ExpectedShortTimeout = settings.ShortTimeout.TotalSeconds + 1;
            double ExpectedLongTimeout = settings.LongTimeout.TotalSeconds + 1;

            settings.ShortTimeout = TimeSpan.FromSeconds(ExpectedShortTimeout);
            settings.LongTimeout = TimeSpan.FromSeconds(ExpectedLongTimeout);
            MonahrqConfiguration.Save(settings);

            var settingsFromDisk = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            Assert.AreEqual(ExpectedShortTimeout, settingsFromDisk.ShortTimeout.TotalSeconds);
            Assert.AreEqual(ExpectedLongTimeout, settingsFromDisk.LongTimeout.TotalSeconds);
        }

        [TestMethod]
        public void TestChangeLastFolderPositive()
        {
            // Set the last folder as a string that's not a folder
            var settings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            string expected = "THIS IS NOT A FOLDER";
            settings.LastFolder = expected;
            MonahrqConfiguration.Save(settings);

            var settingsFromDisk = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            Assert.AreEqual(expected, settingsFromDisk.LastFolder);

            // TODO: do we need better tests for this item?
        }
    }
}
