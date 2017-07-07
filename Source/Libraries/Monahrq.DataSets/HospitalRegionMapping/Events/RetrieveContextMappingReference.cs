using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Practices.Prism.Events;
using Monahrq.DataSets.HospitalRegionMapping.Categories;
using Monahrq.DataSets.HospitalRegionMapping.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Hospitals;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Sdk.Events;

namespace Monahrq.DataSets.HospitalRegionMapping.Events
{
    /// <summary>
    /// The cancelable event arguements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.ComponentModel.CancelEventArgs" />
    public class CancelableEventArgs<T> : CancelEventArgs
    {
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public T Data { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CancelableEventArgs{T}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public CancelableEventArgs(T data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// The wizard navigate selected states event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.Uri}" />
    public class WizardNavigateSelectStatesEvent : CompositePresentationEvent<Uri> { }

    /// <summary>
    /// The item deleted event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class ItemDeletedEventBase<T> : CompositePresentationEvent<T> { }
    /// <summary>
    /// The item deleting event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.DataSets.HospitalRegionMapping.Events.CancelableEventArgs{T}}" />
    public class ItemDeletingEventBase<T> : CompositePresentationEvent<CancelableEventArgs<T>> { }
    /// <summary>
    /// The item delete cancelled event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class ItemDeletedCanceledEventBase<T> : CompositePresentationEvent<T> { }
    /// <summary>
    /// The item saved event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class ItemSavedEventBase<T> : CompositePresentationEvent<T> { }
    /// <summary>
    /// Theitem saving event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.DataSets.HospitalRegionMapping.Events.CancelableEventArgs{T}}" />
    public class ItemSavingEventBase<T> : CompositePresentationEvent<CancelableEventArgs<T>> { }
    /// <summary>
    /// The item save cancelled event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class ItemSaveCanceledEventBase<T> : CompositePresentationEvent<T> { }
    /// <summary>
    /// The item editing event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.DataSets.HospitalRegionMapping.Events.CancelableEventArgs{T}}" />
    public class ItemEditingEventBase<T> : CompositePresentationEvent<CancelableEventArgs<T>> { }
    /// <summary>
    /// The item edit cancelled event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class ItemEditCanceledEventBase<T> : CompositePresentationEvent<T> { }
    /// <summary>
    /// The item selected event base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class ItemSelectedEventBase<T> : CompositePresentationEvent<T> { }
    /// <summary>
    /// The global geographic context applied event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.String}" />
    public class ContextAppliedEvent : CompositePresentationEvent<string> { }
    /// <summary>
    /// The view model ready event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class ViewModelReadyEventBase<T> : CompositePresentationEvent<T> { }

    /// <summary>
    /// The hospital categories ciew model ready event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ViewModelReadyEventBase{Monahrq.DataSets.HospitalRegionMapping.Categories.CategoriesViewModel}" />
    public class CategoriesViewModelReadyEvent : ViewModelReadyEventBase<CategoriesViewModel> { }
    /// <summary>
    /// The hospital categories view model saved event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ItemSavedEventBase{Monahrq.DataSets.HospitalRegionMapping.Categories.CategoriesViewModel}" />
    public class CategoriesViewModelSavedEvent : ItemSavedEventBase<CategoriesViewModel> { }

    /// <summary>
    /// The regions view model saved event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ItemSavedEventBase{Monahrq.Infrastructure.Domain.Regions.Region}" />
    public class RegionsViewModelSavedEvent : ItemSavedEventBase<Region> { }
    /// <summary>
    /// The regions view model ready event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ViewModelReadyEventBase{Monahrq.DataSets.HospitalRegionMapping.Regions.RegionsViewModel}" />
    public class RegionsViewModelReadyEvent : ViewModelReadyEventBase<RegionsViewModel> { }

    /// <summary>
    /// the hospital view model selected event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ItemSelectedEventBase{Monahrq.DataSets.HospitalRegionMapping.Hospitals.HospitalViewModel}" />
    public class HospitalViewModelSelectedEvent : ItemSelectedEventBase<HospitalViewModel> { }
    /// <summary>
    /// The hospitals view model ready event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ViewModelReadyEventBase{Monahrq.DataSets.HospitalRegionMapping.Hospitals.HospitalsViewModel}" />
    public class HospitalsViewModelReadyEvent : ViewModelReadyEventBase<HospitalsViewModel> { }
    /// <summary>
    /// 
    /// </summary>the hospital view model ready event.
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ViewModelReadyEventBase{Monahrq.DataSets.HospitalRegionMapping.Hospitals.HospitalViewModel}" />
    public class HospitalViewModelReadyEvent : ViewModelReadyEventBase<HospitalViewModel> { }
    /// <summary>
    /// the hospitals view model saved event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ItemSavedEventBase{Monahrq.DataSets.HospitalRegionMapping.Hospitals.HospitalsViewModel}" />
    public class HospitalsViewModelSavedEvent : ItemSavedEventBase<HospitalsViewModel> { }
    /// <summary>
    /// the hospitals list editing event.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.Events.ItemSavedEventBase{System.Collections.Generic.List{Monahrq.Infrastructure.Entities.Domain.Hospitals.Hospital}}" />
    public class HospitalsListEditingEvent : ItemSavedEventBase<List<Hospital>> { }

    /// <summary>
    /// The request load mapping event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.Type}" />
    public class RequestLoadMappingTabEvent : CompositePresentationEvent<Type> { }

    /// <summary>
    /// The state list request event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Entities.Events.ExtendedEventArgs{System.Collections.Generic.List{System.String}}}" />
    public class StateListRequestEvent : CompositePresentationEvent<ExtendedEventArgs<List<string>>> { }

    /// <summary>
    /// The hospital details navigate event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Entities.Events.ExtendedEventArgs{Monahrq.DataSets.HospitalRegionMapping.Hospitals.HospitalViewModel}}" />
    public class HospitalsDetailsNavigateEvent : CompositePresentationEvent<ExtendedEventArgs<HospitalViewModel>> { }

    /// <summary>
    /// The geographic context change event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.Empty}" />
    public class GeographicalContextChangeEvent : CompositePresentationEvent<Empty> { }

    /// <summary>
    /// The request item event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Entities.Events.ExtendedEventArgs{T}}" />
    public class RequestItemEvent<T> : CompositePresentationEvent<ExtendedEventArgs<T>> { } ;
}
