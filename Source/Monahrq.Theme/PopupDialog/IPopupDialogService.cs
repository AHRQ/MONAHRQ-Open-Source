using System;

namespace Monahrq.Theme.PopupDialog
{
    public interface IPopupDialogService
    {
        string Message { get; set; }
        System.Windows.Controls.Control ControlContent { get; set; }
        string Title { get; set; }
        PopupDialogButtons Buttons { get; set; }

        void ShowMessage();
        void ShowMessage(string message);
        void ShowMessage(string message, string title);
        void ShowMessage(string message, string title, PopupDialogButtons buttons);
        PopupDialogButtons ClickedButton { get; }
        event EventHandler Closed;

        string ButtonOKText { get; set; }
        string ButtonCancelText { get; set; }
        string ButtonYesText { get; set; }
        string ButtonNoText { get; set; }
        string ButtonAbortText { get; set; }
        string ButtonRetryText { get; set; }
        string ButtonIgnoreText { get; set; }
    }
}
