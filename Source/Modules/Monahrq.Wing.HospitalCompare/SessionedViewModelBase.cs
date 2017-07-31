using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.Wing.HospitalCompare
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TContext">The type of the context.</typeparam>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{TContext}" />
	public abstract class SessionedViewModelBase<TContext, TEntity> : WizardStepViewModelBase<TContext>
        where TContext : DataSets.Model.DatasetContextBase
        where TEntity :  DatasetRecord
    {
		/// <summary>
		/// Gets or sets the session.
		/// </summary>
		/// <value>
		/// The session.
		/// </value>
		protected IStatelessSession Session { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SessionedViewModelBase{TContext, TEntity}"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		protected SessionedViewModelBase(TContext context)
            : base(context)
        {
        }

		/// <summary>
		/// Starts the import.
		/// </summary>
		public virtual void StartImport()
        {
            //Session = DataContextObject.ShellContext.SessionFactoryHolder.GetSessionFactory().OpenStatelessSession();
            Session = DataContextObject.Provider.SessionFactory.OpenStatelessSession();
        }

		/// <summary>
		/// Inserts the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		protected void Insert(TEntity entity)
        {
            Session.Insert(entity);
        }

		/// <summary>
		/// Imports the completed.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		protected virtual void ImportCompleted(bool success)
        {
            try
            {
                if (success)
                {
                    DataContextObject.Finish();
                }
                Session.Close();
            }
            catch (SessionException) { } // we don't care
            finally
            {
                Session.Dispose();
            }
        }
    }
}
