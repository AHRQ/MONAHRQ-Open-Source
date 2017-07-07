using System.Windows.Input;

namespace Monahrq.Theme.PopupDialog
{
    public interface IPopupDialogViewModel
    {
        string Message { get; set; }
        System.Windows.Controls.Control ControlContent { get; set; }
        string Title { get; set; }
        PopupDialogButtons Buttons { get; set; }

        string OKButtonText { get; set; }
        string CancelButtonText { get; set; }
        string YesButtonText { get; set; }
        string NoButtonText { get; set; }
        string AbortButtonText { get; set; }
        string RetryButtonText { get; set; }
        string IgnoreButtonText { get; set; }

        ICommand OKCommand { get; }
        ICommand CancelCommand { get; }
        ICommand YesCommand { get; }
        ICommand NoCommand { get; }
        ICommand AbortCommand { get; }
        ICommand RetryCommand { get; }
        ICommand IgnoreCommand { get; }
    }
}
