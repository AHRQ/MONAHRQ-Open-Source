using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Theme.Controls.Wizard.Models
{
    public interface IWizardDataContextObject
    {
        bool Cancel();
        void Dispose();
        string GetName();

        bool IsCustom { get; }
        Dataset GetDatasetItem();
        Target RefreshTarget(Target targetToRefesh);
    }
}
