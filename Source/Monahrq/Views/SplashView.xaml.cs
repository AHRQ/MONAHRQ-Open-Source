using System.ComponentModel.Composition;
using System.Windows;
using Monahrq.Infrastructure;
using Monahrq.ViewModels;
using Microsoft.Practices.Prism.Events;
using Monahrq.Events;
using System.Windows.Threading;
using System;
using System.Diagnostics;
using System.IO;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Events;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for SplashView.xaml
    /// </summary>
    [Export(typeof(SplashView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class SplashView : IPartImportsSatisfiedNotification
    {
        #region Delegates
        public delegate void CloseWindowDelegate();
        public delegate void UpdateMessageDelegate(string message);
        #endregion

        public SplashView()
        {
            InitializeComponent();
            Loaded += OnLoaded;

        }

        SplashViewModel SplashViewModel
        {
            get
            {
                return DataContext as SplashViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        IEventAggregator Events { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplashView" /> class.
        /// </summary>
        /// <param name="splashViewModel">The splash view model.</param>
        /// <param name="events">The events.</param>
        [ImportingConstructor]
        public SplashView(SplashViewModel splashViewModel, IEventAggregator events)
        {
            InitializeComponent();
            Events = events;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = false;            // do NOT block the user from doing email while we boot-up

            //Defaults for splash screen
            DebugUiSetup();
            WindowStartupLocation = WindowStartupLocation.Manual;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;

            Loaded += OnLoaded;

            if (splashViewModel != null)
            {
                SplashViewModel = splashViewModel;

                splashViewModel.PropertyChanged += delegate
                {
                    this.Refresh();
                };
            }
        }

        [Conditional("DEBUG")]
        private void DebugUiSetup()
        {
            ShowInTaskbar = false;
            Topmost = false;
        }

        /// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PumpThread();
            //calculate it manually since CenterScreen substracts  
            //taskbar's height from available area
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
            Events.GetEvent<CloseSplashEvent>().Subscribe(p => Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                                                                                        new Action(Close)));
            Events.GetEvent<MessageUpdateEvent>().Subscribe(
                 mue =>
                 {
                     Action action = () =>
                         {
                             SplashViewModel.Status = mue.Message;
                             Application.Current.DoEvents();
                             this.Refresh();
                         };
                     Dispatcher.Invoke(DispatcherPriority.Render, action);
                     Thread.Sleep(0);
                     this.Refresh();
                 });

            Events.GetEvent<UiMessageUpdateEventForeGround>().Subscribe(
               mue =>
               {
                   Action action = () =>
                   {
                       SplashViewModel.Status = mue.Message;
                       Application.Current.DoEvents();
                       this.Refresh();
                   };
                   Dispatcher.Invoke(DispatcherPriority.Render, action);
                   Thread.Sleep(0);
                   this.Refresh();
               }, ThreadOption.UIThread);
        }

        private void PumpThread()
        {
            Application.Current.DoEvents();
            Action act = () =>
            {
                    Thread.Sleep(1000);
                    if (IsVisible)
                    {
                        PumpThread();
                    }
            };
            act.BeginInvoke(null, null);
            Thread.Sleep(0);
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {

        }
    }
}
