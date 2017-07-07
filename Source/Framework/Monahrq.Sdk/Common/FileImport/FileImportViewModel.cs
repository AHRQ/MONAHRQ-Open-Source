using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Sdk.Services.Import;

namespace Monahrq.Sdk.Common.FileImport
{
    [Export]
    public class FileImportViewModel: IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets or sets the hospital importer.
        /// </summary>
        /// <value>
        /// The hospital importer.
        /// </value>
        [Import(ImporterContract.Hospital, typeof(IEntityFileImporter))]
        public IEntityFileImporter HospitalImporter { get; set; }

        /// <summary>
        /// Gets or sets the region importer.
        /// </summary>
        /// <value>
        /// The region importer.
        /// </value>
        [Import(ImporterContract.CustomRegion, typeof(IEntityFileImporter))]
        public IEntityFileImporter RegionImporter { get; set; }

        /// <summary>
        /// Gets or sets the import regions command.
        /// </summary>
        /// <value>
        /// The import regions command.
        /// </value>
        public ICommand ImportRegionsCommand { get; set; }
        /// <summary>
        /// Gets or sets the import hospitals command.
        /// </summary>
        /// <value>
        /// The import hospitals command.
        /// </value>
        public ICommand ImportHospitalsCommand { get; set; }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            ImportHospitalsCommand = new DelegateCommand(() => HospitalImporter.Execute());
            ImportRegionsCommand = new DelegateCommand(() => RegionImporter.Execute());
       }
    }
}
