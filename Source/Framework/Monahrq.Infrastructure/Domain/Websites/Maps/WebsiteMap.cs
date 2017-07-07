using System.Collections.Generic;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Sdk.Utilities;
using NHibernate.Mapping;
using NHibernate.Type;
using Monahrq.Infrastructure.Utility.Extensions;
using System;

namespace Monahrq.Infrastructure.Domain.Websites.Maps
{
	public class WebsiteTableNames
	{
		public static string WebsiteMeasuresTable = "Websites_" + Inflector.Pluralize(typeof(WebsiteMeasure).Name);
		public static string WebsiteDatasetsTable = "Websites_" + Inflector.Pluralize(typeof(WebsiteDataset).Name);
		public static string WebsiteReportsTable = "Websites_" + Inflector.Pluralize(typeof(WebsiteReport).Name);
		public static string WebsiteHospitalsTable = "Websites_" + Inflector.Pluralize(typeof(WebsiteHospital).Name);
		public static string WebsiteNursingHomesTable = "Websites_" + Inflector.Pluralize(typeof(WebsiteNursingHome).Name);
		public static string WebsiteThemesTable = "Websites_" + Inflector.Pluralize(typeof(WebsiteTheme).Name);
		public static string WebsiteMenusTable = "Websites_" + Inflector.Pluralize(typeof(WebsiteMenu).Name);
	}

	[MappingProviderExport]
	public class WebsiteMap : EntityMap<Website, int, IdentityGeneratedKeyStrategy>
	{
		public WebsiteMap()
		{
			Map(x => x.CurrentStatus);
			Map(x => x.ReportedYear);
			Map(x => x.Description).Length(2000);
			//Map(x => x.Audience).CustomType<EnumStringType<Audience>>();
			Map(x => x.DefaultAudience).CustomType<EnumStringType<Audience>>();
			//.Not.Nullable().Default(Website.WebsiteDefaultAudience.NotSelected.GetEnumDescription());
			Map(x => x.Audiences).CustomType<EnumListToStringType<Audience>>();
			//.Not.Nullable();
			Map(x => x.ReportedQuarter);

			Map(x => x.StateContext)
				.CustomType<StringListToStringType>();

			Map(x => x.RegionTypeContext)
				.Length(150);

			Map(x => x.HospitalsChangedWarning)
				.Nullable().Default("0");

			Map(x => x.UtilizationReportCompression)
				.Nullable().Default("0");
             
            Map(x => x.PublishIframeVersion)
                .Nullable().Default("0");

            Map(x => x.IncludeGuideToolInYourWebsite)
                .Default("0").Not.Nullable();

            #region Metadata
            Map(x => x.BrowserTitle).Length(100);
			Map(x => x.Keywords).Length(1000);
			Map(x => x.AboutUsSectionSummary).Length(1000);
			Map(x => x.AboutUsSectionText).CustomSqlType("nvarchar(max)");

			Map(Reveal.Member<Website>("_selectedReportingStatesXml"), "SelectedReportingStatesXml")
				.CustomSqlType("xml")
				.CustomType<XmlDocType>();
			//.Not.Nullable();

			Map(Reveal.Member<Website>("_selectedZipCodeRadiiXml"), "SelectedZipCodeRadiiXml")
				.CustomSqlType("xml")
				.CustomType<XmlDocType>();
			//.Not.Nullable();

			Map(Reveal.Member<Website>("_feedbackTopicsXml"), "FeedbackTopicsXml")
				.CustomSqlType("xml")
				.CustomType<XmlDocType>();

			Map(x => x.GoogleAnalyticsKey).Length(500);
			Map(x => x.GoogleMapsApiKey).Length(500);
			Map(x => x.GeographicDescription).Length(256);
			Map(x => x.OutPutDirectory).Length(500);
			#endregion

			#region FeedbackUrl
			Map(x => x.IsStandardFeedbackForm).Default("0").Not.Nullable();
			Map(x => x.CustomFeedbackFormUrl).Nullable();
			Map(x => x.IncludeFeedbackFormInYourWebsite).Default("0").Not.Nullable();
			Map(x => x.FeedBackEmail).Length(255).Nullable();
			#endregion

			#region Theme Mapping

			Map(x => x.HeaderTitle).Length(100);

			HasMany(x => x.Themes)
				.KeyColumn("Website_Id")
				.AsBag().NotFound.Ignore()
				.Not.Inverse()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Website_Themes")
				.NonStrictReadWrite();

			Component(x => x.LogoImage, i =>
			{
				i.Map(x => x.ImagePath, "LogoImagePath").Length(1000);
				i.Map(x => x.MemeType, "LogoImageMemeType").Length(20);
				i.Map(x => x.Image, "LogoImage").CustomType<BinaryBlobType>();
			});

			Component(x => x.BannerImage, i =>
			{
				i.Map(x => x.ImagePath, "BannerImagePath").Length(1000);
				i.Map(x => x.MemeType, "BannerImageMemeType").Length(20);
				i.Map(x => x.Image, "BannerImage").CustomType<BinaryBlobType>();
				i.Map(x => x.Name, "BannerName").Length(50);
			});

			Component(x => x.HomepageContentImage, i =>
			{
				i.Map(x => x.ImagePath, "HomepageContentImagePath").Length(1000);
				i.Map(x => x.MemeType, "HomepageContentImageMemeType").Length(20);
				i.Map(x => x.Image, "HomepageContentImage").CustomType<BinaryBlobType>();
			});
			#endregion

			HasMany(x => x.Reports)
				.KeyColumn("Website_Id")
				//.Not.KeyNullable()
				//.AsList(x => x.Column("[Index]"))
				.AsBag()
				.NotFound.Ignore()
				.Not.Inverse()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Website_Reports").NonStrictReadWrite();

			HasMany(x => x.Datasets)
				.KeyColumn("Website_Id")
				//.Not.KeyNullable()
				//.AsList(x => x.Column("[Index]"))
				.AsBag()
				.NotFound.Ignore()
				.Not.Inverse()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Website_Datasets").NonStrictReadWrite();

			HasMany(x => x.Measures)
				.KeyColumn("Website_Id")
				//.Not.KeyNullable()
				//.AsList(x => x.Column("[Index]"))
				.AsBag()
				.Not.Inverse()
				.NotFound.Ignore()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Website_Measures").NonStrictReadWrite();

			HasMany(x => x.WebsitePages)
				.KeyColumn("Website_Id")
				.AsBag()
				.Not.Inverse()
				.NotFound.Ignore()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Website_WebsitePages").NonStrictReadWrite();


		//	HasMany(x => x.WebsitePages)
		//		.Inverse()
		//		.Not.LazyLoad()
		//		.Cascade.All();

			HasMany(x => x.ActivityLog)
				.KeyColumn("Website_Id")
				//.Not.KeyNullable()
				//.AsList(x => x.Column("[Index]"))
				.AsBag()
				.Not.Inverse()
				.Cascade.AllDeleteOrphan()
				.NotFound.Ignore()
				.Not.LazyLoad()
				.Cache.Region("Website_Activities").NonStrictReadWrite();

			HasMany(x => x.Hospitals)
				.KeyColumn("Website_Id")
				//.Not.KeyNullable()
				//.AsList(x => x.Column("[Index]"))
				.AsBag()
				.Not.Inverse()
				.Cascade.AllDeleteOrphan()
				.NotFound.Ignore()
				.Not.LazyLoad()
				.Cache.Region("Website_Hospitals").NonStrictReadWrite();

			HasMany(x => x.NursingHomes)
				.KeyColumn("Website_Id")
				//.Not.KeyNullable()
				//.AsList(x => x.Column("[Index]"))
				.AsBag()
				.Not.Inverse()
				.NotFound.Ignore()
				.Cascade.All()
				.Not.LazyLoad()
				.Cache.Region("Website_NursingHomes").NonStrictReadWrite();

			HasMany(x => x.Menus)
				.KeyColumn("Website_Id")
				.AsBag()
				.Not.Inverse()
				.NotFound.Ignore()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Website_Menus").NonStrictReadWrite();

			Cache.NonStrictReadWrite().Region("Websites");
		}
	}

	[MappingProviderExport]
	public class WebsiteHospitalMap : ClassMap<WebsiteHospital>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteReportMap"/> class.
		/// </summary>
		public WebsiteHospitalMap()
		{
			Table(WebsiteTableNames.WebsiteHospitalsTable);
			SelectBeforeUpdate();
			Id(o => o.Id).GeneratedBy.Identity();

			//References(x => x.Website, "Website_Id").ForeignKey("fk_WebsiteMeasures_Websites").Not.Nullable();
			References(x => x.Hospital, "Hospital_Id")
							 .Cascade.SaveUpdate()
							 .Not.Nullable()
							 .Not.LazyLoad();

			Map(x => x.CCR);
		}
	}

	[MappingProviderExport]
	public class WebsiteMeasureMap : ClassMap<WebsiteMeasure>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteMeasureMap"/> class.
		/// </summary>
		public WebsiteMeasureMap()
		{
			Table(WebsiteTableNames.WebsiteMeasuresTable);
			SelectBeforeUpdate();
			Id(o => o.Id).GeneratedBy.Identity();

			//References(x => x.Website, "Website_Id").ForeignKey("fk_WebsiteMeasures_Websites").Not.Nullable();
			References(x => x.OriginalMeasure, "OriginalMeasure_Id")
							 .ForeignKey("fk_WebsiteMeasures_Measures")
							 .Not.Nullable()
							 .Cascade.SaveUpdate()
							 .Not.LazyLoad();

			References(x => x.OverrideMeasure, "OverrideMeasure_Id")
							 .ForeignKey("fk_WebsiteMeasures_Measures_Override")
							 .Cascade.SaveUpdate()
							 .Not.LazyLoad();

			Map(x => x.IsSelected);

			//Map(x => x.Index, "[Index]").Not.Nullable().Default("0"); 

			//Cache.NonStrictReadWrite().Region("WebsiteMeasures");
		}
	}

	[MappingProviderExport]
	public class WebsiteReportMap : ClassMap<WebsiteReport>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteReportMap"/> class.
		/// </summary>
		public WebsiteReportMap()
		{
			Table(WebsiteTableNames.WebsiteReportsTable);
			SelectBeforeUpdate();
			Id(o => o.Id).GeneratedBy.Identity();

			// References(x => x.Website, "Website_Id").ForeignKey("fk_WebsiteMeasures_Websites").Not.Nullable();
			References(x => x.Report, "Report_Id")
							 .Cascade.None()
							 .NotFound.Ignore()
							 .Not.Nullable()
							 .Not.LazyLoad();

            Map(x => x.IsQuarterlyTrendingEnabled)
                .Default("1")
                .Not.Nullable();

			Map(x => x.SelectedYears)
				.CustomType<JsonToStringType<List<TrendingYear>>>()
				.Nullable();

			Map(x => x.DefaultSelectedYear)
				.CustomSqlType("nvarchar(max)");

		    Map(x => x.AssociatedWebsites)
		        .Not.Update()
		        .Not.Insert()
		        .Formula("( select STUFF((SELECT ', ' + w.[Name] FROM [dbo].[Websites] w left join [dbo].[Websites_WebsiteReports] wr on wr.[Website_Id] = w.[Id] Where wr.[Report_Id] = Report_Id FOR XML PATH('')),1,1,'') )");

		    //Map(x => x.Index, "[Index]").Not.Nullable().Default("0"); 

		    //Cache.NonStrictReadWrite().Region("WebsiteReports");
		}


	}

	[MappingProviderExport]
	public class WebsiteDatasetMap : ClassMap<WebsiteDataset>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteDatasetMap"/> class.
		/// </summary>
		public WebsiteDatasetMap()
		{
			Table(WebsiteTableNames.WebsiteDatasetsTable);
			SelectBeforeUpdate();
			Id(o => o.Id).GeneratedBy.Identity();

			References(x => x.Dataset, "Dataset_Id")
				.Not.LazyLoad()
				.Cascade.None()
				.Not.Nullable();


			//Map(x => x.Index, "[Index]").Not.Nullable().Default("0"); 

			//Cache.NonStrictReadWrite().Region("WebsiteDatasets");
		}
	}

	[MappingProviderExport]
	public class WebsiteNursingHomeMap : ClassMap<WebsiteNursingHome>
	{
		public WebsiteNursingHomeMap()
		{
			Table(WebsiteTableNames.WebsiteNursingHomesTable);
			SelectBeforeUpdate();
			Id(o => o.Id).GeneratedBy.Identity();

			References(x => x.NursingHome, "NursingHome_ProviderId")
				.PropertyRef("ProviderId")
							 .Cascade.SaveUpdate()
							 .Not.Nullable()
							 .Not.LazyLoad();
		}
	}

	[MappingProviderExport]
	public class WebsiteThemeMap : ClassMap<WebsiteTheme>
	{
		public WebsiteThemeMap()
		{
			Table(WebsiteTableNames.WebsiteThemesTable);
			Id(x => x.Id).GeneratedBy.Identity();
			Map(x => x.SelectedTheme).Length(100);
			Map(x => x.BrandColor).Length(50);
			Map(x => x.Brand2Color).Length(50);
			Map(x => x.AccentColor).Length(50);
			Map(x => x.SelectedFont).Length(100);
			Map(x => x.BackgroundColor).Length(50);
			Map(x => x.BodyTextColor).Length(50);
			Map(x => x.LinkTextColor).Length(50);
			Map(x => x.AudienceType)
				.CustomType<EnumStringType<Audience>>();
		}
	}

	[MappingProviderExport]
	public class WebsiteMenuMap : ClassMap<WebsiteMenu>
	{
		public WebsiteMenuMap()
		{
			Table(WebsiteTableNames.WebsiteMenusTable);
			SelectBeforeUpdate();

			Id(x => x.Id).GeneratedBy.Identity();

			Map(x => x.Menu)
				.CustomType<JsonToStringType<Menu>>()
				.Nullable();
		}
	}
}
