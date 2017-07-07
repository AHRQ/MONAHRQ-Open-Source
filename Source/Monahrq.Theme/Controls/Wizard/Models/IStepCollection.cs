using System;
using System.Collections;
using System.Collections.Generic;
namespace Monahrq.Theme.Controls.Wizard.Models
{

    public interface IStepCollection
    {
        Type ContextType { get; }
        IDictionary Collection { get; }
    }

    public interface IStepCollection<out TValue>: IStepCollection
    {
        TValue Context { get; }
    }
}
