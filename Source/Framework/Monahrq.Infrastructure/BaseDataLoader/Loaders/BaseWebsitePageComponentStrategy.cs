using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Extensions;
using System.ComponentModel.Composition;
using System;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
	[Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class BaseWebsitePageComponentStrategy : BaseDataDataReaderImporter<BaseWebsitePageComponent, int>
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
        protected override string Fileprefix { get { return "BaseWebsitePageComponent"; } }
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(BaseWebsitePageComponent));
        }
        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override BaseWebsitePageComponent LoadFromReader(System.Data.IDataReader dr)
        {
			var item = new BaseWebsitePageComponent()
			{
				Name = dr.Guard<string>("Name").UnQuote(),
				WebsitePageName = dr.Guard<string>("WebsitePageName").UnQuote(),
				CodePath = dr.Guard<string>("CodePath").UnQuote(),
				ComponentType = dr.Guard<string>("ComponentType").UnQuote().GetEnum<WebsitePageComponentTypeEnum>(),
				Audiences = dr.Guard<string>("Audiences").UnQuote().Split(',').ToList().ConvertAll(s => s.GetEnum<Entities.Domain.Reports.Audience>())
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
