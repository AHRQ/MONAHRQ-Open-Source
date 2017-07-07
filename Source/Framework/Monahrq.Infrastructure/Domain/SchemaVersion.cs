using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Domain
{
    [ImplementPropertyChanged, Serializable]
    [EntityTableName("SchemaVersions")]
    public class SchemaVersion : Entity<int>
    {
        public string Version { get; set; }

        public int Major
        {
            get
            {
                string[] version = Version.Split('.');
                if (version.Length < 1) return 0;
                int schemaMajor;
                int.TryParse(version[0], out schemaMajor);
                return schemaMajor;
            }
        }

        public int Minor
        {
            get
            {
                string[] version = Version.Split('.');
                if (version.Length < 2) return 0;
                int schemaMinor;
                int.TryParse(version[1], out schemaMinor);
                return schemaMinor;
            }
        }

        public int Milestone
        {
            get
            {
                string[] version = Version.Split('.');
                if (version.Length < 3) return 0;
                int schemaMilestone;
                int.TryParse(version[2], out schemaMilestone);
                return schemaMilestone;
            }
        }

        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime ActiveDate { get; set; }
        public VersionType VersionType { get; set; }
        public string FileName { get; set; }
    }

    public enum VersionType
    {
        Default,
        MonthAndYear,
        YearOnly
    }
}
