using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Entities.Domain.Reports;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="BaseWebsitePage"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePage, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class BaseWebsitePageStrategy : BaseDataDataReaderImporter<BaseWebsitePage, int>
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
        protected override string Fileprefix { get { return "BaseWebsitePage"; } }
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(BaseWebsitePage));
        }
        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override BaseWebsitePage LoadFromReader(System.Data.IDataReader dr)
        {
			var item = new BaseWebsitePage
			{
				Name = dr.Guard<string>("Name").UnQuote(),
				TemplateRelativePath = dr.Guard<string>("TemplateRelativePath").UnQuote(),
				PublishRelativePath = dr.Guard<string>("PublishRelativePath").UnQuote(),
				PageType = dr.Guard<string>("PageType").UnQuote().GetEnum<WebsitePageTypeEnum>(),
				ReportName = dr.Guard<string>("ReportName").UnQuote(),
				Audience = dr.Guard<string>("Audience").UnQuote().GetEnum<Audience>(),
				Url = dr.Guard<string>("Url").UnQuote(),
				IsEditable = dr.Guard<bool>("IsEditable"),
			};
			return item;
        }
    }
}
