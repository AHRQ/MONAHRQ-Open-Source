using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Monahrq.Default.PopupDialog

{
    [Export(typeof(IPopupDialog))]
    public class PopupDialogController : IPopupDialog
    {
        PopupDialogViewModel PopupViewModel { get; set; }

        public PopupDialogController()
        {
            PopupViewModel = ServiceLocator.Current.GetInstance<PopupDialogViewModel>();
        }


        // Main methods
        public void ShowMessage()
        {
            PopupViewModel.ShowMessage();
        }

        public void ShowMessage(string message)
        {
            PopupViewModel.Message = message;
            PopupViewModel.ShowMessage();
        }

        public void ShowMessage(string message, string title)
        {
            PopupViewModel.Title = title;
            PopupViewModel.Message = message;
            PopupViewModel.ShowMessage();
        }

        public void ShowMessage(string message, string title, PopupDialogEnum buttons)
        {
            PopupViewModel.Title = title;
            PopupViewModel.Message = message;
            SetButtons(buttons);
            PopupViewModel.ShowMessage();
        }

        public void ShowControlContent()
        {
            throw new NotImplementedException();
        }

        public void ShowControlContent(Control ControlContent)
        {
            throw new NotImplementedException();
        }

        public void ShowControlContent(Control ControlContent, string Title)
        {
            throw new NotImplementedException();
        }

        public void ShowControlContent(Control ControlContent, string Title, PopupDialogEnum Buttons)
        {
            throw new NotImplementedException();
        }

        public void ClosePopup()
        {
            throw new NotImplementedException();
        }


        // Buttons
        private void SetButtons(PopupDialogEnum buttons)
        {
            PopupViewModel.ShowOKButton = (buttons & PopupDialogEnum.OK) == PopupDialogEnum.OK;
            PopupViewModel.ShowCancelButton = (buttons & PopupDialogEnum.Cancel) == PopupDialogEnum.Cancel;
            PopupViewModel.ShowYesButton = (buttons & PopupDialogEnum.Yes) == PopupDialogEnum.Yes;
            PopupViewModel.ShowNoButton = (buttons & PopupDialogEnum.No) == PopupDialogEnum.No;
            PopupViewModel.ShowAbortButton = (buttons & PopupDialogEnum.Abort) == PopupDialogEnum.Abort;
            PopupViewModel.ShowRetryButton = (buttons & PopupDialogEnum.Retry) == PopupDialogEnum.Retry;
            PopupViewModel.ShowIgnoreButton = (buttons & PopupDialogEnum.Ignore) == PopupDialogEnum.Ignore;
        }

        private PopupDialogEnum GetButtons()
        {
            return (ShowOKButton ? PopupDialogEnum.OK : PopupDialogEnum.None) |
                (ShowCancelButton ? PopupDialogEnum.Cancel : PopupDialogEnum.None) |
                (ShowYesButton ? PopupDialogEnum.Yes : PopupDialogEnum.None) |
                (ShowNoButton ? PopupDialogEnum.No : PopupDialogEnum.None) |
                (ShowAbortButton ? PopupDialogEnum.Abort : PopupDialogEnum.None) |
                (ShowRetryButton ? PopupDialogEnum.Retry : PopupDialogEnum.None) |
                (ShowIgnoreButton ? PopupDialogEnum.Ignore : PopupDialogEnum.None);
        }


        public PopupDialogEnum Buttons
        {
            get { return GetButtons(); }
            set { SetButtons(value); }
        }

        public bool ShowOKButton
        {
            get { return PopupViewModel.ShowOKButton; }
            set { PopupViewModel.ShowOKButton = value; }
        }

        public string ButtonOKText
        {
            get { return PopupViewModel.OKButtonText;  }
            set { PopupViewModel.OKButtonText = value; }
        }

        public bool ShowCancelButton
        {
            get { return PopupViewModel.ShowCancelButton; }
            set { PopupViewModel.ShowCancelButton = value; }
        }

        public string ButtonCancelText
        {
            get { return PopupViewModel.CancelButtonText; }
            set { PopupViewModel.CancelButtonText = value; }
        }

        public bool ShowYesButton
        {
            get { return PopupViewModel.ShowYesButton; }
            set { PopupViewModel.ShowYesButton = value; }
        }

        public string ButtonYesText
        {
            get { return PopupViewModel.YesButtonText; }
            set { PopupViewModel.YesButtonText = value; }
        }

        public bool ShowNoButton
        {
            get { return PopupViewModel.ShowNoButton; }
            set { PopupViewModel.ShowNoButton = value; }
        }

        public string ButtonNoText
        {
            get { return PopupViewModel.NoButtonText; }
            set { PopupViewModel.NoButtonText = value; }
        }

        public bool ShowAbortButton
        {
            get { return PopupViewModel.ShowAbortButton; }
            set { PopupViewModel.ShowAbortButton = value; }
        }

        public string ButtonAbortText
        {
            get { return PopupViewModel.AbortButtonText; }
            set { PopupViewModel.AbortButtonText = value; }
        }

        public bool ShowRetryButton
        {
            get { return PopupViewModel.ShowRetryButton; }
            set { PopupViewModel.ShowRetryButton = value; }
        }

        public string ButtonRetryText
        {
            get { return PopupViewModel.RetryButtonText; }
            set { PopupViewModel.RetryButtonText = value; }
        }

        public bool ShowIgnoreButton
        {
            get { return PopupViewModel.ShowIgnoreButton; }
            set { PopupViewModel.ShowIgnoreButton = value; }
        }

        public string ButtonIgnoreText
        {
            get { return PopupViewModel.IgnoreButtonText; }
            set { PopupViewModel.IgnoreButtonText = value; }
        }


        // Content
        public string Title
        {
            get { return PopupViewModel.Title; }
            set { PopupViewModel.Title = value; }
        }

        public string Message
        {
            get { return PopupViewModel.Message; }
            set { PopupViewModel.Message = value; }
        }

        public Control ControlContent
        {
            get { return PopupViewModel.ControlContent; }
            set { PopupViewModel.ControlContent = value; }
        }
    }
}