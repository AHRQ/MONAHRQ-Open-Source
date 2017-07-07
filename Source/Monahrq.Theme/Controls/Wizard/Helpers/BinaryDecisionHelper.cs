using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Monahrq.Theme.Controls.Wizard.Models;

namespace Monahrq.Theme.Controls.Wizard.Helpers
{

	/// <summary>
	/// This is for use with a simple yes/no decision step
	/// </summary>
	public class BinaryDecisionHelper
    {
		/// <summary>
		/// This is for use with a simple yes/no decision step
		/// </summary>
		/// <param name="displayName">The display name.</param>
		public void ConfigureBinaryDecision( string displayName = "" )
        {
            var list = new List<RouteOptionViewModel<bool>>();
            list.Add( new RouteOptionViewModel<bool>( true, 0 ) );

            // If they choose no, we skip the steps passed in for yes.  The StepManager puts these steps back in if they choose yes.
            // I guess I haven't yet come across the need to skip steps for a no selection.  Could add that param.
           
            list.Add( new RouteOptionViewModel<bool>( false, 1 ) );
            BinaryDecisionGroup = new RouteOptionGroupViewModel<bool>( displayName ) { OptionModels = new ReadOnlyCollection<RouteOptionViewModel<bool>>( list ) };
        }

		/// <summary>
		/// Pre: SimpleYesNoDecisionHasBeenMade() has returned true
		/// </summary>
		/// <returns></returns>
		public bool GetValueOfBinaryDecision()
        {
            return (bool)BinaryDecisionGroup.OptionModels.First(rovm => rovm.IsSelected).GetValue();
        }

		/// <summary>
		/// Binaries the decision has been made.
		/// </summary>
		/// <returns></returns>
		public bool BinaryDecisionHasBeenMade()
        {
            return BinaryDecisionGroup.OptionModels.FirstOrDefault(rovm => rovm.IsSelected) != null;
        }

		/// <summary>
		/// Gets the binary decision group.
		/// </summary>
		/// <value>
		/// The binary decision group.
		/// </value>
		public RouteOptionGroupViewModel<bool> BinaryDecisionGroup { get; private set; }
    }

}
