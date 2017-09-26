using System.Linq;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Monahrq.Infrastructure.Extensions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The abstract/base base data version strategy.
    /// </summary>
    public abstract class BaseDataVersionStrategy
    {
        #region Contructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataVersionStrategy" /> class.
        /// </summary>
        protected BaseDataVersionStrategy() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataVersionStrategy" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="sessionFactoryProvider">The session factory provider.</param>
        /// <param name="entityType">Type of the entity.</param>
        [ImportingConstructor]
        protected BaseDataVersionStrategy(
            [Import(LogNames.Session)] ILogWriter logger,
            IDomainSessionFactoryProvider sessionFactoryProvider,
            Type entityType)
        {
            EntityType = entityType;
            DataProvider = sessionFactoryProvider;
            Logger = logger;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        /// <remarks>
        /// Note: EntityType will refer to the basedata table / entity name.
        /// Filename will refer to the base filename that is being imported.
        /// In some cases, there may be multiple files being imported to the database for the same version.
        /// For example, HRR and HSA both get imported to the same basedata table.
        /// </remarks>
        public Type EntityType { get; set; }
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public abstract SchemaVersion Version { get; }
        /// <summary>
        /// Gets or sets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        protected IDomainSessionFactoryProvider DataProvider { get; set; }
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogWriter Logger { get; set; }
        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public abstract string Filename { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether this instance is newest.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is newest; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool IsNewest();
        /// <summary>
        /// Determines whether this instance is loaded.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool IsLoaded();
        /// <summary>
        /// Gets the version based off of the version strategy.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public abstract SchemaVersion GetVersion(ISession session = null);
        #endregion
    }

    /// <summary>
    /// The default base data version strategy
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataVersionStrategy" />
    public class DefaultBaseDataVersionStrategy : BaseDataVersionStrategy
    {
        /// <summary>
        /// The filename
        /// </summary>
        private string _filename;

        #region Contructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBaseDataVersionStrategy" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="sessionFactoryProvider">The session factory provider.</param>
        /// <param name="entityType">Type of the entity.</param>
        [ImportingConstructor]
        public DefaultBaseDataVersionStrategy(
            [Import(LogNames.Session)] ILogWriter logger,
            IDomainSessionFactoryProvider sessionFactoryProvider,
            Type entityType)
            : base(logger, sessionFactoryProvider, entityType)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the major version.
        /// </summary>
        /// <value>
        /// The major.
        /// </value>
        public int? Major { get; set; }
        /// <summary>
        /// Gets or sets the minor version.
        /// </summary>
        /// <value>
        /// The minor.
        /// </value>
        public int? Minor { get; set; }
        /// <summary>
        /// Gets or sets the milestone version.
        /// </summary>
        /// <value>
        /// The milestone.
        /// </value>
        public int? Milestone { get; set; }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public override string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                _filename = Path.GetFileNameWithoutExtension(value);

                // Figure out the version from the filename if possible.
                // Assumes filename-major-minor-milestone.csv
                string s = _filename;
                if (s.IndexOf("-") > 0) s = s.Substring(s.IndexOf("-") + 1);
                string[] version = s.Split('-');
                int major = 0;
                int minor = 0;
                int milestone = 0;
                if (version.Length >= 1) int.TryParse(version[0], out major);
                Major = major;
                if (version.Length >= 2) int.TryParse(version[1], out minor);
                Minor = minor;
                if (version.Length >= 3) int.TryParse(version[2], out milestone);
                Milestone = milestone;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public override SchemaVersion Version
        {
            get
            {
                return new SchemaVersion
                {
                    Name = EntityType.EntityTableName(),
                    VersionType = VersionType.Default,
                    Version = string.Format("{0}.{1}.{2}", Major, Minor, Milestone),
                    FileName = Filename,
                    ActiveDate = DateTime.Now
                };
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether this instance is loaded.
        /// </summary>
        /// <returns></returns>
        public override bool IsLoaded()
        {
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                var schemas = (List<SchemaVersion>)session.QueryOver<SchemaVersion>()
                    .Where(x => x.Name == EntityType.EntityTableName())
                    .Where(x => x.FileName == Filename)
                    .Where(x => x.Version == string.Format("{0}.{1}.{2}", Major, Minor, Milestone))
                    .List<SchemaVersion>() ?? new List<SchemaVersion>();
                return schemas.Count >= 1;
            }
        }

        /// <summary>
        /// Determines whether this instance is newest.
        /// </summary>
        /// <returns></returns>
        public override bool IsNewest()
        {
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                var schema = session.QueryOver<SchemaVersion>()
                    .Where(x => x.Name == EntityType.EntityTableName())
                    .Where(x => x.FileName == Filename)
                    .OrderBy(x => x.Major).Desc
                    .OrderBy(x => x.Minor).Desc
                    .OrderBy(x => x.Milestone).Desc
                    .SingleOrDefault();
                if (schema == null || schema.Version == null || schema.Version == string.Empty || Major == null || Minor == null || Milestone == null)
                {
                    return true;
                }

                return ((Major > schema.Major) ||
                        (Major == schema.Major && Minor > schema.Minor) ||
                        (Major == schema.Major && Minor == schema.Minor && Milestone >= schema.Milestone));
            }
        }

        /// <summary>
        /// Gets the version based off of the version strategy.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public override SchemaVersion GetVersion(ISession session = null)
        {
            SchemaVersion version;
            var tableName = EntityType.EntityTableName();

            var internalSession = session ?? DataProvider.SessionFactory.OpenSession();

            if (session != null)
            {
                version = QueryForSchemaVersion(internalSession, tableName);
            }
            else
            {
                using (internalSession)
                {
                    version = QueryForSchemaVersion(internalSession, tableName);
                }
            }

            if (version == null)
                version = Version;
            else
            {
                version.Version = Version.Version;
                version.ActiveDate = Version.ActiveDate;
				version.FileName = Version.FileName;
			}

            return version;
        }

        /// <summary>
        /// Queries the SchemaVersion Db table for the latest version entry for the entity.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private SchemaVersion QueryForSchemaVersion(ISession session, string tableName, string fileName = null)
        {
            var criteria = session.CreateCriteria<SchemaVersion>()
                          .Add(Restrictions.Eq("Name", tableName));

            if (!string.IsNullOrEmpty(fileName))
            {
                criteria = criteria.Add(Restrictions.InsensitiveLike("FileName", fileName, MatchMode.Start));
            }

            return criteria.AddOrder(new Order("ActiveDate", false))
                          .SetMaxResults(1)
                          .UniqueResult<SchemaVersion>();
        }
        #endregion
    }

    /// <summary>
    /// The month and year baase data version strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataVersionStrategy" />
    public class MonthAndYearBaseDataVersionStrategy : BaseDataVersionStrategy
    {
        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        /// <value>
        /// The month.
        /// </value>
        public int? Month { get; set; }
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public int? Year { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthAndYearBaseDataVersionStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="sessionFactoryProvider">The session factory provider.</param>
        /// <param name="entityType">Type of the entity.</param>
        [ImportingConstructor]
        public MonthAndYearBaseDataVersionStrategy(
            [Import(LogNames.Session)] ILogWriter logger,
            IDomainSessionFactoryProvider sessionFactoryProvider,
            Type entityType)
            : base(logger, sessionFactoryProvider, entityType)
        { }

        /// <summary>
        /// Determines whether this instance is loaded.
        /// </summary>
        /// <returns></returns>
        public override bool IsLoaded()
        {
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                var schemas = session.QueryOver<SchemaVersion>()
                                     .Where(x => x.Name == EntityType.EntityTableName())
                                     .Where(x => x.FileName == Filename)
                                     .Where(x => x.Year == Year)
                                     .Where(x => x.Month == Month)
                                     .List<SchemaVersion>().ToList();

                return schemas.Count >= 1;
            }
        }

        /// <summary>
        /// Gets the version based off of the version strategy.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public override SchemaVersion GetVersion(ISession session = null)
        {
            return Version;
        }

        /// <summary>
        /// Determines whether this instance is newest.
        /// </summary>
        /// <returns></returns>
        public override bool IsNewest()
        {
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                var schema = session.QueryOver<SchemaVersion>()
                    .Where(x => x.Name == EntityType.EntityTableName())
                    .Where(x => x.FileName == Filename)
                    .OrderBy(x => x.Year).Desc
                    .OrderBy(x => x.Month).Desc
                    .SingleOrDefault();

                if (schema == null || schema.Year == null || schema.Month != null || Year != null || Month == null)
                {
                    return true;
                }
                else
                {
                    return ((Year > schema.Year) || (Year == schema.Year && Month >= schema.Month));
                }
            }
        }

        /// <summary>
        /// The filename
        /// </summary>
        private string _filename;
        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public override string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                _filename = Path.GetFileNameWithoutExtension(value);

                // Figure out the version from the filename if possible.
                // Assumes filename-yyyy-mm.csv
                string s = _filename;
                if (s.IndexOf("-") > 0) s = s.Substring(s.IndexOf("-") + 1);
                string[] version = s.Split('.');
                int year = 0;
                int month = 0;
                if (version.Length >= 1) int.TryParse(version[0], out year);
                Year = year;
                if (version.Length >= 2) int.TryParse(version[1], out month);
                Month = month;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public override SchemaVersion Version
        {
            get
            {
                return new SchemaVersion
                {
                    Name = EntityType.EntityTableName(),
                    VersionType = VersionType.MonthAndYear,
                    Version = "",
                    Year = (this.Year.HasValue ? this.Year.Value : (int?)null),
                    Month = (this.Month.HasValue ? this.Month.Value : (int?)null),
                    FileName = Filename,
                    ActiveDate = DateTime.Now
                };
            }
        }
    }

    /// <summary>
    /// They year only base data version strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataVersionStrategy" />
    public class YearOnlyBaseDataVersionStrategy : BaseDataVersionStrategy
    {
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is replaced.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is replaced; otherwise, <c>false</c>.
        /// </value>
        public bool IsReplaced { get; set; }

        /// <summary>
        /// The old version
        /// </summary>
        private SchemaVersion _oldVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="YearOnlyBaseDataVersionStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="sessionFactoryProvider">The session factory provider.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="isReplaced">if set to <c>true</c> [is replaced].</param>
        [ImportingConstructor]
        public YearOnlyBaseDataVersionStrategy(
            [Import(LogNames.Session)] ILogWriter logger,
            IDomainSessionFactoryProvider sessionFactoryProvider,
            Type entityType, bool isReplaced = false)
            : base(logger, sessionFactoryProvider, entityType)
        {
            IsReplaced = isReplaced;
        }

        /// <summary>
        /// Determines whether this instance is loaded.
        /// </summary>
        /// <returns></returns>
        public override bool IsLoaded()
        {
            try
            {
                using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                {
                    return session.Query<SchemaVersion>()
                                  .Count(x => x.Name == EntityType.EntityTableName() && 
                                         x.FileName.Equals(Filename) && 
                                         x.Year == Year) >= 1;
                }
            }
            catch (Exception ex)
            {
                throw;
                //return false;
            }
        }

        /// <summary>
        /// Determines whether this instance is newest.
        /// </summary>
        /// <returns></returns>
        public override bool IsNewest()
        {
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                var schema = session.Query<SchemaVersion>()
                                    .Where(x => x.Name == EntityType.EntityTableName() && x.FileName.Equals(Filename))
                                    .OrderByDescending(x => x.Year)
                                    .SingleOrDefault();

                return schema == null || schema.Year == null || Year != null || Year >= schema.Year;
            }
        }

        /// <summary>
        /// Gets the version based off of the version strategy.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public override SchemaVersion GetVersion(ISession session = null)
        {
            var fileName = Filename.SubStrBefore("-");
            var schema = session.Query<SchemaVersion>()
                                .Where(x => x.Name == EntityType.EntityTableName() && x.FileName.StartsWith(fileName))
                                .OrderByDescending(x => x.Year)
                                .FirstOrDefault();

            if (schema == null || !IsReplaced)
                return Version;

            schema.Year = Version.Year;
            schema.FileName = Version.FileName;
            schema.ActiveDate = Version.ActiveDate;
            return schema;
        }

        /// <summary>
        /// The filename
        /// </summary>
        private string _filename;
        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public override string Filename
        {
            get { return _filename; }

            set
            {
                _filename = Path.GetFileNameWithoutExtension(value);

                // Figure out the version from the filename if possible.
                // Assumes filename-yyyy.csv
                string s = _filename;
                if (s.IndexOf("-") > 0) s = s.Substring(s.IndexOf("-") + 1);
                int year = 0;
                int.TryParse(s, out year);
                Year = year;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public override SchemaVersion Version
        {
            get
            {
                return new SchemaVersion
                    {
                        Name = EntityType.EntityTableName(),
                        VersionType = VersionType.YearOnly,
                        Version = "",
                        Year = (Year.HasValue ? Year.Value : (int?)null),
                        FileName = Filename,
                        ActiveDate = DateTime.Now
                    };
            }
        }
    }
}
