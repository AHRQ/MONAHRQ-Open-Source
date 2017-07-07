using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Reports.Attributes;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Monahrq.Websites.ViewModels
{
    public enum AttributeType { Filters = 1, Display, ComparisonKeys, Columns, Audience };

    public class AttributeViewModel:BaseViewModel
    {
        public AttributeType AttributeType { get; set; }

        #region Properties

        private bool _isPresent;
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
        public string Header
        {
            get { return _header; }
            set { _header = value;
            RaisePropertyChanged(()=>Header);
            }
        }

        public override string ToString()
        {
            return Header;
        }

        private string _subTitle;
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

        public Report Report { get; protected set; }

        public AttributeViewModel(Report report)
        {
            Report = report;
            IsPresent = false;
        }

        public AttributeViewModel()
        {

        }
    }

    public static class AttributeViewModelFactory
    {
        public static AttributeCollectionSet Get(Report report)
        {
            return new AttributeCollectionSet(report);
        }
    }

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
    public abstract class AttributeViewModel<TAttribute> : AttributeViewModel
    {
        public AttributeViewModel() { }
        private IEnumerationSetCollection<TAttribute> _attributeSet;

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

        public abstract TAttribute Value
        {
            get;
            set;
        }

        private void ReconcileValue(object sender, PropertyChangedEventArgs e)
        {
            Value = AttributeSet.Value;
        }

        public AttributeViewModel(Report report)
            : base(report)
        {
            Report = report;
            IsPresent = false;
        }

        public override string ToString()
        {
            return AttributeSet.ToString();
        }
    }

    /*COMPOSITE ENUM BASED ATTRIBUTE (has multiple categories that are combine to one category on UI)*/
    public class FiltersViewModel : AttributeViewModel<ReportFilter>
    {
        public FiltersViewModel(Report report)
            : base(report)
        {
            Header = "Filters";
            SubTitle = "Filters:";
            Description = "Select the filtering options for this report.";
            AttributeSet = new FilterSetCollection(report);
            AttributeType = AttributeType.Filters;
        }

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

        public class ProfileDisplayViewModel : AttributeViewModel<ReportProfileDisplayItem>
        {
            public ProfileDisplayViewModel(Report report)
                : base(report)
            {
                AttributeType = AttributeType.Display;
                Header = "Display";
                SubTitle = "Display:";
                Description = "Select the hospital profile attributes to display for this report.";
                AttributeSet = new ReportProfileSetCollection(report.ReportProfile);
                IsPresent = true;
            }

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
    }

    public class ProfileDisplayViewModel : AttributeViewModel<ReportProfileDisplayItem>
    {
        public ProfileDisplayViewModel(Report report)
            : base(report)
        {
            AttributeType = AttributeType.Display;
            Header = "Display";
            SubTitle = "Display:";
            Description = "Select the hospital profile attributes to display for this report.";
            AttributeSet = new ReportProfileSetCollection(report.ReportProfile);
            IsPresent = true;
        }

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

    public class AudiencesViewModel : AttributeViewModel<Audience>
    {
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

        public override Audience Value
        {
            get
            {
                return Report.Audiences.Any() ? Report.Audiences[0] : Audience.None;
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

    public class  AttributeCollectionSet
    {
        public List<AttributeViewModel> AttributeViewModels { get; set; }
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
    
    [ImplementPropertyChanged]
    public class IconSet
    {
        public IconSet(string name, IEnumerable<BitmapImage> imageCollection)
        {
            Name = name;
            IconCollection =new ObservableCollection<BitmapImage>(imageCollection);
        }

        public string Name { get; set; }
        public ObservableCollection<BitmapImage> IconCollection { get; set; } 
    }

    public class RatingsFiltersViewModel : AttributeViewModel
    {
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

        private IconSet _selectedIconSet;
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
        public ObservableCollection<IconSet> IconSet
        {
            get { return _iconSet; }
            set { _iconSet = value;

            RaisePropertyChanged(() => IconSet);
            }
        }
    }
    
    /*NON ENUM BASED ATTRIBUTE*/
    public class ReportColumnsViewModel : AttributeViewModel
    {
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
        public ObservableCollection<ColumnAttributeClass> SourceColumns
        {
            get { return _sourceColumns; }
            set { _sourceColumns = value;
                RaisePropertyChanged(()=>SourceColumns);
            }
        }
    }

    /*Exstension method to return collection of non-enum base attributes*/
    public static class AttributeBaseExctension
    {
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
    [ImplementPropertyChanged]
    public class AttributeBaseItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public IEntity Enitity { get; set; }

        public AttributeBaseItem(IEntity item)
        {
            Name = item.Name;
            IsSelected = true;
            Enitity = item;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
    }

    public class  ColumnAttributeClass:AttributeBaseItem
    {
        private Report Report;
        public ColumnAttributeClass(IEntity item, Report report)
            : base(item)
        {
            Report = report;
        }

        public new event PropertyChangedEventHandler PropertyChanged = delegate { };

        /* On Select change add or remove columns to Report.Columns*/
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "IsSelected") return;
            if(Report==null) return;
            if (Report.Columns == null) return;

            if (IsSelected)
            {
                if (Report.Columns.All(x => x.Name != Name))
                {
                    Report.Columns.Add(Enitity as ReportColumn);
                }
            }
            else
            {
                if (Report.Columns.Any(x => x.Name == Name))
                {
                    var columnToRemove = Report.Columns.FirstOrDefault(x => x.Name == Name);
                    Report.Columns.Remove(columnToRemove);
                }
            }
            Report.IsChanged = !Report.IsChanged;
            PropertyChanged(this, args);
        }
    }
}
