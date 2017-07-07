using System.Collections.Generic;
using Monahrq.DataSets.Context;
using Monahrq.DataSets.Views.Hospitals;
using Monahrq.Theme.Controls.Wizard.Models;

namespace Monahrq.DataSets.ViewModels.Hospitals.Wizard
{
   public class GeoContextStepCollection:StepCollection<GeographicContext>
    {
       public GeoContextStepCollection(): base()
        {
            Context.Steps = this;
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            ApplySteps();
// ReSharper restore DoNotCallOverridableMethodsInConstructor
        }


       protected virtual void ApplySteps()
       {

           var list = new List<CompleteStep<GeographicContext>>()
                {
                    new CompleteStep<GeographicContext>()
                        {
                            ViewModel = new SelectStatesStep(Context),
                            ViewType = typeof (SelectStatesView),        // select states
                            Visited = true
                        }
                };

           var list2 = new List<CompleteStep<GeographicContext>>
                {
                    new CompleteStep<GeographicContext>()
                        {
                            ViewModel =  new SelectDataSourceStep(Context),
                            ViewType = typeof (SelectDataSourceView)   
                        },
                        
                    new CompleteStep<GeographicContext>()
                        {
                            ViewModel = new ImportRegionsDataStep(Context),
                            ViewType = typeof (ImportRegionsDataView)          // import regions (optional)
                        },
                      new CompleteStep<GeographicContext>()
                        {
                            ViewModel = new ImportHospitalsDataStep(Context),
                            ViewType = typeof (ImportHospitalDataView)      // import hospitals (optional)
                        },


                 
                };

           var list3 = new List<CompleteStep<GeographicContext>>
                {
                            new CompleteStep<GeographicContext>()
                        {
                            ViewModel = new MapRegionsToHospitalsStep(Context),
                            ViewType = typeof (MapRegionsToHospitalsView)
                        },

                  
                };

           AddGroupOfSteps(new StepGroup("States"), list);
           AddGroupOfSteps(new StepGroup("Data Source"), list2);
           AddGroupOfSteps(new StepGroup("Mapping"), list3);

       }
    }
}
