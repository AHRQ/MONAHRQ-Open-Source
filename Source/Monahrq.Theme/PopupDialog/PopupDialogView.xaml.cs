﻿using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using PropertyChanged;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Monahrq.Theme.PopupDialog
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    [ImplementPropertyChanged]
    [ViewExport(typeof(IPopupDialogView), RegionName = RegionNames.Modal)]
    public partial class PopupDialogView : UserControl, IPopupDialogView
    {
       
        public PopupDialogView()
        {
            InitializeComponent();
        }

        [Import]
        public IPopupDialogViewModel Model 
        {
            get { return DataContext as IPopupDialogViewModel; }
            private set { DataContext = value;  }
        }

        public void Connect(int connectionId, object target)
        {
            //throw new System.NotImplementedException();
        }
    }
}
