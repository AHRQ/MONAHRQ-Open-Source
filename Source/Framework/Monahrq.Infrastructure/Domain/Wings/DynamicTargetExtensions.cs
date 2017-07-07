using System;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Infrastructure.Extensions;
using NHibernate;
using NHibernate.Linq;

namespace Monahrq.Infrastructure.Domain.Wings
{
    public static class DynamicWingExtensions
    {
        public static Target CreateTarget(
            this DynamicTarget target, Wing wing)
        {
            var result = TargetRepository.New(wing, target.Id, target.Name);
            if(result.Guid == Guid.Empty)
                result.Guid = Guid.NewGuid();

            result.Description = target.Description;
            result.ClrType = null;
            result.IsReferenceTarget = false;
            result.DisplayOrder = target.DisplayOrder;
            result.IsCustom = true;
            result.DbSchemaName = target.DbSchemaName;
            result.CreateSqlScript = target.CreateTableScript;
            result.ImportSQLScript = target.ImportSQLScript;
            result.AddMeausersSqlScript = target.AddMeasuresScript;
            result.AddReportsSqlScript = target.AddReportsScript;
            result.AllowMultipleImports = target.AllowMultipleImports;
            result.ImportType = target.ImportSteps.Type;

            result.Publisher = target.Publisher;
            result.PublisherEmail = target.PublisherEmail;
            result.PublisherWebsite = target.PublisherWebsite;
            result.Version = new Version { Number = target.Version };
            result.IsDisabled = target.IsDisabled;
            result.WingTargetXmlFilePath = target.WingTargetXmlFilePath;
            result.TemplateFileName = target.TempateFileName;

            if (wing.Targets.All(t => t.Guid != result.Guid || !t.Name.EqualsIgnoreCase(result.Name)))
                wing.Targets.Add(result);

            return result;
        }

        //public static Infrastructure.Entities.Domain.Wings.Wing FactoryWingFromDymanicWing(this WingTarget target,)
        //{
        //     var wing = new Infrastructure.Entities.Domain.Wings.Wing(dynamicWing.Name)
        //         {
        //             WingGUID = dynamicWing.Id == null || dynamicWing.Id == Guid.Empty ? new Guid() : dynamicWing.Id,
        //             Description = dynamicWing.Description,
        //             Publisher = dynamicWing.Publisher,
        //             PublisherEmail = dynamicWing.PublisherEmail,
        //             PublisherWebsite = dynamicWing.PublisherWebsite,
        //             Version = new Infrastructure.Domain.Version { Number = dynamicWing.Version },
        //             IsDisabled = dynamicWing.IsDisabled
        //         };

        //     return wing;
        //}

        /// <summary>
        /// Loads the custom scopes.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public static Target LoadCustomScopes(this Target target, DynamicTarget dynamicTarget)
        {
            if (target.Scopes == null) target.Scopes = new List<Scope>();

            foreach (var column in dynamicTarget.Columns)
            {
                var scope = CreateScope(target, column);

                if (scope == null) continue;


                if (!target.Scopes.Any(s => s.Name.EqualsIgnoreCase(scope.Name)))
                    target.Scopes.Add(scope);
            }

            return target;
        }

        /// <summary>
        /// Creates the scope.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        private static Scope CreateScope(Target target, DynamicTargetColumn column)
        {
            if (column == null || column.Scope == DynamicScopeEnum.None) return default(Scope);

            Scope scope;

            switch (column.Scope)
            {
                case DynamicScopeEnum.Race:
                    scope = new Scope(target, column.Name, typeof(Race));
                    break;
                case DynamicScopeEnum.PrimaryPayer:
                    scope = new Scope(target, column.Name, typeof(PrimaryPayer));
                    break;
                case DynamicScopeEnum.Sex:
                    scope = new Scope(target, column.Name, typeof(Sex));
                    break;
                case DynamicScopeEnum.AdmissionSource:
                    scope = new Scope(target, column.Name, typeof(AdmissionSource));
                    break;
                case DynamicScopeEnum.AdmissionType:
                    scope = new Scope(target, column.Name, typeof(AdmissionType));
                    break;
                case DynamicScopeEnum.DischargeDisposition:
                    scope = new Scope(target, column.Name, typeof(DischargeDisposition));
                    break;
                case DynamicScopeEnum.EDServices:
                    scope = new Scope(target, column.Name, typeof(EDServices));
                    break;
                case DynamicScopeEnum.PointOfOrigin:
                    scope = new Scope(target, column.Name, typeof(PointOfOrigin));
                    break;
                case DynamicScopeEnum.HospitalTraumaLevel:
                    scope = new Scope(target, column.Name, typeof(HospitalTraumaLevel));
                    break;
                case DynamicScopeEnum.Custom:
                    scope = new Scope(target, column.Name) { IsCustom = true };
                    break;
                default:
                    scope = default(Scope);
                    break;
            }

            var element = target.Elements.FirstOrDefault(e => e.Name.EqualsIgnoreCase(column.Name));
            if (element != null && element.Scope == null)
            {
                element.Scope = scope;
            }

            scope.Description = column.Description;
            scope.CreateCustomScopeValues(scope.Name, column);

            return scope;
        }

        /// <summary>
        /// Creates the custom scope values.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="name">The name.</param>
        /// <param name="column">The column.</param>
        public static void CreateCustomScopeValues(this Scope scope, string name, DynamicTargetColumn column)
        {
            if (column.Scope == DynamicScopeEnum.None) return;

            if (scope.IsCustom)
            {
                // TODO: FINISH CUSTOM SCOPE VALUES. JASON
            }
            else
            {
                if (!string.IsNullOrEmpty(scope.ClrType))
                {
                    try
                    {
                        var scopeType = Type.GetType(scope.ClrType);
                        var enumValues = Enum.GetValues(scopeType);

                        // IList<System.Tuple<string, string, object>> enumValuDescList = new List<System.Tuple<string, string, object>>();
                        foreach (var value in enumValues)
                        {
                            var fi = value.GetType().GetField(value.ToString());

                            if (null == fi) continue; // Next

                            var attrs = fi.GetCustomAttributes(typeof(WingScopeValueAttribute), true).OfType<WingScopeValueAttribute>().ToArray();

                            if (attrs.Length == 0) continue; // Next 

                            var item = new ScopeValue(scope, attrs[0].Name) { Description = attrs[0].Description, Value = attrs[0].Value };

                            if (!item.Owner.Name.EqualsIgnoreCase(scope.Name))
                            {
                                
                            }
                            
                            if (!scope.Values.Any(sv => sv.Name.EqualsIgnoreCase(item.Name)))
                                scope.Values.Add(item);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// Loads the custom elements.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="wingTarget">The wing target.</param>
        public static void LoadCustomElements(this Target target, DynamicTarget wingTarget)
        {
            if (wingTarget == null || wingTarget.Columns == null) return;

            for (var index = 0; index < wingTarget.Columns.Count; index++)
            {
                var targetColumn = wingTarget.Columns[index];
                var result = new Element(target, targetColumn.Name);
                result.LongDescription = result.Description = targetColumn.Description;
                result.Type = targetColumn.DataType;
                result.IsRequired = targetColumn.IsRequired;
                result.Ordinal = (index + 1);
                result.MappingHints = new List<string> { targetColumn.Name };
                result.IsUnique = targetColumn.IsUnique;
                result.Length = (targetColumn.Length > 0) ? (int?)targetColumn.Length : null;
                result.Scale = (targetColumn.Scale > 0) ? (int?)targetColumn.Scale : null;
                result.Precision = (targetColumn.Precision > 0) ? (int?)targetColumn.Precision : null;

                if (target.Elements.All(t => !t.Name.EqualsIgnoreCase(result.Name)))
                    target.Elements.Add(result);
            }
        }

        /// <summary>
        /// Creates the measure.
        /// </summary>
        /// <param name="dynamicMeasure">The dynamic measure.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="target">The target.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public static Measure CreateMeasure(this DynamicTargetMeasure dynamicMeasure, DynamicTarget dynamicTarget, Target target, ISession session)
        {
            Measure measure = null;

            if (dynamicMeasure == null)
                return default(DynamicMeasure);

            if (dynamicMeasure.IsExistingMeasure && !string.IsNullOrEmpty(dynamicMeasure.MeasureCode))
            {
                measure = session.Query<Measure>()
                                 .FirstOrDefault(m => m.Name.ToLower() == dynamicMeasure.MeasureCode.ToLower());
            }

            if (measure == null)
            {
                measure = Measure.CreateMeasure(typeof(DynamicMeasure), target, dynamicMeasure.Name);
                measure.MeasureType = dynamicMeasure.MeasureType;
                measure.RiskAdjustedMethod = dynamicMeasure.RiskAdjustedMethod;
                measure.Description = dynamicMeasure.Description;
                measure.ConsumerDescription = dynamicMeasure.ConsumerDescription;
                measure.Footnotes = dynamicMeasure.Footnotes;
                measure.MoreInformation = dynamicMeasure.MoreInformation;
                measure.Url = dynamicMeasure.Url;
                measure.UrlTitle = dynamicMeasure.UrlTitle;
                measure.NQFEndorsed = dynamicMeasure.NqfEndorsed;
                measure.NQFID = dynamicMeasure.NqfId;
                measure.MeasureTitle = new MeasureTitle
                {
                    Plain = dynamicMeasure.MeasureTitle.Plain,
                    Clinical = dynamicMeasure.MeasureTitle.Clinical,
                    Selected = dynamicMeasure.MeasureTitle.Selected
                };

                measure.ConsumerPlainTitle = dynamicMeasure.MeasureTitle.ConsumerPlain;
                measure.HigherScoresAreBetter = dynamicMeasure.HigherScoresAreBetter;
                measure.SuppressionNumerator = string.IsNullOrEmpty(dynamicMeasure.SuppressionNumerator)
                                                   ? (decimal?)null
                                                   : decimal.Parse(dynamicMeasure.SuppressionNumerator);
                measure.SuppressionDenominator = string.IsNullOrEmpty(dynamicMeasure.SuppressionDenominator)
                                                     ? (decimal?)null
                                                     : decimal.Parse(dynamicMeasure.SuppressionDenominator);
                measure.PerformMarginSuppression = dynamicMeasure.PerformMarginSuppression;
                measure.StatePeerBenchmark = new StatePeerBenchmark
                {
                    CalculationMethod = dynamicMeasure.StatePeerBenchmark.CalculationMethod,
                    ProvidedBenchmark = string.IsNullOrEmpty(dynamicMeasure.StatePeerBenchmark.ProvidedBenchmark)
                                            ? (decimal?)null
                                            : decimal.Parse(dynamicMeasure.StatePeerBenchmark.ProvidedBenchmark)
                };
                dynamicMeasure.Topics.CreateTopics(dynamicTarget, target, session).ForEach(t => measure.AddTopic(t));
                measure.UpperBound = string.IsNullOrEmpty(dynamicMeasure.UpperBound)
                                         ? (decimal?)null
                                         : decimal.Parse(dynamicMeasure.UpperBound);
                measure.LowerBound = string.IsNullOrEmpty(dynamicMeasure.LowerBound)
                                         ? (decimal?)null
                                         : decimal.Parse(dynamicMeasure.LowerBound);

                measure.SupportsCost = dynamicMeasure.SupportsCost;
            }

            return measure;
        }

        /// <summary>
        /// Creates the topics.
        /// </summary>
        /// <param name="dynamicTopics">The dynamic topics.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="target">The target.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public static IList<Topic> CreateTopics(this IList<DynamicTopic> dynamicTopics,
                                                DynamicTarget dynamicTarget, Target target, ISession session)
        {
            var topics = new List<Topic>();

            if (dynamicTopics == null || !dynamicTopics.Any())
                return topics;

            foreach (var dynamicTopic in dynamicTopics.ToList())
            {
                using (var trans = session.BeginTransaction())
                {
                    var topicCategory = session.Query<TopicCategory>()
                                                         .FirstOrDefault(tc => tc.Name.ToUpper() == dynamicTopic.Name.ToUpper()) ??
                                                          new TopicCategory(dynamicTopic.Name)
                                                          {
                                                              LongTitle = dynamicTopic.Name,
                                                              Description = dynamicTopic.Name,
                                                              TopicType = dynamicTopic.Type,
                                                              WingTargetName = target.Name,
                                                              ConsumerDescription = dynamicTopic.ConsumerDescription,
                                                              CategoryType = dynamicTopic.CategoryType,
                                                              Facts = dynamicTopic.Facts.Select(f=> new TopicFacts
                                                                                                          {
                                                                                                              Name = f.Name,
                                                                                                              Text = f.Text,
                                                                                                              CitationText = f.CitationText,
                                                                                                              ImagePath = f.ImagePath
                                                                                                          }).ToList()
                                                          };

                    if (!topicCategory.IsPersisted)
                        session.SaveOrUpdate(topicCategory);
                    else
                        topicCategory = session.Merge(topicCategory);


                    foreach (var subTopic in dynamicTopic.SubTopics)
                    {
                        var topic = session.Query<Topic>().FirstOrDefault(t =>
                                                                        t.Name.ToUpper() == subTopic.Name.ToUpper() &&
                                                                        t.Owner.Name.ToLower() == topicCategory.Name.ToLower()) ??
                                                        new Topic(topicCategory, subTopic.Name)
                                                        {
                                                            LongTitle = subTopic.Name,
                                                            Description = subTopic.Name,
                                                            WingTargetName = target.Name
                                                        };

                        if (!topic.IsPersisted)
                            session.SaveOrUpdate(topic);
                        else
                            topic = session.Merge(topic);

                        topics.Add(topic);
                    }
                    trans.Commit();
                }
            }

            return topics;
        }
    }
}
