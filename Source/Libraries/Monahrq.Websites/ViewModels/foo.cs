using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Websites.ViewModels
{
    [Export(typeof(Ifoo))]
    public class foo : Ifoo
    {
        public Bar bar { get; private set; }

        [Import]
        Bar Ifoo.bar
        {
            get
            {
                return this.bar;
            }
            set
            {
                this.bar = value;
            }
        }
    }

}
