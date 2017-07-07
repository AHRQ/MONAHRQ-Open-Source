using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Context;
using Monahrq.DataSets.Views.Hospitals;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels.Hospitals.Wizard
{
    [Export]
    [ImplementPropertyChanged]
    public class SelectDataSourceStep : WizardStepViewModelBase<GeographicContext>
    {
        private GeographicContext Context { get; set; }
        public SelectDataSourceStep(GeographicContext context)
            : base(context)
        {
            Context = context;

            ServiceLocator.Current.GetInstance<IEventAggregator>()
              .GetEvent<WizardBackEvent>().Subscribe(evnt =>
                  {
                      var x = evnt;
                  });
        }

        private bool _isUseDefaultData;
        public bool IsUseDefaultData
        {
            get { return _isUseDefaultData; }
            set
            {
                _isUseDefaultData = value;
                RaisePropertyChanged(() => IsUseDefaultData);
            }
        }

        private bool _isIsUseImportData;
        public bool IsUseImportData
        {
            get { return _isIsUseImportData; }
            set
            {
                _isIsUseImportData = value;
                RaisePropertyChanged(() => IsUseImportData);
            }
        }

        private bool _isImportHospitalData;
        public bool IsImportHospitalData
        {
            get { return _isImportHospitalData; }
            set
            {
                _isImportHospitalData = value;
            RaisePropertyChanged(()=>IsImportHospitalData);
            }
        }


        private bool _isImportRegionsData;
        public bool IsImportRegionsData
        {
            get { return _isImportRegionsData; }
            set { _isImportRegionsData = value;
            RaisePropertyChanged(()=>IsImportRegionsData);
            }
        }

        public override string DisplayName
        {
            get { return "Select Data Source"; }
        }

        public override bool IsValid()
        {
           if (IsUseDefaultData) return true;
           return IsUseImportData & (IsImportHospitalData || IsImportRegionsData);
        }

        public override RouteModifier OnNext()
        {
            if(IsUseDefaultData)
            {
                return new RouteModifier { ExcludeViewTypes = new List<Type> { typeof(ImportRegionsDataView), typeof(ImportHospitalDataView), } };
             }


            if (IsImportHospitalData && IsImportRegionsData)
            {
                return new RouteModifier
                {
                    IncludeViewTypes = new List<Type> { typeof(ImportRegionsDataView), typeof(ImportHospitalDataView) }
                };
            }
          

            if (!IsImportHospitalData)
            {
                return new RouteModifier { ExcludeViewTypes = new List<Type> { typeof(ImportHospitalDataView) },
                    IncludeViewTypes = new List<Type> { typeof(ImportRegionsDataView) } };
            }

            if (!IsImportRegionsData)
            {
                return new RouteModifier { ExcludeViewTypes = new List<Type> { typeof(ImportRegionsDataView) },
                                           IncludeViewTypes = new List<Type> { typeof(ImportHospitalDataView) }
                };
            }

            return null;

        }
    }
}