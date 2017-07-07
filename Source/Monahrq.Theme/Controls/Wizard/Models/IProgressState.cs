using System;
namespace Monahrq.Theme.Controls.Wizard.Models
{
    public interface IProgressState
    {
        int Current { get; }
        double Ratio { get; }
        int Total { get; }
    }
}
