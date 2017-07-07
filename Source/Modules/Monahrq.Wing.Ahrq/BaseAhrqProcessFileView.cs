using Monahrq.Infrastructure.Entities.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using Monahrq.Sdk.Extensions;
using System.Windows;
using System.ComponentModel;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    /// Ahrq process file view class.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    public class BaseAhrqProcessFileView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAhrqProcessFileView"/> class.
        /// </summary>
        [ImportingConstructor]
        public BaseAhrqProcessFileView()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            Loaded += delegate
            {
                ImportData();
            };
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        IDataImporter Model
        {
            // return the parent window DataContext as the Model for this class
            get { return DataContext as IDataImporter; }
        }

        //object sync = new object();
        //void Model_CountUpdated(object sender, ExtendedEventArgs<Action> e)
        //{

        //    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        //    {
        //      ///  Model.NotifyUi -= Model_CountUpdated;
        //        lock (sync)
        //        {
        //            try
        //            {
        //                e.Data();
        //            }
        //            finally
        //            {
        //          //      Model.NotifyUi += Model_CountUpdated;
        //            }
        //        }
        //        this.Refresh();
        //    }));
        //    //Application.Current.DoEvents();
        //}

        /// <summary>
        /// Imports the data.
        /// </summary>
        public void ImportData()
        {
            Model.StartImport();
        }
    }

    /// <summary>
    /// Data importer interface
    /// </summary>
    public interface IDataImporter
    {
        void StartImport();
    }
}
