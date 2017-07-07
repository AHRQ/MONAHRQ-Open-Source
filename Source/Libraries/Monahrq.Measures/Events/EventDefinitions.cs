using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Measures.Service;
using Monahrq.Measures.ViewModels;
using System;

namespace Monahrq.Measures.Events
{

    public class StatusNotificationEvent : CompositePresentationEvent<string> { }
    public class MeasureDetailsNavigateEvent:CompositePresentationEvent<MeasureModel>{}




    public class SelectedTopicAssigmentEvent : CompositePresentationEvent<bool> { }
    public class TopicsUpdatedEvent : CompositePresentationEvent<int> { }
    public class TopicFilterApplied : CompositePresentationEvent<EventArgs> { }
    public class MeasureFilterApplied : CompositePresentationEvent<EventArgs> { }
    public class TopicSelectedChanged : CompositePresentationEvent<ITreeViewItemViewModel> { }


    /// <summary>
    /// Topic evnet model class
    /// </summary>
    /// <seealso cref="Monahrq.Measures.Events.EventModel{Monahrq.Infrastructure.Entities.Domain.Measures.Topic}" />
    public class TopicEventModel:EventModel<Topic>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicEventModel"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public TopicEventModel(Topic entity) : base(entity)
        {
        }

        /// <summary>
        /// Gets the validate.
        /// </summary>
        /// <value>
        /// The validate.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public override Action<IEntity, Exception> Validate
        {
            get { throw new NotImplementedException(); }
        }
    }

    /// <summary>
    /// Interface to validate the rule
    /// </summary>
    /// <typeparam name="INamedEntity">The type of the named entity.</typeparam>
    public interface IValidationRule<INamedEntity>
    {
    }

    //todo: keep thinking this rolls back to same State Machine and workflow
    //todo: pls don't change I am still thinking
    //todo: simplify

    /// <summary>
    /// Event model class
    /// </summary>
    /// <typeparam name="T">the target type</typeparam>
    /// <seealso cref="Monahrq.Measures.Events.IEventModel{T}" />
    public abstract class EventModel<T>:IEventModel<T> where T : IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventModel{T}"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected EventModel(T entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public T Entity { get; private set; }
        /// <summary>
        /// Gets the validate.
        /// </summary>
        /// <value>
        /// The validate.
        /// </value>
        public abstract Action<IEntity, Exception> Validate { get; }

        /// <summary>
        /// Gets the insert.
        /// </summary>
        /// <value>
        /// The insert.
        /// </value>
        public Action<T, Exception> Insert { get; private set; }
        /// <summary>
        /// Gets the select.
        /// </summary>
        /// <value>
        /// The select.
        /// </value>
        public Action<T, Exception> Select { get; private set; }
        /// <summary>
        /// Gets the update.
        /// </summary>
        /// <value>
        /// The update.
        /// </value>
        public Action<T, Exception> Update { get; private set; }
        /// <summary>
        /// Gets the delete.
        /// </summary>
        /// <value>
        /// The delete.
        /// </value>
        public Action<T, Exception> Delete { get; private set; }


     
    }

    /// <summary>
    /// Interface to event model amd contains several Actions
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEventModel<TEntity> where TEntity:IEntity 
    {
        TEntity Entity { get; }

        Action<IEntity, Exception> Validate { get; }
        Action<TEntity, Exception> Insert { get; }
        Action<TEntity, Exception> Select { get; }
        Action<TEntity, Exception> Update { get; }
        Action<TEntity, Exception> Delete { get; }


    }
}
