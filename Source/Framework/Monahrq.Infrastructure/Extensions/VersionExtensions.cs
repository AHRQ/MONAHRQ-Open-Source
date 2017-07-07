using System;

namespace Monahrq.Infrastructure.Extensions
{
    public static class VersionExtensions
    {
        public static UpgradeTypeEnum CheckForUpdate(this Version version, Version versiontoCompare)
        {
            if (version.Major.CompareTo(versiontoCompare.Major) != 0)
                return version.Major.CompareTo(versiontoCompare.Major) > 0
                    ? UpgradeTypeEnum.Major
                    : UpgradeTypeEnum.None;

            if (version.Minor.CompareTo(versiontoCompare.Minor) != 0 || version.Build.CompareTo(versiontoCompare.Build) != 0)
                return version.Minor.CompareTo(versiontoCompare.Minor) > 0 || version.Build.CompareTo(versiontoCompare.Build) > 0
                    ? UpgradeTypeEnum.Minor
                    : UpgradeTypeEnum.None;

            if (version.Revision != versiontoCompare.Revision)
                return version.Revision.CompareTo(versiontoCompare.Revision) > 0
                    ? UpgradeTypeEnum.Minor
                    : UpgradeTypeEnum.None;

            //if (this.beta != o.beta)
            //{
            //    return Integer.compare(this.beta, o.beta);
            //}
            return UpgradeTypeEnum.None;
        }
    }

    public enum UpgradeTypeEnum
    {
        None,
        Major,
        Minor
    }
}
