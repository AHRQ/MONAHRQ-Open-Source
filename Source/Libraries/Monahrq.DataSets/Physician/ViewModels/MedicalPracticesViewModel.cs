using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.DataSets.Physician.Views;
using Monahrq.Infrastructure.Domain.Physicians;

using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Utilities;
using Monahrq.Sdk.ViewModels;
using Monahrq.Theme.Converters;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using PropertyChanged;
using System.Windows.Forms;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.DataSets.Physician.ViewModels
{
    [Export]
    [ImplementPropertyChanged]
    public class MedicalPracticesViewModel : ListTabViewModel<MedicalPractice>
    {
        #region Fields and Constants

        public const int InitialLoadCount = 50;
        public const string SpliterWidthOn = "7";
        public const string DetailViewWidthOn = "0.2*";
        public const string DataViewWidthOn = "0.8*";
        public const string ZeroWidth = "0";
        private string _selectedFilter;
        private string _filterText;
        #endregion

        #region Properties

        public MedicalPractice NewMedicalPractice { get; set; }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;
            }
        }

        public string SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;
            }
        }

        public List<string> FilterEnumerations
        {
            get { return new List<string> { "", "Organization Name", "Group Practice Id" }; }
        }

        public bool IsDetailsViewOn { get; set; }

        public string DetailViewWidth { get; set; }

        public string SpliterWidth { get; set; }

        public string DataViewWidth { get; set; }

        #endregion

        #region Commands

        public DelegateCommand DetailsViewCommand { get; set; }

        public DelegateCommand<MedicalPractice> AddNewAdress { get; set; }

        #endregion

        #region Methods

        protected override void OnAddNewItem(MedicalPractice entity)
        {
            NavigateMedicalPracticeEdit("-1");
        }

        protected override void OnEdit(MedicalPractice entity)
        {
            var medicalPracticeId = entity != null ? entity.Id.ToString(CultureInfo.InvariantCulture) : CurrentSelectedItem.Id.ToString(CultureInfo.InvariantCulture);

            NavigateMedicalPracticeEdit(medicalPracticeId);
        }

        protected override void OnDelete(MedicalPractice entity)
        {
            if (entity == null) return;

            var associatedPhysicians = new List<string>();
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                associatedPhysicians = session.Query<Monahrq.Infrastructure.Domain.Physicians.PhysicianMedicalPractice>()
                                              .Where(x => x.MedicalPractice.Id == entity.Id && !x.Physician.IsDeleted)
                                              .Select(x => string.Format(" - {0} {1} (NPI: {2})", x.Physician.FirstName, x.Physician.LastName, x.Physician.Npi))
                                              .ToList();
            }

            if (associatedPhysicians.Any())
            {
                // string physicianNames = string.Join(",", associatedPhysicians);
                var warningMessage = string.Format("Unable to delete medical practice \"{0}\" because it is associated with the following physicians:", entity.Name) + Environment.NewLine;
                associatedPhysicians.ForEach(x => warningMessage += Environment.NewLine + x);

                MessageBox.Show(warningMessage, "Dataset Deletion Warning", MessageBoxButtons.OK);
                return;
            }
            else if (MessageBox.Show(string.Format(@"Are you sure want to delete {0} ?", entity.Name), @"Delete Confirmation", MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                return;
            }

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    var msg = string.Format("{0} {1} has been deleted.", entity.Name, Inflector.Titleize(entity.GetType().Name));

                    try
                    {
                        session.Evict(entity);
                        session.Delete(entity);
                        trans.Commit();
                        Notify(msg);
                    }
                    catch (Exception exc)
                    {
                        trans.Rollback();

                        var excToUse = exc;
                        LogEntityError(excToUse, entity.GetType(), entity.Name);
                    }
                }
            }

            CollectionItems.Remove(entity);
        }

        public override void Fetch()
        {
            //IsLastItemFetched = PageSize * CurrentPage >= TotalCount - InitialLoadCount;
            //CurrentPage++;
            //if (CollectionItems == null || IsLastItemFetched) return;

            //using (var session = DataserviceProvider.SessionFactory.OpenSession())
            //{
            //    Fetch(session);
            //}
        }

        protected override void ExecLoad(ISession session)
        {
            Fetch(session);
            CollectionItems.MoveCurrentToFirst();
        }

        private void Fetch(ISession session)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var selectedStates = configService.HospitalRegion.DefaultStates.OfType<string>().ToList();
            var statesSingleQuoted = string.Join(",", selectedStates.Select(state => "'" + state + "'").ToArray());
            var query = string.Format(@"SELECT mp.Id, Name,GroupPracticePacId,NumberofGroupPracticeMembers, COUNT(CASE WHEN a.Id is not null THEN 1 ELSE null END) as AddressCountForDisplay
                                        FROM MedicalPractices mp
                                        	lEFT JOIN Addresses a on mp.Id = a.MedicalPractice_Id
                                        WHERE a.State in ({0}) OR a.Id is null
                                        GROUP BY mp.Id, Name,GroupPracticePacId,NumberofGroupPracticeMembers
                                        ORDER BY mp.Id DESC", statesSingleQuoted);

            var items = session.CreateSQLQuery(query)
              .AddScalar("Id", NHibernateUtil.Int32)
              .AddScalar("GroupPracticePacId", NHibernateUtil.String)
              .AddScalar("Name", NHibernateUtil.String)
              .AddScalar("NumberofGroupPracticeMembers", NHibernateUtil.Int32)
              .AddScalar("AddressCountForDisplay", NHibernateUtil.Int32)
              .SetResultTransformer(new AliasToBeanResultTransformer(typeof(MedicalPractice)))
              .List();

            CollectionItems = new ListCollectionView(items);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 1;
            EventAggregator.GetEvent<RequestLoadMappingTabEvent>().Subscribe(type =>
                {
                    if (type == typeof(MedicalPracticeEditViewModel)) OnLoad();
                });

            EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe(s =>
            {
                if (s.Equals("Saved") && IsActive) OnLoad();
            });
        }

        protected override void InitProperties()
        {
            base.InitProperties();
            NewMedicalPractice = new MedicalPractice();
        }

        protected override void InitCommands()
        {
            base.InitCommands();

            DetailsViewCommand = new DelegateCommand(OnDetailsViewCommand);
        }

        private void OnDetailsViewCommand()
        {
            if (!IsDetailsViewOn)
            {
                DetailViewWidth = DetailViewWidthOn;
                SpliterWidth = SpliterWidthOn;
                DataViewWidth = DataViewWidthOn;
                IsDetailsViewOn = true;
            }
            else
            {
                DetailViewWidth = ZeroWidth;
                SpliterWidth = ZeroWidth;
                DataViewWidth = DataViewWidthOn;
                IsDetailsViewOn = false;
            }
        }

        protected override void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            if (!IsActive)
            {
                if (IsDetailsViewOn) DetailsViewCommand.Execute();
                return;
            }
            OnLoad();

            // Needed to run the filter on the newly loaded data.

            SelectedFilter = FilterEnumerations[0];
            FilterText = string.Empty;
            CollectionItems.Filter = null;
            CollectionItems.Filter = CompositeFilter;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private void NavigateMedicalPracticeEdit(string id)
        {
            var query = new UriQuery { { "MedicalPracticeId", id } };

            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MedicalPracticeEditView + query, UriKind.Relative));
        }

        private bool CompositeFilter(object obj)
        {
            var medicalPractice = obj as MedicalPractice;
            if (string.IsNullOrEmpty(FilterText) || medicalPractice == null) return true;

            if (SelectedFilter == FilterEnumerations[1])
            {
                return medicalPractice.Name.ToLower().Contains(FilterText.ToLower());
            }
            return SelectedFilter != FilterEnumerations[2] || medicalPractice.GroupPracticePacId.ToLower().Contains(FilterText.ToLower());
        }

        #endregion
    }
}
