using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.Wing.NursingHomeCompare
{
    public abstract class SessionedViewModelBase<TContext, TEntity> : WizardStepViewModelBase<TContext>
        where TContext : DataSets.Model.DatasetContextBase
        where TEntity :  DatasetRecord
    {
        protected IStatelessSession Session { get; set; }

        protected SessionedViewModelBase(TContext context)
            : base(context)
        {
        }

        public virtual void StartImport()
        {
            Session = DataContextObject.Provider.SessionFactory.OpenStatelessSession();
        }

        protected void Insert(TEntity entity)
        {
            Session.Insert(entity);
        }

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
            finally
            {
                Session.Dispose();
            }
        }
    }
}
