using System;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Theme.PopupDialog
{
    [Flags]
    public enum PopupDialogButtons
    {
        None = 0x0,
        OK = 0x1,
        Cancel = 0x2,
        Yes = 0x4,
        No = 0x8,
        Abort = 0x10,
        Retry = 0x20,
        Ignore = 0x40
    }

    public class DialogButtonClickEvent : CompositePresentationEvent<PopupDialogButtons> { }
}
