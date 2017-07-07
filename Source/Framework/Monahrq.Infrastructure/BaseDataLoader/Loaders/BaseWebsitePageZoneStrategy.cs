using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Extensions;
using System.ComponentModel.Composition;
using System;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="BaseWebsitePageZone"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePageZone, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class BaseWebsitePageZoneStrategy : BaseDataDataReaderImporter<BaseWebsitePageZone, int>
    {
		protected override BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Replace; } }
		///// <summary>
		///// Gets the loader priority.
		///// </summary>
		///// <value>
		///// The loader priority.
		///// </value>
		public override int LoaderPriority { get { return 5; } }
        /// <summary>
        /// Gets the Fileprefix.
        /// </summary>
        /// <value>
        /// The Fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "BaseWebsitePageZone"; } }
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(BaseWebsitePageZone));
        }
        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override BaseWebsitePageZone LoadFromReader(System.Data.IDataReader dr)
        {
			var item = new BaseWebsitePageZone()
			{
				Name = dr.Guard<string>("Name").UnQuote(),
				WebsitePageName = dr.Guard<string>("WebsitePageName").UnQuote(),
				CodePath = dr.Guard<string>("CodePath").UnQuote(),
				ZoneType = dr.Guard<string>("ZoneType").UnQuote().GetEnum<WebsitePageZoneTypeEnum>(),
				Audience = dr.Guard<string>("Audience").UnQuote().GetEnum<Entities.Domain.Reports.Audience>(),
			};


			// NOTE: Must save and flush data here or categories are not being saved!
			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				try
				{
					session.SaveOrUpdate(item);
					session.Flush();
				}
				catch (Exception exc)
				{
					base.Logger.Log(exc.GetBaseException().Message, Microsoft.Practices.Prism.Logging.Category.Exception, Microsoft.Practices.Prism.Logging.Priority.High);
				}
			}
			return null;	// item;
        }
    }
}
