using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Monahrq.DataSets.HospitalRegionMapping.Categories;
using Monahrq.DataSets.ViewModels.Hospitals;

namespace Monahrq.DataSets.Events
{
    public class HospitalCategoryDeletedEvent : CompositePresentationEvent<CategoryViewModel> { }
}
