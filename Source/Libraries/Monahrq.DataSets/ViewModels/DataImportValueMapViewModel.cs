using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Regions;
using Monahrq.Theme.PopupDialog;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The dataset import wizard value map viewmodel.
    /// </summary>
    [ImplementPropertyChanged]
    [Export]
    public class DataImportValueMapViewModel
    {
        /// <summary>
        /// Gets or sets the region MGR.
        /// </summary>
        /// <value>
        /// The region MGR.
        /// </value>
        private IRegionManager RegionMgr { get; set; }
        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        protected IEventAggregator Events { get; set; }

        /// <summary>
        /// Gets or sets the size of the data file.
        /// </summary>
        /// <value>
        /// The size of the data file.
        /// </value>
        public string DataFileSize { get; set; }
        /// <summary>
        /// Gets or sets the data file records.
        /// </summary>
        /// <value>
        /// The data file records.
        /// </value>
        public int DataFileRecords { get; set; }
        /// <summary>
        /// Gets or sets the name of the data file.
        /// </summary>
        /// <value>
        /// The name of the data file.
        /// </value>
        public string DataFileName { get; set; }
        /// <summary>
        /// The mock data
        /// </summary>
        private ObservableCollection<MockMonahrqField> _mockData;
        /// <summary>
        /// Gets or sets the mock data.
        /// </summary>
        /// <value>
        /// The mock data.
        /// </value>
        public ObservableCollection<MockMonahrqField> MockData
        {
            get { return _mockData; }
            set { _mockData = value; }
        }

        /// <summary>
        /// Gets or sets the back click.
        /// </summary>
        /// <value>
        /// The back click.
        /// </value>
        public ICommand BackClick { get; set; }
        /// <summary>
        /// Gets or sets the cancel click.
        /// </summary>
        /// <value>
        /// The cancel click.
        /// </value>
        public ICommand CancelClick { get; set; }
        /// <summary>
        /// Gets or sets the check mappings click.
        /// </summary>
        /// <value>
        /// The check mappings click.
        /// </value>
        public ICommand CheckMappingsClick { get; set; }
        /// <summary>
        /// Gets or sets the import mapping click.
        /// </summary>
        /// <value>
        /// The import mapping click.
        /// </value>
        public ICommand ImportMappingClick { get; set; }
        /// <summary>
        /// Gets or sets the next step click.
        /// </summary>
        /// <value>
        /// The next step click.
        /// </value>
        public ICommand NextStepClick { get; set; }
        /// <summary>
        /// Gets or sets the save click.
        /// </summary>
        /// <value>
        /// The save click.
        /// </value>
        public ICommand SaveClick { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportValueMapViewModel"/> class.
        /// </summary>
        public DataImportValueMapViewModel()
        {
            RegionMgr = ServiceLocator.Current.GetInstance<IRegionManager>();
            Events = ServiceLocator.Current.GetInstance<IEventAggregator>();

            DataFileSize = "0 kb";
            DataFileRecords = 0;
            DataFileName = "Unknown";

            BackClick = new DelegateCommand<object>(BackClickExecute, BackClickCanExecute);
            CancelClick = new DelegateCommand(CancelClickExecute);
            CheckMappingsClick = new DelegateCommand<object>(CheckMappingsClickExecute, CheckMappingsClickCanExecute);
            ImportMappingClick = new DelegateCommand<object>(ImportMappingClickExecute, ImportMappingClickCanExecute);
            NextStepClick = new DelegateCommand<object>(NextStepClickExecute, NextStepClickCanExecute);
            SaveClick = new DelegateCommand<object>(SaveClickExecute, SaveClickCanExecute);

            SetupMockData();
        }

        #region Back Commands

        /// <summary>
        /// Backs the click execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void BackClickExecute(object arg)
        {
            RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.DataImportColMapOpt, UriKind.Relative));
        }

        /// <summary>
        /// Backs the click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool BackClickCanExecute(object arg)
        {
            return true;
        }

        #endregion Back Commands

        #region Cancel Commands

        /// <summary>
        /// Cancels the click execute.
        /// </summary>
        private void CancelClickExecute()
        {
            // TODO: Ask for a yes / no confirmation
            var dialog = ServiceLocator.Current.GetInstance<IPopupDialogService>();
            dialog.Closed += (o, e) =>
                {
                    if (dialog.ClickedButton == PopupDialogButtons.Yes)
                    {
                        RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView, UriKind.Relative));
                    }
                };
            dialog.ShowMessage(@"Are you sure you wish to cancel the data import process?", "Cancel Data Import", PopupDialogButtons.Yes | PopupDialogButtons.No);
        }

        /// <summary>
        /// Cancels the click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool CancelClickCanExecute(object arg)
        {
            return true;
        }

        #endregion Cancel Commands

        #region Check Mappings Commands

        /// <summary>
        /// Checks the mappings click execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void CheckMappingsClickExecute(object arg)
        {
            System.Windows.Forms.MessageBox.Show(@"Waiting for feedback from Vivek on what to do here.");
        }

        /// <summary>
        /// Checks the mappings click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool CheckMappingsClickCanExecute(object arg)
        {
            return true;
        }

        #endregion Check Mappings Commands

        #region Import Mapping Commands

        /// <summary>
        /// Imports the mapping click execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void ImportMappingClickExecute(object arg)
        {
            // TODO: Import mappings from file.
            System.Windows.Forms.MessageBox.Show(@"Open file / open dialog to get mappings file.");
        }

        /// <summary>
        /// Imports the mapping click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool ImportMappingClickCanExecute(object arg)
        {
            return true;
        }

        #endregion Import Mapping Commands

        #region Next Step Commands

        /// <summary>
        /// Nexts the step click execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void NextStepClickExecute(object arg)
        {
            RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.DataImportComplete, UriKind.Relative));
        }

        /// <summary>
        /// Nexts the step click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool NextStepClickCanExecute(object arg)
        {
            return true;
        }

        #endregion Next Step Commands

        #region Save Commands

        /// <summary>
        /// Saves the click execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void SaveClickExecute(object arg)
        {
            System.Windows.Forms.MessageBox.Show(@"Waiting for feedback from Vivek on what to do here.");
        }

        /// <summary>
        /// Saves the click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool SaveClickCanExecute(object arg)
        {
            return true;
        }

        #endregion Save Commands

        /// <summary>
        /// Setups the mock data used for design time.
        /// </summary>
        public void SetupMockData()
        {
            if (MockData != null)
                MockData.Clear();
            MockData = new ObservableCollection<MockMonahrqField>();

            var tempData = new MockMonahrqField();
            
            tempData.Name = "Discharge disposition";
            tempData.ShortDescription = "Short Description";
            tempData.LongDescription = "Long Description";
            tempData.Values.Add(new MockMonarqValues { ID = 0, Description = "0 : Ignore" });
            tempData.Values.Add(new MockMonarqValues { ID = 1, Description = "1 : Routine" });
            tempData.Values.Add(new MockMonarqValues { ID = 5, Description = "5 : Another type of facility" });
            tempData.Values.Add(new MockMonarqValues { ID = 6, Description = "6 : Home health care" });
            tempData.Values.Add(new MockMonarqValues { ID = 7, Description = "7 : Against medical advice" });
            tempData.Values.Add(new MockMonarqValues { ID = 20, Description = "20 : Died" });
            tempData.ClientValues.Add(new MockClientValues { ClientValue = "NULL", MonahrqID = 0 });
            tempData.ClientValues.Add(new MockClientValues { ClientValue = "1", MonahrqID = 0 });
            tempData.ClientValues.Add(new MockClientValues { ClientValue = "5", MonahrqID = 0 });
            tempData.ClientValues.Add(new MockClientValues { ClientValue = "6", MonahrqID = 6 });
            tempData.ClientValues.Add(new MockClientValues { ClientValue = "7", MonahrqID = 7 });
            tempData.ClientValues.Add(new MockClientValues { ClientValue = "20", MonahrqID = 20 });
            MockData.Add(tempData);

            tempData = new MockMonahrqField();
            tempData.Name = "Another one";
            tempData.ShortDescription = "Short Description";
            tempData.LongDescription = "Long Description";
            tempData.Values.Add(new MockMonarqValues { ID = 0, Description = "0 : Value 0" });
            tempData.Values.Add(new MockMonarqValues { ID = 1, Description = "1 : Value 1" });
            tempData.Values.Add(new MockMonarqValues { ID = 2, Description = "2 : Value 2" });
            tempData.Values.Add(new MockMonarqValues { ID = 3, Description = "3 : Value 3" });
            tempData.Values.Add(new MockMonarqValues { ID = 4, Description = "4 : Value 4" });
            tempData.Values.Add(new MockMonarqValues { ID = 5, Description = "5 : Value 5" });
            tempData.ClientValues.Add(new MockClientValues { ClientValue = "NULL", MonahrqID = 0 });
            MockData.Add(tempData);
        }

    }

    /// <summary>
    /// The design time mock monahrq field class.
    /// </summary>
    [ImplementPropertyChanged]
    public class MockMonahrqField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockMonahrqField"/> class.
        /// </summary>
        public MockMonahrqField()
        {
            if (Values != null)
                Values.Clear();
            if (ClientValues != null)
                ClientValues.Clear(); 
            Values = new ObservableCollection<MockMonarqValues>();
            ClientValues = new ObservableCollection<MockClientValues>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        /// <value>
        /// The short description.
        /// </value>
        public string ShortDescription { get; set; }
        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        /// <value>
        /// The long description.
        /// </value>
        public string LongDescription { get; set; }
        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public ObservableCollection<MockMonarqValues> Values { get; set; }
        /// <summary>
        /// Gets or sets the client values.
        /// </summary>
        /// <value>
        /// The client values.
        /// </value>
        public ObservableCollection<MockClientValues> ClientValues { get; set; }
    }

    /// <summary>
    /// the design time mock monahrq value class.
    /// </summary>
    [ImplementPropertyChanged]
    public class MockMonarqValues
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set;  }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
    }

    /// <summary>
    /// The design time mock client values class.
    /// </summary>
    [ImplementPropertyChanged]
    public class MockClientValues
    {
        /// <summary>
        /// Gets or sets the client value.
        /// </summary>
        /// <value>
        /// The client value.
        /// </value>
        public string ClientValue { get; set; }
        /// <summary>
        /// Gets or sets the monahrq identifier.
        /// </summary>
        /// <value>
        /// The monahrq identifier.
        /// </value>
        public int MonahrqID { get; set; }
    }

}
