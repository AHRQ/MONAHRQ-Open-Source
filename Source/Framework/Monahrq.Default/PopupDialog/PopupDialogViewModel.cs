using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Monahrq.Sdk.Common.PopupDialog;
using Monahrq.Theme.PopupDialog;
using PropertyChanged;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;

namespace Monahrq.Default.PopupDialog
{
    [ImplementPropertyChanged]
    [Export(typeof(IPopupDialogViewModel))]
    public class PopupDialogViewModel : IPopupDialogViewModel
    {
        [Import]
        IEventAggregator Events { get; set; }

        public PopupDialogViewModel()
        {
            Buttons = PopupDialogButtons.None;
            OKCommand = new DelegateCommand(() => Events.GetEvent<DialogButtonClickEvent>().Publish(PopupDialogButtons.OK));
            CancelCommand = new DelegateCommand(() => Events.GetEvent<DialogButtonClickEvent>().Publish(PopupDialogButtons.Cancel));
            YesCommand = new DelegateCommand(() => Events.GetEvent<DialogButtonClickEvent>().Publish(PopupDialogButtons.Yes));
            NoCommand = new DelegateCommand(() => Events.GetEvent<DialogButtonClickEvent>().Publish(PopupDialogButtons.No) );
            AbortCommand = new DelegateCommand(() =>  Events.GetEvent<DialogButtonClickEvent>().Publish(PopupDialogButtons.Abort));
            RetryCommand = new DelegateCommand(() =>  Events.GetEvent<DialogButtonClickEvent>().Publish(PopupDialogButtons.Retry));
            IgnoreCommand = new DelegateCommand(() =>  Events.GetEvent<DialogButtonClickEvent>().Publish(PopupDialogButtons.Ignore));
            OKButtonText = "OK";
            CancelButtonText = "Cancel";
            YesButtonText = "Yes";
            NoButtonText = "No";
            AbortButtonText = "Abort";
            RetryButtonText = "Retry";
            IgnoreButtonText = "Ignore";
            Buttons = PopupDialogButtons.OK | PopupDialogButtons.Cancel;
        }

        public string Message { get; set; }
        public Control ControlContent { get; set; }
        public string Title { get; set; }
        public PopupDialogButtons Buttons { get; set; }

        public string OKButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public string YesButtonText { get; set; }
        public string NoButtonText { get; set; }
        public string AbortButtonText { get; set; }
        public string RetryButtonText { get; set; }
        public string IgnoreButtonText { get; set; }
        
        public ICommand OKCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand YesCommand { get; set; }
        public ICommand NoCommand { get; set; }
        public ICommand AbortCommand { get; set; }
        public ICommand RetryCommand { get; set; }
        public ICommand IgnoreCommand { get; set; }
    }
}
