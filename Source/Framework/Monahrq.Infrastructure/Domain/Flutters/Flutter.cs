using System;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Flutters
{
    [Serializable]
    [ImplementPropertyChanged]
    [EntityTableName("Wings_FlutterRegistry")]
    public class Flutter : Entity<int>
    {
        //public virtual Guid FlutterGUID { get; set; }

        /// <summary>
        /// Gets or sets the associated reports types.
        /// </summary>
        /// <value>
        /// The associated reports types.
        /// </value>
        public virtual string AssociatedReportsTypes { get; set; }
        /// <summary>
        /// Gets or sets the flutter path.
        /// </summary>
        /// <value>
        /// The flutter path.
        /// </value>
        public virtual string InstallPath { get; set; }
        /// <summary>
        /// Gets or sets the flutter output path.
        /// </summary>
        /// <value>
        /// The flutter output path.
        /// </value>
        public virtual string OutputPath { get; set; }
        /// <summary>
        /// Gets or sets the configuration identifier.
        /// </summary>
        /// <value>
        /// The configuration identifier.
        /// </value>
        public virtual string ConfigurationId { get; set; }

        public override void CleanBeforeSave()
        {
            base.CleanBeforeSave();

            if (!string.IsNullOrEmpty(Name) && Name.ContainsIgnoreCase("Flutter"))
            {
                Name = Name.Replace("Flutter", null);
            }
        }
    }
}
