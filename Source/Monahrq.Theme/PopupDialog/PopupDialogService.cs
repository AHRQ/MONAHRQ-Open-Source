using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Regions;
using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace Monahrq.Theme.PopupDialog
{
    [Export(typeof(IPopupDialogService))]
    public class PopupDialogService : IPopupDialogService
    {
        public PopupDialogButtons ClickedButton { get; private set; }

        [Import]
        IPopupDialogView PopupView { get; set; }

        [Import]
        IRegionManager RegionManager { get; set; }

        public event EventHandler Closed = delegate { };

        // Main methods
        public void ShowMessage()
        {
            ClickedButton = PopupDialogButtons.None;
            ClosePopup();
            var modalRegion = RegionManager.Regions[RegionNames.Modal];
            modalRegion.ActiveViews.ToList().ForEach(modalRegion.Deactivate);
            modalRegion.Add(PopupView);
            modalRegion.Activate(PopupView);
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<DialogButtonClickEvent>().Subscribe(ButtonClicked);
        }

        public void ShowMessage(string message)
        {
            PopupView.Model.Message = message;
            ShowMessage();
        }

        public void ShowMessage(string message, string title)
        {
            PopupView.Model.Title = title;
            PopupView.Model.Message = message;
            ShowMessage();
        }

        public void ShowMessage(string message, string title, PopupDialogButtons buttons)
        {
            PopupView.Model.Title = title;
            PopupView.Model.Message = message;
            PopupView.Model.Buttons = buttons;
            ShowMessage();
        }

        private void ButtonClicked(PopupDialogButtons button)
        {
            ClickedButton = button;
            ClosePopup();
        }

        private void ClosePopup()
        {
            var modalRegion = RegionManager.Regions[RegionNames.Modal];
            if (modalRegion.Views.Contains(PopupView))
            {
                modalRegion.Deactivate(PopupView);
                modalRegion.Remove(PopupView);
                Closed(this, EventArgs.Empty);
            }
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<DialogButtonClickEvent>().Unsubscribe(ButtonClicked);
        }

        public PopupDialogButtons Buttons
        {
            get { return PopupView.Model.Buttons; }
            set { PopupView.Model.Buttons = value; }
        }

        public string ButtonOKText
        {
            get { return PopupView.Model.OKButtonText; }
            set { PopupView.Model.OKButtonText = value; }
        }

        public string ButtonCancelText
        {
            get { return PopupView.Model.CancelButtonText; }
            set { PopupView.Model.CancelButtonText = value; }
        }

        public string ButtonYesText
        {
            get { return PopupView.Model.YesButtonText; }
            set { PopupView.Model.YesButtonText = value; }
        }

        public string ButtonNoText
        {
            get { return PopupView.Model.NoButtonText; }
            set { PopupView.Model.NoButtonText = value; }
        }

        public string ButtonAbortText
        {
            get { return PopupView.Model.AbortButtonText; }
            set { PopupView.Model.AbortButtonText = value; }
        }

        public string ButtonRetryText
        {
            get { return PopupView.Model.RetryButtonText; }
            set { PopupView.Model.RetryButtonText = value; }
        }

        public string ButtonIgnoreText
        {
            get { return PopupView.Model.IgnoreButtonText; }
            set { PopupView.Model.IgnoreButtonText = value; }
        }

        // Content
        public string Title
        {
            get { return PopupView.Model.Title; }
            set { PopupView.Model.Title = value; }
        }

        public string Message
        {
            get { return PopupView.Model.Message; }
            set { PopupView.Model.Message = value; }
        }

        public System.Windows.Controls.Control ControlContent
        {
            get { return PopupView.Model.ControlContent; }
            set { PopupView.Model.ControlContent = value; }
        }
    }
}
