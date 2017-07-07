using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Monahrq.Websites.ViewModels
{
    interface Ifoo
    {
        Bar bar { get; set; }

    }

[Export]
public class Bar
{
    
}
}
