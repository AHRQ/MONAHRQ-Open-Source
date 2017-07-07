using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Domain.BaseData.ViewModel;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Websites.Services;

namespace Monahrq.Websites.ViewModels
{
    public interface IWebsiteViewModel : IPartImportsSatisfiedNotification
    {
        IBaseDataService BaseDataService { get; set; }
        IRegionManager RegionManager { get; set; }
        IWebsiteDataService WebsiteDataService { get; set; }
        ObservableCollection<EntityViewModel<ReportingQuarter, int>> Quarters { get; set; }
    }
}
