using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Monahrq.Theme.Styles
{
    public class ModalPopupStyleProvider : Style
    {
        public ModalPopupStyleProvider()
        {
            var setter = new Setter(FrameworkElement.WidthProperty, 100);
        }

    }
}
