using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Websites.ViewModels.Publish;

namespace Monahrq.Websites.ViewModels
{
    public interface IWebsitePublishViewModel
    {
        bool WasRun {get;}
        string RunDependencyCheckButtonCaption {get; }
        bool IsDependencyCheckRunning { get; }
        void RunDependencyCheck();
        DelegateCommand RunDependencyCheckCommand { get; }
        IEnumerable<IValidationResultViewModel> DependencyCheckResults { get; }
        Visibility ResultsVisibility { get; }
    }

  
}
