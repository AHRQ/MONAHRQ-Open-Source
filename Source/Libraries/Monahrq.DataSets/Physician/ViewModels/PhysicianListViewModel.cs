using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using LinqKit;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Types;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Services.Import;
using Monahrq.Sdk.Utilities;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using PropertyChanged;
using db = Monahrq.Infrastructure.Domain.Physicians;

namespace Monahrq.DataSets.Physician.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListTabViewModel{db.Physician}" />
    [ImplementPropertyChanged]
    [Export]
    public class PhysicianListViewModel : ListTabViewModel<db.Physician>
    {
        #region Fields and Constants

        private const int INITIAL_LOAD_COUNT = 50;
        private string _selectedFilter;
        private string _filterText;

        #endregion

        #region Properties

        public string PhysicianSampleFile => "Physician Import Sample.csv";

        /// <summary>
        /// Gets or sets the selected filter.
        /// </summary>
        /// <value>
        /// The selected filter.
        /// </value>
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

        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        /// <value>
        /// The filter text.
        /// </value>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                //OnLoad();
                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;
            }
        }

        /// <summary>
        /// Gets the filter enumerations.
        /// </summary>
        /// <value>
        /// The filter enumerations.
        /// </value>
        public List<string> FilterEnumerations
        {
            get { return EnumExtensions.GetEnumDescriptions<FilterEnum>(); }
        }

        /// <summary>
        /// Gets or sets the physician importer.
        /// </summary>
        /// <value>
        /// The physician importer.
        /// </value>
        [Import(ImporterContract.Physician)]
        private IEntityFileImporter PhysicianImporter { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the add new physician.
        /// </summary>
        /// <value>
        /// The add new physician.
        /// </value>
        public DelegateCommand AddNewPhysician { get; set; }

        /// <summary>
        /// Gets or sets the import physician.
        /// </summary>
        /// <value>
        /// The import physician.
        /// </value>
        public DelegateCommand ImportPhysician { get; set; }

        /// <summary>
        /// Gets or sets the delete all physicians.
        /// </summary>
        /// <value>
        /// The delete all physicians.
        /// </value>
        public DelegateCommand DeleteAllPhysicians { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            OnLoad();
        }

        /// <summary>
        /// Fetches this instance.
        /// </summary>
        public override void Fetch()
        {}

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void ExecLoad(ISession session)
        {
            TotalCount = 0;
            CurrentPage = 0;
            PageSize = INITIAL_LOAD_COUNT;
            
            var statesList = ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList();
            ListExtensions.ForEach(statesList, x => TotalCount += session.Query<db.Physician>().Count(GetWhereClause(x)));

            Fetch(session, statesList);
            CollectionItems.MoveCurrentToFirst();
            IsLastItemFetched = PageSize*CurrentPage >= TotalCount - INITIAL_LOAD_COUNT;
            PageSize = 5;
        }

        /// <summary>
        /// Fetches the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="statesList">The states list.</param>
        private void Fetch(ISession session, IEnumerable<string> statesList)
        {
            var physicians = new List<db.Physician>();
            //var enumerable = statesList as string[] ?? statesList.ToArray();
            var states = statesList != null ? statesList.Distinct().ToList() : new List<string>();
            //var statesSingleQuoted = string.Join(",", states.Select(state => "'" + state + "'").ToArray());
            states.Sort();
            //var statesSingleQuoted = string.Join(",", states.Select(state => "'" + state + "'").ToArray());
            var statePredicates = string.Join(" OR ", states.Select(state => "state like '%" + state + "%'").ToArray());
            var physicianStatePredicates = string.Join(" OR ", states.Select(state => "states like '%" + state + "%'").ToArray());

            #region Original Query
            //            foreach (var state in states)
            //            {
            //                string query = string.Format(@";With PhysiciansCTE as  
            //(
            //		-- No Addresses
            //		SELECT DISTINCT p.Id, p.Npi,  p.FirstName,p.LastName,p.PrimarySpecialty, null as CityForDisplay, p.[States] as StateForDisplay
            //		FROM Physicians p
            //		WHERE p.IsDeleted=0 AND NOT EXISTS (select * from Physicians_MedicalPractices pm where pm.Physician_Id = p.Id)
            //		 AND NOT EXISTS (SELECT * FROM Addresses a3 WHERE a3.Physician_Id = p.Id)
            //         AND p.[States] like '%{0}%'
            //	UNION ALL  
            //		-- Physician Addresses                                 
            //		SELECT DISTINCT p.Id, p.Npi,  p.FirstName,p.LastName, p.PrimarySpecialty, a1.City as CityForDisplay, a1.[State] as StateForDisplay
            //			FROM Physicians p JOIN Addresses a1 on p.[Id] = a1.[Physician_Id] AND UPPER(a1.[AddressType]) = 'PHYSICIAN' and a1.[State] = '{0}'
            //			WHERE p.IsDeleted=0
            //			AND NOT EXISTS (select * from Physicians_MedicalPractices pm where pm.Physician_Id = p.Id)
            //	UNION ALL 
            //	    -- Medical Practice Addresses
            //		SELECT DISTINCT p.Id, p.Npi, p.FirstName, p.LastName, p.PrimarySpecialty, MAX(a.City) as CityForDisplay, MAx(a.[State]) as StateForDisplay	  
            //          FROM [dbo].[Addresses] a
            //	        INNER JOIN [dbo].[Physicians_MedicalPractices] pmp on pmp.[MedicalPractice_Id] = a.[MedicalPractice_Id] AND a.[State] = '{0}'
            //	        INNER JOIN [dbo].[Physicians] p ON p.[Id] = pmp.[Physician_Id] and p.[IsDeleted]=0
            //          where UPPER(a.[AddressType]) = 'MEDICALPRACTICE' and p.[States] like '%{0}%'
            //          GROUP BY p.Id, p.Npi,p.FirstName,p.LastName,p.PrimarySpecialty 
            //)
            //SELECT DISTINCT 
            //		pcte.Id, 
            //		pcte.Npi, 
            //		pcte.FirstName, 
            //		pcte.LastName, 
            //		pcte.PrimarySpecialty, 
            //		(SELECT TOP 1 p1.CityForDisplay 
            //		 From  PhysiciansCTE p1 
            //		 Where p1.Npi = pcte.Npi and p1.StateForDisplay = pcte.StateForDisplay and p1.CityForDisplay is not null)  as CityForDisplay, 
            //		pcte.StateForDisplay
            //FROM PhysiciansCTE pcte
            //WHERE pcte.StateForDisplay like '%{0}%' or (pcte.StateForDisplay is null and pcte.CityForDisplay is null)
            //GROUP BY pcte.Id, pcte.Npi, pcte.FirstName, pcte.LastName, pcte.StateForDisplay,pcte.CityForDisplay ,pcte.PrimarySpecialty
            //ORDER BY pcte.StateForDisplay, CityForDisplay, pcte.Npi, pcte.LastName;", state);
            #endregion

            #region Query Attempt 2
            //            var query = string.Format(@"
//SELECT        Id, Npi, FirstName, LastName, States, IsDeleted, City AS CityForDisplay, State AS StateForDisplay, PrimarySpecialty
//FROM            (SELECT        *, ROW_NUMBER() OVER (PARTITION BY NPI
//                          ORDER BY City DESC) AS rn
//FROM            (
//	SELECT        Phy.Id, Phy.Npi, Phy.FirstName, Phy.LastName, Phy.States, Phy.IsDeleted, adds.City, adds.State, Phy.PrimarySpecialty
//	FROM            Physicians AS Phy LEFT OUTER JOIN
//							 Addresses AS adds ON Phy.Id = adds.Physician_Id AND Phy.Id = adds.Physician_Id
//	WHERE        (Phy.IsDeleted = 0) AND ({0})
//	UNION ALL
//	SELECT        Phy.Id, Phy.Npi, Phy.FirstName, Phy.LastName, Phy.States, Phy.IsDeleted, Adds.City, Adds.State, Phy.PrimarySpecialty
//	FROM            Physicians AS Phy INNER JOIN
//							 Physicians_MedicalPractices AS PMp ON Phy.Id = PMp.Physician_Id INNER JOIN
//							 Addresses AS Adds ON PMp.MedicalPractice_Id = Adds.MedicalPractice_Id
//	WHERE        (Phy.IsDeleted = 0) AND ({0})
//				) AS Phy) AS PPhy
//WHERE        PPhy.rn = 1
//ORDER BY StateForDisplay, CityForDisplay, Npi, LastName
            //", statePredicates);
            #endregion

            var query =
                string.Format(@";WITH NormalizedPhysicians(Id, NPI, FirstName, LastName, PrimarySpecialty, State, Data) as 
(
    select Id, NPI, FirstName, LastName, PrimarySpecialty, CAST(LEFT(States, CHARINDEX(',',States+',')-1) AS NVARCHAR(255)),
        STUFF(States, 1, CHARINDEX(',',States+','), '') as Data
    from [Physicians]
    where isDeleted = 0
    union all
    select Id, NPI, FirstName, LastName, PrimarySpecialty, CAST(LEFT(Data, CHARINDEX(',',Data+',')-1) AS NVARCHAR(255)),
        STUFF(Data, 1, CHARINDEX(',',Data+','), '')
    from NormalizedPhysicians
    where Data > ''
)
SELECT        Id, Npi, FirstName, LastName, States, IsDeleted, City AS CityForDisplay, State AS StateForDisplay, PrimarySpecialty
FROM            (SELECT        *, ROW_NUMBER() OVER (PARTITION BY NPI
                          ORDER BY City DESC) AS rn
FROM            (
    SELECT        Phy.Id, Phy.Npi, Phy.FirstName, Phy.LastName, Phy.States, Phy.IsDeleted, 'N/A' as [City], 
                  (CASE 
			            WHEN CHARINDEX(Phy.[States],',',0) > 0 THEN SUBSTRING(Phy.[States],0, CHARINDEX(Phy.[States],',',0))
			            ELSE Phy.[States]
		           END) as [State], Phy.PrimarySpecialty
	FROM            Physicians AS Phy
	WHERE (Phy.Id NOT IN (SELECT DISTINCT adds.Physician_Id FROM Addresses AS adds WHERE Phy.Id = adds.Physician_Id))   
	 AND  (Phy.Id NOT IN (SELECT DISTINCT pmp.[Physician_Id] FROM Physicians_MedicalPractices AS pmp WHERE pmp.[Physician_Id] = Phy.Id))
	 AND (Phy.IsDeleted = 0) AND ({1})
	UNION ALL
	SELECT        Phy.Id, Phy.Npi, Phy.FirstName, Phy.LastName, Phy.States, Phy.IsDeleted, adds.City, adds.State, Phy.PrimarySpecialty
	FROM            Physicians AS Phy LEFT OUTER JOIN
							 Addresses AS adds ON Phy.Id = adds.Physician_Id AND Phy.Id = adds.Physician_Id
	WHERE        (Phy.IsDeleted = 0) AND ({0})
	UNION ALL
	SELECT        Phy.Id, Phy.Npi, Phy.FirstName, Phy.LastName, Phy.States, Phy.IsDeleted, Adds.City, Adds.State, Phy.PrimarySpecialty
	FROM            Physicians AS Phy INNER JOIN
							 Physicians_MedicalPractices AS PMp ON Phy.Id = PMp.Physician_Id INNER JOIN
							 Addresses AS Adds ON PMp.MedicalPractice_Id = Adds.MedicalPractice_Id
	WHERE        (Phy.IsDeleted = 0) AND ({0})
				) AS Phy) AS PPhy
WHERE        PPhy.rn = 1
ORDER BY StateForDisplay, CityForDisplay, Npi, LastName", statePredicates, physicianStatePredicates);

            var items = session.CreateSQLQuery(query)
                .AddScalar("Id", NHibernateUtil.Int32)
                .AddScalar("Npi", NHibernateUtil.Int64)
                .AddScalar("FirstName", NHibernateUtil.String)
                .AddScalar("LastName", NHibernateUtil.String)
                .AddScalar("PrimarySpecialty", NHibernateUtil.String)
                .AddScalar("CityForDisplay", NHibernateUtil.String)
                .AddScalar("StateForDisplay", NHibernateUtil.String)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof (db.Physician)))
                .List();

            physicians.AddRange(items.Cast<db.Physician>().ToList());

            CollectionItems = new ListCollectionView(physicians);
           // RaiseErrorsChanged(() => CollectionItems);

            DeleteAllPhysicians = new DelegateCommand(OnDeleteAllPhysicians, () =>
            {
                return CollectionItems != null && CollectionItems.Count > 0;
                //return true;
            });
        }

        /// <summary>
        /// Called when [OnEdit].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected override void OnEdit(db.Physician entity)
        {
            var physicianId = entity != null
                ? entity.Id.ToString(CultureInfo.InvariantCulture)
                : CurrentSelectedItem.Id.ToString(CultureInfo.InvariantCulture);

            var q = new UriQuery {{"PhysicianId", physicianId}};

            NavigateToEditScreen(q);
        }

        /// <summary>
        /// Called when [delete].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected override async void OnDelete(db.Physician entity)
        {
            if (entity == null) return;

            var result =
                MessageBox.Show(@"Are you sure want to delete the data for this physician, """ + entity.Name + @"""?", @"Delete Confirmation", MessageBoxButtons.OKCancel);

            if (result != DialogResult.OK) return;

            var errorOccurred = false;
            Exception errorException = null;
            var progressService = new ProgressService();

            progressService.SetProgress("Deleting physican", 0, false, true);

            //await Task.Delay(500);

            string entityName = entity.Name;
            var executedSuccessully = await progressService.Execute(() =>
            {
                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        try
                        {
                            var updateQuery = string.Format(@"UPDATE {0} SET IsDeleted = 1 WHERE Id = {1} AND IsDeleted = 0",
                                typeof (db.Physician).EntityTableName(),
                                entity.Id);
                            session.CreateSQLQuery(updateQuery).ExecuteUpdate();
                            session.Flush();
                            trans.Commit();

                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            },
            opResult =>
            {
                if (!opResult.Status && opResult.Exception != null)
                {
                    errorOccurred = true;
                    errorException = opResult.Exception;
                }
                else
                {
                    errorOccurred = true;
                }
            }, new CancellationToken());

            progressService.SetProgress("Completed", 100, true, false);

            if (errorOccurred && errorException != null)
            {
                Logger.Write(errorException, "Error deleting physician \"{0}\"", entity.Name);
                LogEntityError(errorException, typeof(db.Physician), entityName);
                return;
            }

            CollectionItems.Remove(entity);
            CollectionItems.Refresh();

            Notify(string.Format("{0} {1} has been deleted.", entityName,
                       Inflector.Titleize(typeof(db.Physician).Name)));
        }

        /// <summary>
        /// Called when [delete all physicians].
        /// </summary>
        private async void OnDeleteAllPhysicians()
        {
            //bool noPhysicianDataSetCheck = true;

            var websiteDatasets = new Dictionary<string,List<string>>();
            var contextualStates = ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList();
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                foreach (var state in contextualStates)
                {
                    var query = string.Format(@"select distinct ds.[File], w.[Name] from [Wings_Datasets] ds
	inner join [Websites_WebsiteDatasets] wd on wd.[Dataset_Id] = ds.[Id]
    inner join [Websites] w on w.[Id] = wd.[Website_Id]
where ds.[ProviderStates] like '%{0}%' and ds.[ProviderUseRealtime]=0;", state);

                    var foundItems = session.CreateSQLQuery(query)
                        .AddScalar("File", NHibernateUtil.String)
                        .AddScalar("Name", NHibernateUtil.String)
                        .DynamicList();

                    if (foundItems.Any())
                    {
                        foreach (var item in foundItems)
                        {
                            if (!websiteDatasets.ContainsKey(item.Name))
                                websiteDatasets.Add(item.Name, new List<string> { item.File });
                            else
                            {
                                var datasets = websiteDatasets[item.Name];
                                if (!datasets.Contains(item.File))
                                {
                                    datasets.Add(item.File);
                                    websiteDatasets[item.Name] = datasets;
                                }
                            }
                            
                        }
                        
                    }
                    
                }
            }

            if (websiteDatasets.Any())
            {
                var message = new StringBuilder();

                foreach (var wd in websiteDatasets)
                {
                    foreach (var ds in  wd.Value.ToList())
                        message.AppendLine(ds + " ( Website: " + wd.Key + " )");
                }

                MessageBox.Show(string.Format(@"Please remove the following dataset(s) from their corresponding website(s) before deleting physicians for the following states {1}:{0}{0}{2}", Environment.NewLine, string.Join(", ", contextualStates), message), 
                                @"Delete Confirmation", MessageBoxButtons.OKCancel);
                return;
            }

            var result = MessageBox.Show(@"Are you sure want to delete the data for all physicians?", @"Delete Confirmation", MessageBoxButtons.OKCancel);

            if (result != DialogResult.OK) return;

            var deleteResult = await Task.Run(() =>
            {
                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        try
                        {
                            var deleteAllQuery = string.Format(
                                @"
truncate table [dbo].[Addresses];
truncate table [dbo].[Physicians_AffiliatedHospitals];
truncate table [dbo].[Physicians_Audits];
truncate table [dbo].[Physicians_MedicalPractices];
delete from [dbo].[MedicalPractices];
delete from [dbo].[Physicians];");

                            session.CreateSQLQuery(deleteAllQuery)
                                   .SetTimeout(50000)
                                   .ExecuteUpdate();

                            session.Flush();
                            trans.Commit();
                            
                            return new DeleteAllPhysiciansResult { Status = true};
                        }
                        catch (Exception exc)
                        {
                            trans.Rollback();

                            return new DeleteAllPhysiciansResult { Status = false, Exception = exc };
                        }
                    }
                }
            });

            if (deleteResult.Status)
            {
                Notify(string.Format("All {0} have been successfully deleted.",
                    Inflector.Pluralize(Inflector.Titleize(typeof(db.Physician).Name))));
                CollectionItems = new ListCollectionView(new List<db.Physician>());
            }

            if (!deleteResult.Status && deleteResult.Exception != null)
            {
                var fullMessage = new StringBuilder();
                fullMessage.AppendLine("An error occurred while deleting all physicans. Error Message: " + deleteResult.Exception.Message);
                fullMessage.AppendLine();
                fullMessage.AppendLine("Stack Trace: " + deleteResult.Exception.StackTrace);
                Logger.Write(deleteResult.Exception, "Error deleting all physicians");

                LogEntityError(deleteResult.Exception, typeof (db.Physician), "*");
            }

            //CollectionItems.Refresh();
            ForceLoad();
        }

        /// <summary>
        /// The deleta all physicians result.
        /// </summary>
        private class DeleteAllPhysiciansResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="DeleteAllPhysiciansResult"/> is status.
            /// </summary>
            /// <value>
            ///   <c>true</c> if status; otherwise, <c>false</c>.
            /// </value>
            public bool Status { get; set; }
            /// <summary>
            /// Gets or sets the exception.
            /// </summary>
            /// <value>
            /// The exception.
            /// </value>
            public Exception Exception { get; set; }
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();

            AddNewPhysician = new DelegateCommand(OnAddNewPhysician);
            ImportPhysician = new DelegateCommand(OnImportPhysician);
        }

        /// <summary>
        /// Called when [import physician].
        /// </summary>
        private void OnImportPhysician()
        {
            //TODO: Implement Code for import custom physician
            PhysicianImporter.Execute();
            ForceLoad();
        }

        /// <summary>
        /// Called when [add new physician].
        /// </summary>
        private void OnAddNewPhysician()
        {
            var q = new UriQuery {{"PhysicianId", "-1"}};
            NavigateToEditScreen(q);
        }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 0;
            EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe(s =>
            {
                if (s.Equals("Saved")) OnLoad();
            });

            //PhysicianImporter.Importing -= PhysicianImporter_Importing;
            PhysicianImporter.Importing += PhysicianImporter_Importing;
            // PhysicianImporter.Imported -= PhysicianImporter_Imported;
            PhysicianImporter.Imported += PhysicianImporter_Imported;

            EventAggregator.GetEvent<SimpleImportCompletedEvent>().Subscribe(Requery);
        }

        /// <summary>
        /// Requeries the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void Requery(ISimpleImportCompletedPayload payload)
        {
            if (!payload.Inserted.Any()) return;

            if (payload.CountInserted > 0)
            {
                OnLoad();
            }
        }

        /// <summary>
        /// Handles the IsActiveChanged event of the ListTabViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            if (!IsActive) return;

            OnLoad();

            // Needed to run the filter on the newly loaded data.
            FilterText = string.Empty;
            SelectedFilter = FilterEnum.None.GetDescription();
            CollectionItems.Filter = null;
            CollectionItems.Filter = CompositeFilter;
        }

        /// <summary>
        /// Handles the Imported event of the PhysicianImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PhysicianImporter_Imported(object sender, EventArgs e)
        {
            EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
        }

        /// <summary>
        /// Handles the Importing event of the PhysicianImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PhysicianImporter_Importing(object sender, EventArgs e)
        {
            EventAggregator.GetEvent<PleaseStandByEvent>().Publish(PhysicianImporter.CreatePleaseStandByEventPayload());
        }

        /// <summary>
        /// Composites the filter.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool CompositeFilter(object obj)
        {

            var item = obj as db.Physician;
            if (item == null || string.IsNullOrEmpty(FilterText) || string.IsNullOrEmpty(SelectedFilter)) return true;

            var selctedFilterEnum = EnumExtensions.GetEnumValueFromDescription<FilterEnum>(SelectedFilter);

            var filterTxt = FilterText.Trim();
            switch (selctedFilterEnum)
            {
                case FilterEnum.Npi:
                    return item.Npi.HasValue &&
                           item.Npi.Value.ToString(CultureInfo.CurrentCulture).ContainsIgnoreCase(filterTxt);
                case FilterEnum.PhysicianName:
                    var physicianName = string.Format("{0} {1}", item.FirstName, item.LastName);
                    return !string.IsNullOrEmpty(physicianName) && physicianName.ContainsIgnoreCase(filterTxt);
                case FilterEnum.City:
                    return !string.IsNullOrEmpty(item.CityForDisplay) &&
                           item.CityForDisplay.ContainsIgnoreCase(filterTxt.ToLower());
                case FilterEnum.State:
                    return !string.IsNullOrEmpty(item.StateForDisplay) &&
                           item.StateForDisplay.ContainsIgnoreCase(filterTxt.ToLower());
                case FilterEnum.PrimarySpeciality:
                    return (!string.IsNullOrEmpty(item.PrimarySpecialty) &&
                            item.PrimarySpecialty.ContainsIgnoreCase(filterTxt));
                default:
                    return true;
            }
        }

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Determines whether [is navigation target] [the specified navigation context].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when [navigated from].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Navigates to edit screen.
        /// </summary>
        /// <param name="query">The query.</param>
        private void NavigateToEditScreen(UriQuery query)
        {
            RegionManager.RequestNavigate(RegionNames.MainContent,
                new Uri(ViewNames.PhysicianDetail + query, UriKind.Relative));
        }

        /// <summary>
        /// Gets the where clause.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private Expression<Func<db.Physician, bool>> GetWhereClause(string state)
        {
            var clause = PredicateBuilder.True<db.Physician>();
            //clause.And(p => p.Practices.Any(mp => mp.Addresses.Any(a => a.State == state)) && !p.IsDeleted);
            clause.And(p => p.States.Contains(state));

            if (string.IsNullOrEmpty(FilterText) || string.IsNullOrEmpty(SelectedFilter)) return clause;

            var selctedFilterEnum = EnumExtensions.GetEnumValueFromDescription<FilterEnum>(SelectedFilter);

            switch (selctedFilterEnum)
            {
                case FilterEnum.Npi:
                    clause.And(p => p.Npi.Value.ToString().Contains(FilterText));
                    break;
                case FilterEnum.PhysicianName:
                    clause.And(p => p.FirstName.Contains(FilterText) || p.LastName.Contains(FilterText));
                    break;
                case FilterEnum.City:
                    clause.And(
                        p =>
                            p.PhysicianMedicalPractices.Any(
                                pmp => pmp.MedicalPractice.Addresses.Any(a => a.City.Contains(FilterText))));
                    break;
                case FilterEnum.State:
                    clause.And(
                        p =>
                            p.PhysicianMedicalPractices.Any(
                                pmp => pmp.MedicalPractice.Addresses.Any(a => a.State.Contains(FilterText))));
                    break;
                case FilterEnum.PrimarySpeciality:
                    clause.And(p => p.PrimarySpecialty.Contains(FilterText));
                    break;
            }
            return clause;
        }

        #endregion
    }

    /// <summary>
    /// The physician listing filter by enumeration.
    /// </summary>
    public enum FilterEnum
    {
        /// <summary>
        /// The none
        /// </summary>
        [Description("")] None,

        /// <summary>
        /// The npi
        /// </summary>
        [Description("NPI")] Npi,

        /// <summary>
        /// The physician name
        /// </summary>
        [Description("Physician Name")] PhysicianName,

        /// <summary>
        /// The city
        /// </summary>
        [Description("City")] City,

        /// <summary>
        /// The state
        /// </summary>
        [Description("State")] State,

        /// <summary>
        /// The primary speciality
        /// </summary>
        [Description("Primary Speciality")] PrimarySpeciality
    }
}