using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.ViewModels.Hospitals.Wizard;
using Monahrq.Sdk.Regions;
using Monahrq.Theme.Controls.Wizard.Models;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;
using System;

namespace Monahrq.DataSets.Views.Hospitals
{
   [Export(ViewNames.GeoContextWizard)]
    public partial class GeoContextWizard : UserControl,  INavigationAware
    {
        public IEventAggregator EventAggregator { get; set; }
        
        public GeoContextWizard()
        {
            InitializeComponent();
            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            var wizardSteps = new GeoContextStepCollection();
            DataContext = FactoryWizardViewModel(wizardSteps);
          
            //DataContextChanged += (o, e) =>
            //    {
            //        var wizard = e.OldValue as IWizardViewModel;
            //        if (wizard != null)
            //        {
            //            wizard.Detach();
            //        }
            //        wizard = e.NewValue as IWizardViewModel;
            //        if (wizard != null)
            //        {
            //            wizard.Attach();
            //        }
            //    };
        }

        IWizardViewModel Wizard
        {
            get
            {
                return DataContext as IWizardViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        private static object FactoryWizardViewModel(IStepCollection wizardSteps)
        {

            var wizardViewModelType = typeof(GeoWizardViewModel<>).MakeGenericType(wizardSteps.ContextType);
            var constructorInfo = wizardViewModelType.GetConstructor(new Type[0]);
            
            if (constructorInfo == null)
            {
                throw new NullReferenceException("Could not create Geo Wizard View Model");
            }
            else
            {
                var wizardViewModel = constructorInfo.Invoke(new object[0]);
                wizardViewModelType.GetMethod("ProvideSteps").Invoke(wizardViewModel, new object[] {wizardSteps});
                return wizardViewModel;
            }
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var x = navigationContext;
            this.BringIntoView();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var x = navigationContext;
        }
    }



}
