using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Interface used to make objects posses a RegionManager
	/// </summary>
	public interface IRegionManagerAware
    {
		/// <summary>
		/// Gets or sets the region manager.
		/// </summary>
		/// <value>
		/// The region manager.
		/// </value>
		IRegionManager RegionManager { get; set; }
    }
    //public class RegionManagerObservableObject : ObservableObject<IRegionManager>
    //{
    //}
}
