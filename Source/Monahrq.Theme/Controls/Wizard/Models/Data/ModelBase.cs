using System.Collections.Generic;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using PropertyChanged;

namespace Monahrq.Theme.Controls.Wizard.Models.Data
{
	/// <summary>
	/// Base class for Wizard Models.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.IWizardDataContextObject" />
	[ImplementPropertyChanged]
    public abstract class ModelBase : IWizardDataContextObject
    {

		/// <summary>
		/// Initializes a new instance of the <see cref="ModelBase"/> class.
		/// </summary>
		public ModelBase()
        {
            Reset();
        }

		/// <summary>
		/// Gets or sets the property bag.
		/// </summary>
		/// <value>
		/// The property bag.
		/// </value>
		Dictionary<string, object> PropertyBag { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="System.Object"/> with the specified key.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object"/>.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public object this[string key]
        {
            get
            {
                object result;
                if (!PropertyBag.TryGetValue(key, out result)) return null;
                return result;
            }
            set
            {
                PropertyBag[key] = value;
            }
        }

		/// <summary>
		/// Resets this instance.
		/// </summary>
		public void Reset()
        {
            PropertyBag = new Dictionary<string, object>();
            AfterReset();
        }

		/// <summary>
		/// Afters the reset.
		/// </summary>
		protected virtual void AfterReset()
        {
        }

		/// <summary>
		/// Cancels this instance.
		/// </summary>
		/// <returns></returns>
		public virtual bool Cancel()
        {
            return true;
        }

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <returns></returns>
		public abstract string GetName();

		/// <summary>
		/// Gets a value indicating whether this instance is custom.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is custom; otherwise, <c>false</c>.
		/// </value>
		public abstract bool IsCustom { get; }
		/// <summary>
		/// Gets the dataset item.
		/// </summary>
		/// <returns></returns>
		public abstract Dataset GetDatasetItem();
		/// <summary>
		/// Refreshes the target.
		/// </summary>
		/// <param name="targetToRefesh">The target to refesh.</param>
		/// <returns></returns>
		public abstract Target RefreshTarget(Target targetToRefesh);
    }
}
