using Monahrq.Infrastructure.Entities.Domain;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Monahrq.Infrastructure.Domain.Websites
{
    [Serializable]
    [DataContract]
    [ImplementPropertyChanged]
    public class Menu : Entity<int>, ISelectable
    {
        #region Fields and Constants

        private IList<Menu> _subMenus;
        private int? _parent;
        private string _newLabel;
        private bool _isEdited;
        private string _label;

        #endregion

        #region Constructor

        public Menu()
        {
            _subMenus = new List<Menu>();
            Classes = new List<string>();
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public bool IsVisible { get; set; }

        [Required(ErrorMessage = @"Please provide text as menu items cannot be blank.")]
        [StringLength(30, ErrorMessage = @"Please use fewer than 30 characters.")]
        public string NewLabel
        {
            get { return _newLabel ?? Label; }
            set
            {
                _newLabel = value;
                Validate();
            }
        }

        [DataMember(Name = "id")]
        public override int Id { get; set; }

        [DataMember(Name = "IsSelected", Order = 0)]
        public bool IsSelected { get; set; }

        [DataMember(Name = "product", Order = 0)]
        public string Product { get; set; }

        [DataMember(Name = "menu", Order = 1)]
        public override string Name { get; set; }

        [DataMember(Name = "type", Order = 2)]
        public string Type { get; set; }

        [DataMember(Name = "parent", Order = 3)]
        public int? Parent
        {
            get { return Owner != null ? Owner.Id : _parent; }
            set { _parent = value; }
        }

        [DataMember(Name = "label", Order = 4)]
        [StringLength(30, ErrorMessage = @"Please use fewer than 30 characters.")]
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
            }
        }

        [DataMember(Name = "priority", Order = 5)]
        public int Priority { get; set; }

        [DataMember(Name = "entity", Order = 6)]
        public string Entity { get; set; }

        [DataMember(Name = "DataSets", Order = 9)]
        public List<string> DataSets { get; set; }

        [DataMember(Name = "classes", Order = 7)]
        public List<string> Classes { get; set; }

        [DataMember(Name = "route", Order = 8)]
        public virtual RouteInfo Routes { get; set; }

        [DataMember(Name = "target", Order = 3)]
        public int? Target { get; set; }

        [IgnoreDataMember]
        public virtual Menu Owner { get; set; }

        [DataMember(Name = "SubMenus", Order = 10)]
        public virtual IList<Menu> SubMenus
        {
            get { return new ReadOnlyCollection<Menu>(_subMenus); }
            private set { _subMenus = value; }
        }

        [IgnoreDataMember]
        public string ProductLabel
        {
            get { return string.Format("{0}:{1}", Product, Label); }
        }

        #endregion

        #region Methods

        public void AddSubMenu(Menu subMenu)
        {
            subMenu.Owner = this;
            _subMenus.Add(subMenu);
        }

        public void FindAllChildren(ref List<Menu> items)
        {
            foreach (var item in SubMenus)
            {
                item.FindAllChildren(ref items);
            }
            items.Add(this);
        }

        public string ToJsonString()
        {

            var result = string.Empty;
            if (Type == "menu-config")
            {
                result = string.Format(@"product: '{0}',{1}menu: '{2}',{1}type: '{3}',{1}target: '{4}',{1}entity: '{5}',{1}classes: [{6}],{1}"
                                        , Product, Environment.NewLine, Name, Type, Target, Entity, Classes.Any() ? string.Join("','", Classes) : "");

                return result;
            }

            result = string.Format(@"product: '{0}',{1}menu: '{2}',{1}type: '{3}',{1}id: '{4}',{1}parent: '{5}',{1}label: '{6}',{1}priority: '{7}',{1}entity: '{8}',{1}classes: [{9}],{1}route: [{10}],{1}"
                                    , Product, Environment.NewLine, Name, Type, Id, Parent, Label, Priority, Entity, Classes.Any() ? string.Join("','", Classes) : "", Routes.ToJsonString());

            return result;
        }

        public void SelectAll()
        {
            IsSelected = true;

            var items = new List<Menu>();

            foreach (var item in SubMenus)
            {
                item.FindAllChildren(ref items);
            }

            items.ForEach(x => x.IsSelected = true);
        }

        public void UnSelectAll()
        {
            IsSelected = false;
            var items = new List<Menu>();

            foreach (var item in SubMenus)
            {
                item.FindAllChildren(ref items);
            }

            items.ForEach(x => x.IsSelected = false);
        }

        #endregion


    }

    [Serializable]
    [DataContract]
    [ImplementPropertyChanged]
    public class RouteInfo
    {
        public RouteInfo()
        {
            ActiveName = new List<string>();
            IgnoreName = new List<string>();
            Params = new Dictionary<string, string>();
        }

        [DataMember(Name = "name", Order = 1)]
        public string Name { get; set; }

        [DataMember(Name = "activeName", Order = 2)]
        public List<string> ActiveName { get; set; }

        [DataMember(Name = "ignoreName", Order = 3)]
        public List<string> IgnoreName { get; set; }

        [DataMember(Name = "params", Order = 4)]
        public Dictionary<string, string> Params { get; set; }

        public string ToJsonString()
        {
            return string.Format("name:'{0}',{1}activeName:'{2}',ignoreName:'{6}' ,{1}params:{{3}},{1}", Name, Environment.NewLine, ActiveName.Any() ? string.Join("','", ActiveName) : ""
                , Params.Any() ? string.Join(",", Params.Select(x => string.Format("{4}:{5}", x.Key, x.Value))) : string.Empty, IgnoreName.Any() ? string.Join("','", IgnoreName) : "");
        }
    }

}
