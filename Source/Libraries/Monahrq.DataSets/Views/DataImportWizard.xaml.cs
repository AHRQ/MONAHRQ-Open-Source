using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;
using Monahrq.DataSets.Events;
using System;
using Monahrq.Theme.Controls.Wizard.Models.Data;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using System.Linq;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// The data import wizard user control.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    [Export("DataImportWizard")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class DataImportWizard : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportWizard"/> class.
        /// </summary>
        [ImportingConstructor]
        public DataImportWizard()
        {
            InitializeComponent();

            Loaded += delegate
            {
                var eventaggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
                var currentTypeModel = CurrentDataTypeModel();
                var currentType = currentTypeModel.Item1;
                var existingTypeId = currentTypeModel.Item2;
                var provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
                Guid wingGuid;
                using (var sess = provider.SessionFactory.OpenStatelessSession())
                {
                    wingGuid = sess.Query<Target>()
                        .Join(sess.Query<Wing>(), t => t.Owner.Id, w => w.Id, (t, w) => new {t, w})
                        .Where(@t1 => @t1.t.Name == currentType.Target.Name)
                        .Select(@t1 => @t1.w.WingGUID).FirstOrDefault();
                }

                //var id = wing.WingGUID;
                var args = new WizardStepsRequestEventArgs<DataTypeModel, Guid, int?>(currentType, wingGuid, existingTypeId);
                eventaggregator.GetEvent<WizardStepsRequestEvent<DataTypeModel, Guid, int?>>().Publish(args);
                if (args.WizardSteps == null)
                {
                    args.WizardSteps = new DefaultStepCollection(currentType, existingTypeId);
                }
                this.DataContext = FactoryWizardViewModel(args);

            };

            Unloaded += delegate
            {
                this.DataContext = null;
            };

            DataContextChanged += (o, e) =>
                {
                    var wizard = e.OldValue as IWizardViewModel;
                    if (wizard != null)
                    {
                        wizard.Detach();
                    }
                    wizard = e.NewValue as IWizardViewModel;
                    if (wizard != null)
                    {
                        wizard.Attach();
                    }
                };
        }

        /// <summary>
        /// Gets or sets the wizard.
        /// </summary>
        /// <value>
        /// The wizard.
        /// </value>
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

        /// <summary>
        /// Factories the wizard view model.
        /// </summary>
        /// <param name="args">The <see cref="WizardStepsRequestEventArgs{DataTypeModel, Guid, System.Nullable{System.Int32}}"/> instance containing the event data.</param>
        /// <returns></returns>
        private object FactoryWizardViewModel(WizardStepsRequestEventArgs<DataTypeModel, Guid, int?> args)
        {
            var wizardViewModelType = typeof(DataWizardViewModel<>).MakeGenericType(args.WizardSteps.ContextType);
            var wizardViewModel = wizardViewModelType.GetConstructor(new Type[0]).Invoke(new object[0]);
            wizardViewModelType.GetMethod("ProvideSteps").Invoke(wizardViewModel, new[] { args.WizardSteps });
            return wizardViewModel;
        }

        /// <summary>
        /// Currents the data type model.
        /// </summary>
        /// <returns></returns>
        Tuple<DataTypeModel, int?> CurrentDataTypeModel()
        {
            var requestEvent = ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<RequestCurrentDataTypeModelEvent>();
            var args = new RequestCurrentDataTypeModelEventArgs();
            requestEvent.Publish(args);
            return new Tuple<DataTypeModel, int?>(args.Data, args.DataId);
        }

    }
}
