using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Monahrq.Default.ViewModels;
using Monahrq.Default.Views;
using Monahrq.Sdk.Events;
using System.Diagnostics;
using Monahrq.Infrastructure;


namespace Monahrq.Default.Controllers
{

    /// <summary>
    /// class for UI event handler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DefaultCompositeUIEventHandler<T>
    {
        /// <summary>
        /// Handles the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        public abstract void Handle(T result);
    }

    /// <summary>
    /// class for import complete
    /// </summary>
    /// <seealso cref="Monahrq.Default.Controllers.DefaultCompositeUIEventHandler{Monahrq.Sdk.Events.ISimpleImportCompletedPayload}" />
    /// <seealso cref="Monahrq.Sdk.Events.ISimpleImportCompleteHandler" />
    [Export(typeof(ISimpleImportCompleteHandler)), PartCreationPolicy(CreationPolicy.Shared)]
    public class SimpleImportCompleteHandler : DefaultCompositeUIEventHandler<ISimpleImportCompletedPayload>, 
        ISimpleImportCompleteHandler
    {
        [Import(LogNames.Session)]
        public ILogWriter Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleImportCompleteHandler"/> class.
        /// </summary>
        /// <param name="events">The events.</param>
        [ImportingConstructor]
        public SimpleImportCompleteHandler(IEventAggregator events)
        {
            events.GetEvent<SimpleImportCompletedEvent>().Subscribe(Handle);
        }

        /// <summary>
        /// Handles the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        public override void Handle(ISimpleImportCompletedPayload result)
        {
            this.Logger.Information($@"Simple import completed for {result.Description}; {result.CountInserted} rows inserted, {result.NumberOfErrors} errors encountered");

            if (result.NumberOfErrors == 0 && result.CountInserted > 0)
            {
                MessageBox.Show(Application.Current.MainWindow,
                    string.Format("File successfully imported. \n{0} lines read with zero errors.", result.CountInserted),
                    result.Description, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (result.NumberOfErrors > 0)
            {
                string message;

                if (result.CountInserted > 0)
                {
                    message = string.Format("File import complete with partial success.\n{0} of {1} lines imported.\nWould you like to view the import log?",
                            result.CountInserted,
                            result.NumberOfErrors + result.CountInserted);
                }
                else
                {
                    message = string.Format("File import failed to import {0} line(s).\nWould you like to view the import log?", result.NumberOfErrors);
                }

                if (MessageBox.Show(Application.Current.MainWindow, message, result.Description, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Present(result);
                }
            }
        }

        /// <summary>
        /// Presents the specified results.
        /// </summary>
        /// <param name="results">The results.</param>
        private void Present(ISimpleImportCompletedPayload results)
        {
            // just use Notepad so the user can easily read the text (and it's familiar and has more functionality)
            try
            {
                var p = new Process();
                p.StartInfo.FileName = results.ErrorFile;
                p.StartInfo.Arguments = string.Format("Import Errors");
                p.StartInfo.CreateNoWindow = false;
                p.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error displaying logfile", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            //var dlg = new SimpleImportResultsDialog(results);
            //dlg.Title = results.Description;
            //dlg.Owner = Application.Current.MainWindow;
            //dlg.ShowDialog();
        }
    }
}
