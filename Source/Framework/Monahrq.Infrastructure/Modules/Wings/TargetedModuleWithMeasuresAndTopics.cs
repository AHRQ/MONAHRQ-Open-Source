using System;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Logging;
using NHibernate.Linq;

namespace Monahrq.Sdk.Modules.Wings
{
    /// <summary>
    /// A <see cref="WingModule"/> for a <see cref="Target"/> type defined by <typeparamref name="T"/>, which defines it's own <see cref="Measure"/>s and <see cref="Topic"/>s
    /// </summary>
    /// <typeparam name="T">The <see cref="DatasetRecord"/> type that this <see cref="WingModule"/> describes</typeparam>
    public abstract class TargetedModuleWithMeasuresAndTopics<T> : TargetedModuleBase<T>
        where T : DatasetRecord
    {
        /// <summary>
        /// If measures are declared in an XML file in the filesystem, this property returns the path to that file.
        /// </summary>
        public abstract string MeasureFilePath { get; }

        /// <summary>
        /// If measure topics are declared in an XML file in the filesystem, this property returns the path to that file.
        /// </summary>
        public abstract string MeasureTopicFilePath { get; }

        /// <summary>
        /// Loads measures into the database
        /// </summary>
        protected abstract void ImportMeasures();

        /// <summary>
        /// Loads measure topics into the database
        /// </summary>
        protected virtual void ImportMeasureTopics()
        { }

        /// <inheritdoc/>
        public override bool Install()
        {
            return true;
        }

        /// <inheritdoc/>
        public override bool InstallDb()
        {
            try
            {
                this.ImportMeasures();
                this.ImportMeasureTopics();

                return true;
            }
            catch (Exception e)
            {
                this.SessionLogger.LogException(e, "Error importing measures for module '{0}'", this.Description);
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool Update()
        {
            return true;
        }

        /// <inheritdoc/>
        public override bool UpdateDb()
        {
            return true;
        }

        /// <inheritdoc/>
        public override bool Refresh()
        {
            return base.Refresh();
        }

        /// <inheritdoc/>
        public override bool RefreshDb()
        {
            using (var session = this.Provider.SessionFactory.OpenSession())
            {
                var measureFileInfo = !string.IsNullOrEmpty(this.MeasureFilePath) ? new FileInfo(this.MeasureFilePath) : null;
                var measureTopicFileInfo = !string.IsNullOrEmpty(this.MeasureTopicFilePath) ? new FileInfo(this.MeasureTopicFilePath) : null;
                var wing = session.Query<Wing>().FirstOrDefault(w => w.WingGUID == this.WingGUID);

                if (wing == null ||
                    (this.MeasureFilePath != null && measureFileInfo == null) ||
                    (this.MeasureTopicFilePath != null && measureTopicFileInfo == null))
                    return false;

                if (!wing.LastWingUpdate.HasValue ||
                    (	measureFileInfo != null &&		wing.LastWingUpdate.Value.TrimMilliseconds() < measureFileInfo.LastWriteTime.TrimMilliseconds() ||
                     	measureTopicFileInfo != null && wing.LastWingUpdate.Value.TrimMilliseconds() < measureTopicFileInfo.LastWriteTime.TrimMilliseconds()))
                {
                    if (measureFileInfo != null)		this.ImportMeasures();
                    if (measureTopicFileInfo != null)	this.ImportMeasureTopics();
                    return true;
                }
            }
            return false;
        }
    }
}