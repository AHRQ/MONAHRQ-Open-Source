using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.ViewModels
{
    [ImplementPropertyChanged]
    [Export]
    public class PopupDialogViewModel
    {
        [ImportingConstructor]
        public PopupDialogViewModel()
        {
            
        }

        public string Message { get; set; }

    }
}
