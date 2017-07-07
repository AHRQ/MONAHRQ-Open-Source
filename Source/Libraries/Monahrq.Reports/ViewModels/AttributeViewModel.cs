using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Reports.Attributes;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;
using System.ComponentModel;

namespace Monahrq.Reports.ViewModels
{
    public enum AttributeType { Filters = 1, Display, ComparisonKeys, Columns, Audience };

    /// <summary>
    /// Attributes class for the report
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    public class AttributeViewModel : BaseViewModel
    {

        /// <summary>
        /// Gets or sets the type of the attribute.
        /// </summary>
        /// <value>
        /// The type of the attribute.
        /// </value>
        public AttributeType AttributeType { get; set; }

        #region Properties

        private bool _isPresent;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is present and raises the property changed event when IsPresent is changed
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is present; otherwise, <c>false</c>.
        /// </value>
        public bool IsPresent
        {
            get { return _isPresent; }
            set
            {
                _isPresent = value;
                RaiseErrorsChanged(() => IsPresent);
            }
        }

        private string _header;
        /// <summary>
        /// Gets or sets the header and raises the property changed event when header is changed
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                RaisePropertyChanged(() => Header);
            }
        }

        public override string ToString()
        {
            return Header;
        }

        private string _subTitle;
        /// <summary>
        /// Gets or sets the sub title and raises the property changed event when SubTitle is changed
        /// </summary>
        /// <value>
        /// The sub title.
        /// </value>
        public string SubTitle
        {
            get { return _subTitle; }
            set
            {
                _subTitle = value;
                RaisePropertyChanged(() => SubTitle);
            }
        }

        private string _description;
        /// <summary>
        /// Gets or sets the description  and raises the property changed event when <see cref="Description"/> is changed
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged(() => Description);
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the report.
        /// </summary>
        /// <value>
        /// The report.
        /// </value>
        public Report Report { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeViewModel"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        public AttributeViewModel(Report report)
        {
            Report = report;
            IsPresent = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeViewModel"/> class.
        /// </summary>
        public AttributeViewModel()
        {

        }
    }

    /// <summary>
    /// To generate the attribute collection from the report
    /// </summary>
    public static class AttributeViewModelFactory
    {
        public static AttributeCollectionSet Get(Report report)
        {
            return new AttributeCollectionSet(report);
        }
    }

    /// <summary>
    /// Extension class for <see cref="ReportAttributeOption"/>
    /// </summary>
    public static class AttributeEnumExtension
    {
        public static bool Has(this ReportAttributeOption type, ReportAttributeOption value)
        {
            try
            {
                return (type & value) == value;
            }
            catch
            {
                return false;
            }
        }
    }

    /*BASE CLASS FOR ENUM BASED ATTRIBUTE*/
    /// <summary>
    /// Base class for Enum based attribute
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    public abstract class AttributeViewModel<TAttribute> : AttributeViewModel
    {
        protected AttributeViewModel() { }

        private IEnumerationSetCollection<TAttribute> _attributeSet;
        /// <summary>
        /// Gets or sets the attribute set.
        /// </summary>
        /// <value>
        /// The attribute set.
        /// </value>
        public IEnumerationSetCollection<TAttribute> AttributeSet
        {
            get { return _attributeSet; }
            protected set
            {
                if (_attributeSet != null)
                {
                    _attributeSet.PropertyChanged -= ReconcileValue;
                }
                _attributeSet = value; if (_attributeSet != null)
                {
                    _attributeSet.PropertyChanged += ReconcileValue;
                }
                RaisePropertyChanged(() => AttributeSet);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TAttribute"/> value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public abstract TAttribute Value
        {
            get;
            set;
        }

        /// <summary>
        /// Reconciles the <see cref="Value"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ReconcileValue(object sender, PropertyChangedEventArgs e)
        {
            Value = AttributeSet.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeViewModel{TAttribute}"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        protected AttributeViewModel(Report report)
            : base(report)
        {
            Report = report;
            IsPresent = false;
        }

        /// <summary>
        ///To override the ToString and it returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return AttributeSet.ToString();
        }
    }

    /*COMPOSITE ENUM BASED ATTRIBUTE (has multiple categories that are combine to one category on UI)*/
    /// <summary>
    /// Class for composite enum based attribute which has multiple categories that are combine to one category on UI
    /// </summary>
    /// <seealso cref="Monahrq.Reports.ViewModels.AttributeViewModel{Monahrq.Infrastructure.Entities.Domain.Reports.ReportFilter}" />
    public class FiltersViewModel : AttributeViewModel<ReportFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FiltersViewModel"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        public FiltersViewModel(Report report)
            : base(report)
        {
            Header = "Filters";
            SubTitle = "Filters:";
            Description = "Select the filtering options for this report.";
            AttributeSet = new FilterSetCollection(report);
            AttributeType = AttributeType.Filters;
        }

        /// <summary>
        /// Gets or sets the value and raises the property changed event when its value is changed
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override ReportFilter Value
        {
            get
            {
                return Report.Filter;
            }
            set
            {
                Report.Filter = value;
                RaisePropertyChanged(()=>Value);
            }
        }

    }

    /// <summary>
    /// View model class for Profile display
    /// </summary>
    /// <seealso cref="Monahrq.Reports.ViewModels.AttributeViewModel{Monahrq.Infrastructure.Entities.Domain.Reports.ReportProfileDisplayItem}" />
    public class ProfileDisplayViewModel : AttributeViewModel<ReportProfileDisplayItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileDisplayViewModel"/> class, it also initializes the defaults
        /// </summary>
        /// <param name="report">The report.</param>
        public ProfileDisplayViewModel(Report report)
            : base(report)
        {
            AttributeType = AttributeType.Display;
            Header = "Display";
            SubTitle = "Display:";
            Description = "Select the hospital profile attributes to display for this report.";
            if(report.IsDefaultReport)
            {
                AttributeSet = new ReportProfileSetCollection(
                    ReportProfileDisplayItem.Basic|
                    ReportProfileDisplayItem.CostToChargeMedicare | 
                    ReportProfileDisplayItem.Map |
                    ReportProfileDisplayItem.PatientExperience | 
                    ReportProfileDisplayItem.PayerCost
                );
            }else{
                AttributeSet = new ReportProfileSetCollection(report.ReportProfile);
            }
            IsPresent = true;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override ReportProfileDisplayItem Value
        {
            get
            {
                return Report.ReportProfile;
            }
            set
            {
                Report.ReportProfile = value;
            }
        }
    }

    /// <summary>
    /// View Model class for Audiences
    /// </summary>
    /// <seealso cref="Monahrq.Reports.ViewModels.AttributeViewModel{Monahrq.Infrastructure.Entities.Domain.Reports.Audience}" />
    public class AudiencesViewModel : AttributeViewModel<Audience>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudiencesViewModel"/> class, it also initializes the defaults
        /// </summary>
        /// <param name="report">The report.</param>
        public AudiencesViewModel(Report report)
            : base(report)
        {
            AttributeType = AttributeType.Audience;
            Header = "Audience";
            SubTitle = "Audience:";
            Description = "Select the audience type(s).";
            AttributeSet = new AudienceSetCollection(report.Audiences.Any() ? report.Audiences[0] : Audience.None);
            IsPresent = true;
        }

        /// <summary>
        /// Gets the first value of the Audience from the report  or sets the value by adding audience to the report.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override Audience Value
        {
            get
            {
                return Report.Audiences[0];
            }
            set
            {
                if (value != Audience.None)
                {
                    Report.Audiences.Add(value);
                }
            }
        }
    }

    /// <summary>
    /// Class to hold the Attribute Collection Set
    /// </summary>
    public class  AttributeCollectionSet
    {
        /// <summary>
        /// Gets or sets the attribute view models.
        /// </summary>
        /// <value>
        /// The attribute view models.
        /// </value>
        public List<AttributeViewModel> AttributeViewModels { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCollectionSet"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        public AttributeCollectionSet(Report report)
        {
            AttributeViewModels = new List<AttributeViewModel>();

            if ((report.ReportAttributes.Has(ReportAttributeOption.HospitalFilters))
                ||(report.ReportAttributes.Has(ReportAttributeOption.DRGsDischargesFilters)) 
                ||(report.ReportAttributes.Has(ReportAttributeOption.ConditionsAndDiagnosisFilters))
                )
            {
                AttributeViewModels.Add(new FiltersViewModel(report));
            }
            if ((report.ReportAttributes.Has(ReportAttributeOption.Display)))
            {
                AttributeViewModels.Add(new ProfileDisplayViewModel(report));
            }
            if (report.ReportAttributes.Has(ReportAttributeOption.KeysForRatings))
            {
                AttributeViewModels.Add(new RatingsFiltersViewModel(report));
            }
            if (report.ReportAttributes.Has(ReportAttributeOption.ReportColumns))
            {
                AttributeViewModels.Add(new ReportColumnsViewModel(report));
            }
        }
    }

    /// <summary>
    /// Class that has all the icons
    /// </summary>
    [ImplementPropertyChanged]
    public class IconSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IconSet"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="imageCollection">The image collection.</param>
        public IconSet(string name, IEnumerable<BitmapImage> imageCollection)
        {
            Name = name;
            IconCollection =new ObservableCollection<BitmapImage>(imageCollection);
        }

        /// <summary>
        /// Gets or sets the IconSet name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the icon collection.
        /// </summary>
        /// <value>
        /// The icon collection.
        /// </value>
        public ObservableCollection<BitmapImage> IconCollection { get; set; } 
    }

    /// <summary>
    /// Class for the rating filter
    /// </summary>
    /// <seealso cref="Monahrq.Reports.ViewModels.AttributeViewModel" />
    public class RatingsFiltersViewModel : AttributeViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RatingsFiltersViewModel"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        public RatingsFiltersViewModel(Report report)
            : base(report)
        {
            Header = "Comparison Keys";
            SubTitle = "Comparison Keys:";
            Description = "Select the hospital profile attributes to display for this report.";
            AttributeType = AttributeType.ComparisonKeys;
            IconSet = new ObservableCollection<IconSet>(_getComparisonKeyIcons());
            if (String.IsNullOrWhiteSpace(Report.ComparisonKeyIconSetName))
            {
                Report.ComparisonKeyIconSetName = "IconSet1";
            }

            SelectedIconSet = IconSet.FirstOrDefault(x => x.Name == Report.ComparisonKeyIconSetName);
        }

        /// <summary>
        /// The selected icon set
        /// </summary>
        private IconSet _selectedIconSet;
        /// <summary>
        /// Gets or sets the selected icon set.
        /// </summary>
        /// <value>
        /// The selected icon set.
        /// </value>
        public IconSet SelectedIconSet
        {
            get { return _selectedIconSet;}
            set { _selectedIconSet = value;
            RaisePropertyChanged(()=>SelectedIconSet);
                Report.ComparisonKeyIconSetName = SelectedIconSet.Name;
            }
        }

        /*
          Comparison Key Set collections lives inside 
       * 
          @"Domain\Reports\Data\ComparisonKeyIcons\KeySet1
          @"Domain\Reports\Data\ComparisonKeyIcons\KeySet2
          @"Domain\Reports\Data\ComparisonKeyIcons\KeySet3  and etc
            
       * ComparisonKeyIconSetName => strores the name of a KeySet
       * if(keyset is not defined in DB, KeySet1 is selected as default)
       */


        /// <summary>
        /// Gets the comparison key icons.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IconSet> _getComparisonKeyIcons()
        {
            var imageSetCollection = new List<IconSet>();
            
            var appdir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(appdir, @"Domain\Reports\Data\ComparisonKeyIcons");
            var imageDirInfo= new DirectoryInfo(path);

            foreach (var dir in imageDirInfo.GetDirectories())
            {
                var imageSetDirectoryName = dir.FullName;
                var imageSetDirectoryInfo = new DirectoryInfo(Path.Combine(path, imageSetDirectoryName));

                var imageCollection = new List<BitmapImage>();

                foreach (var file in imageSetDirectoryInfo.GetFiles())
                {
                    var fullImagePath = Path.Combine(path, file.FullName);
                    var uri = new Uri(fullImagePath, UriKind.RelativeOrAbsolute);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.Default;
                    image.UriSource = uri;
                    image.EndInit();
                    image.Freeze();
                    imageCollection.Add(image);
                }

                if (imageCollection.Any())
                {
                    imageSetCollection.Add(new IconSet(dir.Name, imageCollection));
                }
            }

            return imageSetCollection;
        }

        private ObservableCollection<IconSet> _iconSet;
        /// <summary>
        /// Gets or sets the icon set.
        /// </summary>
        /// <value>
        /// The icon set.
        /// </value>
        public ObservableCollection<IconSet> IconSet
        {
            get { return _iconSet; }
            set { _iconSet = value;

            RaisePropertyChanged(() => IconSet);
            }
        }
    }

    /*NON ENUM BASED ATTRIBUTE*/
    /// <summary>
    /// Class for report columns
    /// </summary>
    /// <seealso cref="Monahrq.Reports.ViewModels.AttributeViewModel" />
    public class ReportColumnsViewModel : AttributeViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportColumnsViewModel"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        public ReportColumnsViewModel(Report report)
            : base(report)
        {
            Header = "Report Columns";
            SubTitle="Report Columns:";
            Description = "Select the columns to display for this report.";
            AttributeType = AttributeType.Columns;
            SourceColumns = GetBasicAttributes(report.Columns.ToList(),report);
            PropertyChanged += ReportColumnsViewModel_PropertyChanged;

            foreach (var column in SourceColumns.Where(column => Report.Columns.All(x => x.Name != column.Name)))
            {
                column.IsSelected = false;
            }
        }

        /// <summary>
        /// Gets the basic attributes.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="report">The report.</param>
        /// <returns></returns>
        private ObservableCollection<ColumnAttributeClass> GetBasicAttributes(List<ReportColumn> list, Report report)
        {
            var columns = new ObservableCollection<ColumnAttributeClass>();

            foreach (var c in list)
            {
                columns.Add(new ColumnAttributeClass(c,report));
            }
            return columns;
        }

        void ReportColumnsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var x = e.PropertyName;
        }

        private ObservableCollection<ColumnAttributeClass> _sourceColumns;
        /// <summary>
        /// Gets or sets the source columns.
        /// </summary>
        /// <value>
        /// The source columns.
        /// </value>
        public ObservableCollection<ColumnAttributeClass> SourceColumns
        {
            get { return _sourceColumns; }
            set { _sourceColumns = value;
                RaisePropertyChanged(()=>SourceColumns);
            }
        }
    }

    /*Exstension method to return collection of non-enum base attributes*/
    /// <summary>
    /// Extension class for <see cref="Report"/> and contains a method to return collection of non-enum base attributes
    /// </summary>
    public static class AttributeBaseExctension
    {
        /// <summary>
        /// Gets the collection of non-enum base attributes.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static ObservableCollection<AttributeBaseItem> GetBasicAttributes(this Report item,  IEnumerable<IEntity> list)
        {
            var columns = new ObservableCollection<AttributeBaseItem>();

            foreach (var c in list)
            {
                columns.Add(new AttributeBaseItem(c));
            }
            return columns;
        }
    }

    /*BASE CLASS FOR NON-ENUM BASED ATTRIBUTES*/
    /// <summary>
    /// Base class for non-enum based attributes
    /// </summary>
    [ImplementPropertyChanged]
    public class AttributeBaseItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public IEntity Enitity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeBaseItem"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public AttributeBaseItem(IEntity item)
        {
            Name = item.Name;
            IsSelected = true;
            Enitity = item;
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
    }

    /// <summary>
    /// Class for column attributes
    /// </summary>
    /// <seealso cref="Monahrq.Reports.ViewModels.AttributeBaseItem" />
    public class  ColumnAttributeClass:AttributeBaseItem
    {
        private readonly Report _report;
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnAttributeClass"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="report">The report.</param>
        public ColumnAttributeClass(IEntity item, Report report) : base(item)
        {
            _report = report;
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged = delegate { };

        /* On Select change add or remove columns to Report.Columns*/
        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="args">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "IsSelected") return;
            if(_report==null) return;
            if (_report.Columns == null) return;

            // _report.Columns.Clear();
            if (IsSelected)
            {
                if (!_report.Columns.Any(x => x.Name.EqualsIgnoreCase(Name)))
                {
                    _report.Columns.Add(Enitity as ReportColumn);
                }
            }
            else
            {
                if (_report.Columns.Any(x => x.Name == Name))
                {
                    var columnsToRemove = _report.Columns.Where(x => x.Name.EqualsIgnoreCase(Name)).ToList();
                    columnsToRemove.ForEach(col => _report.Columns.Remove(col));
                    //_report.Columns.Remove(columnToRemove);
                    //_report.Columns.ToList().RemoveAll(x => x.Name == Name);
                }
            }
            _report.IsChanged = !_report.IsChanged;
            PropertyChanged(this, args);
        }
    }
}
