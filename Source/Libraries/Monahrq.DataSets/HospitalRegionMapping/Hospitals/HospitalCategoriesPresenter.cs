using System.Linq;

namespace Monahrq.DataSets.HospitalRegionMapping.Hospitals
{
    /// <summary>
    /// The hospital categories presenter class.
    /// </summary>
    public class HospitalCategoriesPresenter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalCategoriesPresenter"/> class.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        public HospitalCategoriesPresenter(HospitalViewModel hospital)
        {
            Categories = string.Join(", ", hospital.Hospital.Categories.Select(cat => cat.Name));
        }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public string Categories { get; set; } 
    }
}
