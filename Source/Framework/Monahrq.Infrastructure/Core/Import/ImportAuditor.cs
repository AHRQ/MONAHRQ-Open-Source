using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Utility;
using NHibernate.Linq;

namespace Monahrq.Infrastructure.Core.Import
{
    /// <summary>
    /// The ccustom datset import auditor class.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.Import.IImportAuditor" />
    [Export(typeof(IImportAuditor))]
    [Obsolete("The ImportAuditor has been deprecated as of Monahrq version 6.0 Build 2.")]
    public class ImportAuditor : IImportAuditor
    {
        /// <summary>
        /// Gets or sets the factory provider.
        /// </summary>
        /// <value>
        /// The factory provider.
        /// </value>
        [Import]
        public IDomainSessionFactoryProvider FactoryProvider { get; set; }
        /// <summary>
        /// Wases the executed.
        /// </summary>
        /// <param name="dataReaders">The data readers.</param>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        public bool WasExecuted(IDataReaderDictionary dataReaders, Expression<Func<object>> expr)
        {
            var attr = dataReaders.VersionAttribute;
            var version = attr.Version;
            var contract = attr.ContractName;
            if (version == 0) return true;
            var prop = expr.StaticProperty();
            using (var session = FactoryProvider.SessionFactory.OpenStatelessSession())
            {
                var audited = (from adt in session.Query<Audit>()
                               where adt.Dataset == prop.Name && 
                                     adt.DataVersion == version && 
                                     adt.Contract == contract
                               select adt.Id).Any();

                if (!audited)
                {
                    //using (var trans = session.BeginTransaction())
                    //{
                        var audit = new Audit
                            {
                                DataVersion = version,
                                Contract = contract,
                                Dataset = prop.Name
                            };

                        session.Insert(audit);
                        //trans.Commit();
                    //}

                }
                return audited;
            }
        }
    }
}