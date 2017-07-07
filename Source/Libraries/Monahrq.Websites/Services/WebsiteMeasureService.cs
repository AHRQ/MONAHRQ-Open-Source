using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Common;
using Monahrq.Websites.Events;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Services
{
    public interface IWebsiteMeasureService : IDataServiceBase
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
        //T Refresh<T>(T item);
        void SaveMeasure(Measure item, Action<bool, Exception> callback);
    }

    [Export(typeof(IWebsiteMeasureService))]
    //[PartCreationPolicy(CreationPolicy.Shared)]
    public class WebsiteMeasureService : DataServiceBase, IWebsiteMeasureService
    {
        public const string ALL_DATASETS = "All Datasets";

        public WebsiteMeasureService()
            : base()
        {
            InitData();
        }

        public IEnumerable<TopicCategory> EntityTopics { get; private set; }
        public IEnumerable<Measure> EntityMeasures { get; private set; }
        public IEnumerable<TopicViewModel> TopicViewModels
        {
            get { return EntityTopics.OrderBy(x => x.Name).Select(t => new TopicViewModel(t)).ToList(); }
        }

        public IEnumerable<MeasureModel> MeasureViewModels
        {
            get { return EntityMeasures.Where(x => !x.IsOverride).OrderBy(x => x.Name).Select(t => new MeasureModel(t)).ToList(); }
        }

        public List<string> DataSets { get; set; }

        private void InitData()
        {
            WaitCursor.Show();
            EntityTopics = Load<TopicCategory>();
            EntityMeasures = Load<Measure>();
            DataSets = Load<Target>().Select(x => x.Name).ToList();
            DataSets.Insert(0, ALL_DATASETS);
        }

        private void ReloadTopicsMeasures(int id)
        {
            EntityTopics = Load<TopicCategory>();
            EntityMeasures = Load<Measure>();
            PublishDataUpdateEvent(id);
        }

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

        public void DeleteTopic(Topic topic, bool publish)
        {
            WaitCursor.Show();

            var measures = EntityMeasures.Where(x => x.Topics.Contains(topic)).ToList();

            foreach (var m in measures)
            {
                m.Topics.Remove(topic);
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

        private List<Measure> MeasurePiplineList = new List<Measure>();

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

        public void AddToPipline(Measure measure)
        {
            MeasurePiplineList.Add(measure);
        }

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

        private void PublishDataUpdateEvent(int id)
        {
            if (EventAggregator != null)
            {
                EventAggregator.GetEvent<TopicsUpdatedEvent>().Publish(id);
            }
        }

        private List<T> Load<T>() where T : INamedEntity
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

        //public T Refresh<T>(T item)
        //{
        //    Session.Refresh(item);
        //    return item;
        //}

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
                        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                        callback(false, e);
                    }
                });



        }
    }
}
