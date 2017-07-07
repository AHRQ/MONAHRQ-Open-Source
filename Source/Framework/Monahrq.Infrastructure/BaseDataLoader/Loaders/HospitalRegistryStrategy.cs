using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using NHibernate.Linq;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="HospitalRegistry"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataCustomImporter{Monahrq.Infrastructure.Entities.Domain.Hospitals.HospitalRegistry, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class HospitalRegistryStrategy : BaseDataCustomImporter<HospitalRegistry, int>
    {
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 1; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(HospitalRegistry));
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        public override void LoadData()
        {
            try
            {
                using (var session = DataProvider.SessionFactory.OpenSession())
                {
                    // Add a new registry if it's not loaded yet.
                    if (!session.Query<HospitalRegistry>().Any())
                    {
                        decimal version = 1;
                        HospitalRegistry registry = new HospitalRegistry(version)
                        {
                            Name = GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description
                        };
                        session.SaveOrUpdate(registry);
                    }

                    // Called because this wasn't in the schema version table yet, so add it so we don't check again.
                    var sversion = VersionStrategy.GetVersion(session);

                    session.SaveOrUpdate(sversion);
                    session.Flush();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), Category.Warn, Priority.Medium);
            }
        }
    }
}
