using System;
using System.Windows.Input;
using Monahrq.Websites.Events;

namespace Monahrq.Websites.ViewModels.Publish
{
    public interface IValidationResultViewModel
    {
        string Message { get; }
        ValidationLevel Quality { get; }
        ValidationOutcome Result { get;  }
        WebsiteTabViewModels CompositionArea { get; }
    }
}
