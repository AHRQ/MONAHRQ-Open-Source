using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.TestSupport.Configuration
{
    public class MonahrqCleanConfigurationFixture
    {
        MonahrqConfigurationSection InitialConfig { get; set; }
        
        [TestInitialize]
        public void InitializeConfig()
        {
            InitialConfig = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            MonahrqConfiguration.Save(new MonahrqConfigurationSection());
        }

        [TestCleanup]
        public void ResetConfig()
        {
            MonahrqConfiguration.Save(InitialConfig);
        }

    }
}
