using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Services;
using Monahrq.Measures.Events;
using Monahrq.Measures.ViewModels;
using Monahrq.Sdk.Common;
using NHibernate.Linq;

namespace Monahrq.Measures.Service
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMeasureServiceSync
    {
        IEnumerable<TopicViewModel> TopicViewModels { get; }
        IEnumerable<MeasureModel> MeasureViewModels { get; }
        IEnumerable<TopicCategory> EntityTopics { get; }
        IEnumerable<Measure> EntityMeasures { get; }
        List<string> DataSets { get; set; }
        void SaveOrUpdateTopic(TopicCategory category);
        void DeleteTopic(Topic topic, bool publish);
        void DeleteTopicCategory(TopicCategory category);
        void AddToPipline(Measure measure);
        void CommitPipeline();
        T GetById<T>(int Id);
        T Refresh<T>(T item);
        void SaveMeasure(Measure item, Action<bool, Exception> callback);
        IList<string> GetWebsitesForMeasure(int measureId);
        MeasureModel GetOrCreateMeasure(int? measureId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.DataServiceBase" />
    /// <seealso cref="Monahrq.Measures.Service.IMeasureServiceSync" />
    [Export(typeof(IMeasureServiceSync))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MeasureServiceSync : DataServiceBase, IMeasureServiceSync
    {
        /// <summary>
        /// All datasets
        /// </summary>
        public const string ALL_DATASETS = "All Datasets";

        /// <summary>
        /// Gets the entity topics.
        /// </summary>
        /// <value>
        /// The entity topics.
        /// </value>
        public IEnumerable<TopicCategory> EntityTopics { get; private set; }
        /// <summary>
        /// Gets the entity measures.
        /// </summary>
        /// <value>
        /// The entity measures.
        /// </value>
        public IEnumerable<Measure> EntityMeasures { get; private set; }
        /// <summary>
        /// Gets the topic view models.
        /// </summary>
        /// <value>
        /// The topic view models.
        /// </value>
        public IEnumerable<TopicViewModel> TopicViewModels
        {
            get { return EntityTopics.OrderBy(x => x.Name).Select(t => new TopicViewModel(t)).ToList(); }
        }

        /// <summary>
        /// Gets the measure view models.
        /// </summary>
        /// <value>
        /// The measure view models.
        /// </value>
        public IEnumerable<MeasureModel> MeasureViewModels
        {
            get { return EntityMeasures.Where(x => !x.IsOverride).OrderBy(x => x.Name).Select(t => new MeasureModel(t)).ToList(); }
        }

        /// <summary>
        /// Gets or sets the data sets.
        /// </summary>
        /// <value>
        /// The data sets.
        /// </value>
        public List<string> DataSets { get; set; }

        /// <summary>
        /// Gets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import]
        public IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureServiceSync"/> class.
        /// </summary>
        public MeasureServiceSync()
            : base()
        {
            //InitData();
        }

        /// <summary>
        /// Initializes the data.
        /// </summary>
        private void InitData()
        {
            WaitCursor.Show();
            EntityTopics = Load<TopicCategory>();
            EntityMeasures = Load<Measure>();
            DataSets = GetDatasetForMeasure(); // Load<Target>().Select(x => x.Name).ToList();
            DataSets.Insert(0, ALL_DATASETS);
        }

        /// <summary>
        /// Reloads the topics measures.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void ReloadTopicsMeasures(int id)
        {
            EntityTopics = Load<TopicCategory>();
            EntityMeasures = Load<Measure>();
            PublishDataUpdateEvent(id);
        }

        /// <summary>
        /// Saves the or update topic.
        /// </summary>
        /// <param name="category">The category.</param>
        public void SaveOrUpdateTopic(TopicCategory category)
        {
            WaitCursor.Show();
            Save(category, (o, e) =>
                {
                    if (e == null)
                    {
                        ReloadTopicsMeasures(category.Id);
                    }
                    else
                    {
                        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                    }
                });
        }

        /// <summary>
        /// Gets the or create measure.
        /// </summary>
        /// <param name="measureId">The measure identifier.</param>
        /// <returns></returns>
        public MeasureModel GetOrCreateMeasure(int? measureId)
        {
            var measureModel = default(MeasureModel);

            var measure = default(Measure);
            if (measureId.HasValue && measureId.Value > 0)
            {
                using (var session = base.GetSession())
                {
                    measure = session.Get<Measure>(measureId.Value);
                }
            }

            if (measure != null)
            {
                measureModel = new MeasureModel(measure);
            }
            else
            {
                measure = new Measure();
                measureModel = new MeasureModel(measure);
            }

            return measureModel;
        }

        /// <summary>
        /// Deletes the topic.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        public void DeleteTopic(Topic topic, bool publish)
        {
            WaitCursor.Show();

            var measures = EntityMeasures.Where(x => x.Topics.Contains(topic)).ToList();

            foreach (var m in measures)
            {
                m.RemoveTopic(topic);
                MeasurePiplineList.Add(m);
            }

            SaveMeasurePipline((isCompleted, e) =>
            {
                if (e == null)
                {
                    topic.Owner.Topics.Remove(topic);
                    Save(topic.Owner, (o, ex) =>
                        {
                            if (ex == null)
                            {
                                Delete(topic, (r, err) =>
                                    {
                                        if (err == null)
                                        {
                                            if (publish)
                                            {
                                                PublishDataUpdateEvent(0);
                                            }
                                        }
                                        else
                                        {
                                            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(err);
                                        }
                                    });
                            }
                            else
                            {
                                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                            }

                        });
                }
                else
                {
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                }
            });
        }

        /// <summary>
        /// Saves the measure pipline.
        /// </summary>
        /// <param name="callback">The callback.</param>
        private void SaveMeasurePipline(Action<bool, Exception> callback)
        {
            WaitCursor.Show();

            var isCompleted = false;
            Exception error = null;

            foreach (var measure in MeasurePiplineList)
            {
                Save(measure, (o, e) =>
                    {
                        if (e == null)
                        {
                            isCompleted = true;

                            // attempt to resolve bug #963 -- this doesn't work
                            //foreach (var topic in measure.Topics)
                            //{
                            //    Session.Merge(topic);
                            //}
                        }
                        else
                        {
                            error = e;
                            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                            callback(isCompleted, error);
                        }

                    });
            }

            MeasurePiplineList.Clear();
            callback(isCompleted, error);
        }

        /// <summary>
        /// The measure pipline list
        /// </summary>
        private List<Measure> MeasurePiplineList = new List<Measure>();

        /// <summary>
        /// Deletes the topic category.
        /// </summary>
        /// <param name="category">The category.</param>
        public void DeleteTopicCategory(TopicCategory category)
        {
            WaitCursor.Show();

            foreach (var topic in category.Topics.ToArray())
            {
                DeleteTopic(topic, false);
            }

            Delete(category, (r, ex) =>
                      {
                          if (ex == null)
                          {
                              ReloadTopicsMeasures(0);
                          }
                          else
                          {
                              EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                          }
                      });
        }

        /// <summary>
        /// Adds to pipline.
        /// </summary>
        /// <param name="measure">The measure.</param>
        public void AddToPipline(Measure measure)
        {
            MeasurePiplineList.Add(measure);
        }

        /// <summary>
        /// Commits the pipeline.
        /// </summary>
        public void CommitPipeline()
        {
            WaitCursor.Show();
            SaveMeasurePipline((isSaved, e) =>
                {
                    if (e == null)
                    {
                        ReloadTopicsMeasures(0);
                    }
                    else
                    {
                        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                    }
                });
        }

        /// <summary>
        /// Publishes the data update event.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void PublishDataUpdateEvent(int id)
        {
            //if (EventAggregator != null)
            //{
            //    SynchronizationContext.Current.Post(x => {
            //        EventAggregator.GetEvent<TopicsUpdatedEvent>().Publish(id);
            //    }, null);
            //}
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<T> Load<T>() where T : class, IEntity
        {
            var data = new List<T>();

            GetAll<T>((result, e) =>
            {
                if (e == null)
                {
                    data = result;
                }
                else
                {
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                }

            });

            return data;
        }

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public T GetById<T>(int Id)
        {
            var item = default(T);
            GetEntityById<T>(Id, (result, error) =>
            {
                if (error == null)
                {
                    item = result;
                }
                else
                {
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(error);
                }
            });

            return item;
        }

        /// <summary>
        /// Refreshes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public T Refresh<T>(T item)
        {
            using (var session = GetSession())
                session.Refresh(item);

            return item;
        }

        /// <summary>
        /// Saves the measure.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="callback">The callback.</param>
        public void SaveMeasure(Measure item, Action<bool, Exception> callback)
        {
            Exception error = null;
            Save(item, (o, e) =>
                {
                    if (e == null)
                    {
                        var result = true;
                        ReloadTopicsMeasures(0);
                        callback(result, error);
                    }
                    else
                    {
                        error = e;
                        //EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                        callback(false, e);
                    }
                });
        }

        /// <summary>
        /// Gets the websites for measure.
        /// </summary>
        /// <param name="measureId">The measure identifier.</param>
        /// <returns></returns>
        public IList<string> GetWebsitesForMeasure(int measureId)
        {
            using (var session = GetSession())
            {
                return session.Query<Website>().Where(x => x.Measures.Any(wm => wm.IsSelected && wm.OriginalMeasure.Id == measureId)).Select(w => w.Name).ToList();
            }
        }

        /// <summary>
        /// Gets the dataset for measure.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDatasetForMeasure()
        {
            using (var session = GetStatelessSession())
            {
                return session.Query<Target>().Select(w => w.Name).ToList();
            }
        }
    }
}
