using System.Collections.Generic;
using System.Linq;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Wing.Dynamic.Models;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// Extension class for <see cref="WizardContext"/>
    /// </summary>
    static class XwalkDataObjectContextExtension
    {
        /// <summary>
        /// Creates the crosswalk mapping field view models.
        /// </summary>
        /// <param name="ctxt">The CTXT.</param>
        /// <returns></returns>
        public static IEnumerable<MappedFieldEntryViewModel> CreateCrosswalkMappingFieldViewModels(this WizardContext ctxt)
        {
            if (ctxt.Histogram == null) return Enumerable.Empty<MappedFieldEntryViewModel>();
            var histogram = ctxt.Histogram;
            return ctxt
                    .RequiredMappings.Concat(ctxt.OptionalMappings)
                    .Select(
                        kvp =>
                        {
                            if (string.IsNullOrEmpty(kvp.Key))
                            {
                                return null;
                            }
                            if (string.IsNullOrEmpty(kvp.Value))
                            {
                                return null;
                            }

                            //var findByName = ctxt.TargetElements
                            //        .Where(elem => elem.Hints.Contains(kvp.Key));
                            var findByName = ctxt.TargetElements
                                                 .Where(elem => elem.Name.EqualsIgnoreCase(kvp.Key));
                            var element = findByName.FirstOrDefault(elem => elem.Scope != null);
                            var field = histogram[kvp.Value];
                            if (element == null || field == null)
                            {
                                return null;
                            }
                            var model = new MappedFieldEntryViewModel(ctxt.TargetProperties[element.Name], element, field);
                            return model;
                        }).Where(item => item != null);
        }
    }
}