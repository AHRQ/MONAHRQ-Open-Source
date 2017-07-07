using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Services;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Sdk.Extensibility;
using Monahrq.Sdk.Modules.Settings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Theme.Controls.Wizard.Models.Data;
using PropertyChanged;

namespace Monahrq.DataSets.Context
{
    [ImplementPropertyChanged]
    public class GeographicContext : ModelBase
    {
        public string Title { get; set; }
        public IStepCollection<GeographicContext> Steps { get; set; }

        public IMonahrqShell ShellContext { get; private set; }
        public IHospitalDataService HospitalDataService { get; set; }
        public IHospitalRegistryService HospitalRegistryService { get; private set; }

        public event EventHandler<CancelEventArgs> Finishing = delegate { };
        public event EventHandler Finished = delegate { };

        
        private HospitalMappingReference _regionMappingReference { get; set; }
        public HospitalMappingReference RegionMappingReference 
        {
            get
            {
                return _regionMappingReference;
            }
            set
            {
                _regionMappingReference = value;
            }
        }

        public GeographicContext()
        {
            HospitalDataService = ServiceLocator.Current.GetInstance<IHospitalDataService>();
            HospitalRegistryService = ServiceLocator.Current.GetInstance<IHospitalRegistryService>();
           // RegionMappingReference = DefaultReferenceMapping();
            ContextStates = new List<State>();

        }

        public override bool Cancel()
        {
           
            var args = new CancelEventArgs(true);
            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<WizardCancellingEvent>()
                .Publish(args);
            var isCancelled = args.Cancel;
            if (isCancelled)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>()
                    .GetEvent<WizardCancelEvent>()
                    .Publish(EventArgs.Empty);
                NotifyDeleteEntry();
            }

            return isCancelled;
        }


        public IEnumerable<State> ContextStates { get; set; }
        public Type ContextRegionType { get; set; }
              
        

        public void NotifyDeleteEntry()
        {
            //if (CurrentContentItem == null || CurrentContentItem.Id <= 0) return;
            //ServiceLocator.Current.GetInstance<IEventAggregator>()
            //.GetEvent<DeleteEntryEvent>().Publish(CurrentContentItem);
            //CurrentContentItem = null;
        }

        public void NotifyUpdateEntry()
        {
            //if (CurrentContentItem == null || CurrentContentItem.Id <= 0) return;
            //ServiceLocator.Current.GetInstance<IEventAggregator>()
            //.GetEvent<UpdateEntryEvent>().Publish(CurrentContentItem);
        }
        public override void Dispose()
        {
            
        }

        public override string GetName()
        {
            return "Geographic Context";
        }

        public void Finish()
        {
            var canceller = new CancelEventArgs(false);
            OnFinishing(canceller);
            if (canceller.Cancel) return;

         //   var item = CurrentContentItem as ContentItemRecord;
         //   if (item != null)
         //   {
              //  ApplySummaryRecord();
           //     item.IsFinished = true;
                //SaveImportEntry(item);
          //  }
            OnFinished(EventArgs.Empty);
        }

        protected virtual void OnFinished(EventArgs eventArgs)
        {
            Finished(this, eventArgs);
        }

        protected virtual void OnFinishing(CancelEventArgs canceller)
        {
            Finishing(this, canceller);
        }

        public void LoadData()
        {
            HospitalDataService.CreateMappingContext(ContextStates, ContextRegionType);
        }


        public HospitalMappingReference DefaultReferenceMapping()
        {
            var abbrvs = HospitalRegion.Default.DefaultStates.OfType<string>().ToArray();
            var states = abbrvs.Count() == 0  
                ? Enumerable.Empty<State>()
                : HospitalRegistryService.GetStates(abbrvs);
            return HospitalRegion.Default.SelectedRegionType == typeof(HealthReferralRegion)
                ? HospitalRegistryService.GenerateMappingReference<HealthReferralRegion>(states)
                : HospitalRegistryService.GenerateMappingReference<HospitalServiceArea>(states);
        }
    }
}
