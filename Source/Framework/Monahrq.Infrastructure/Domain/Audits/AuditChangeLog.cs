using System;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Domain.Audits
{
    [Serializable]
    public abstract class AuditChangeLog : Entity<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditChangeLog" /> class.
        /// </summary>
        protected AuditChangeLog()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditChangeLog" /> class.
        /// </summary>
        /// <param name="auditType">Type of the audit.</param>
        /// <param name="owner">The owner.</param>
        protected AuditChangeLog(AuditType auditType, Entity<int> owner) 
        {
            Action = auditType;
            Owner = owner;
        }

        public virtual void InitLog(Entity<int> entity)
        {

        }

        #region Properties
        /// <summary>
        /// Gets or sets the type of the owner entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public string OwnerType { get; set; }
        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity GUID.
        /// </value>
        public Entity<int> Owner { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity GUID.
        /// </value>
        public virtual int OwnerId { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the action by.
        /// </summary>
        /// <value>
        /// The action by.
        /// </value>
        public string ActionBy { get; set; }
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public AuditType Action { get; set; }
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }
        /// <summary>
        /// Gets or sets the old property value.
        /// </summary>
        /// <value>
        /// The old property value.
        /// </value>
        public string OldPropertyValue { get; set; }
        /// <summary>
        /// Gets or sets the new property value.
        /// </summary>
        /// <value>
        /// The new property value.
        /// </value>
        public string NewPropertyValue { get; set; }

        ///// <summary>
        ///// Gets or sets the parent.
        ///// </summary>
        ///// <value>
        ///// The parent.
        ///// </value>
        //public IEntity<int> Parent
        //{
        //    get { return Entity; }
        //    set { Entity = value; }
        //}

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public new string Name
        {
            get
            {
                return string.Format("{0}: {1}", Inflector.Titleize(GetType().Name), Inflector.Capitalize(OwnerType));
            }
            set {}
        }

        /// <summary>
        /// Gets or sets the create date.
        /// </summary>
        /// <value>
        /// The create date.
        /// </value>
        public DateTime CreateDate { get; set; }

        #endregion
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //[Serializable]
    //public class InsertChangeLog : AuditChangeLog
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="InsertChangeLog"/> class.
    //    /// </summary>
    //    public InsertChangeLog()
    //    {
    //        Action = AuditType.Insert;
    //    }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //[Serializable]
    //public class UpdateChangeLog : AuditChangeLog
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="UpdateChangeLog"/> class.
    //    /// </summary>
    //    public UpdateChangeLog()
    //    {
    //        Action = AuditType.Update;
    //    }

    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //[Serializable]
    //public class DeleteChangeLog : AuditChangeLog
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="DeleteChangeLog"/> class.
    //    /// </summary>
    //    public DeleteChangeLog()
    //    {
    //        Action = AuditType.Delete;
    //    }
    //}

    /// <summary>
    /// Enum used in determining if an item has been inserted, updated or deleted.
    /// </summary>
    [Serializable]
    public enum AuditType
    {
        /// <summary>
        /// Add
        /// </summary>
        Insert = 0,
        /// <summary>
        /// Edit
        /// </summary>
        Update = 0x1,
        /// <summary>
        /// Remove
        /// </summary>
        Delete = 0x2
    }
}
