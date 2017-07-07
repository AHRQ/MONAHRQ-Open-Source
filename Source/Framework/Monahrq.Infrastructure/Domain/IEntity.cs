
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain.Reports;

namespace Monahrq.Infrastructure.Entities.Domain
{
    public interface IEntity : INotifyPropertyChanged, INotifyDataErrorInfo //IDataErrorInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is changed; otherwise, <c>false</c>.
        /// </value>
        bool IsChanged { get; set; }
        /// <summary>
        /// Gets a value indicating whether this instance is persisted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is persisted; otherwise, <c>false</c>.
        /// </value>
        bool IsPersisted { get; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        bool IsDeleted { get; set; }
        /// <summary>
        /// Cleans the before save.
        /// </summary>
        void CleanBeforeSave();

        /// <summary>
        /// Validates the entity asynchronous.
        /// </summary>
        /// <returns></returns>
        Task ValidateAsync();

        /// <summary>
        /// Validates the entity.
        /// </summary>
        void Validate();
    }

    public interface ISoftDeletableOnly
    {
        bool IsDeleted { get; set; }
    }

    public interface IEntity<TId> : IEntity
    {
        TId Id { get; set; }
    }

    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }

    public interface IValidableSelectable : ISelectable
    {
        bool ValidateBeforSelection(object objNeedForValidation);
    }

    public interface IWebsiteReportValidableSelectable : IValidableSelectable
    {
        IList<Audience> WebsiteAudiences { get; set; } 
    }

    public class WebsiteReportValidableSelectableStruct
    {
        public List<object> Items { get; set; }
        public IList<Audience> WebsiteAudiences { get; set; }
    }
}
