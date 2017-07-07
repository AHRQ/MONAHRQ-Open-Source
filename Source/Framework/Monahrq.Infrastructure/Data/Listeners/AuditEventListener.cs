using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Domain.Audits;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Utilities;
using NHibernate;
using NHibernate.Event;

namespace Monahrq.Infrastructure.Data.Listeners
{
    internal class AuditEventListener : IPostUpdateEventListener, IPostInsertEventListener, IPostDeleteEventListener
    {
        /// <summary>
        /// Called when [post update].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public void OnPostUpdate(PostUpdateEvent e)
        {
            if (IsAuditable(e.Entity) && e.OldState != null)
            {
                var dirtyFieldIndexes = e.Persister.FindDirty(e.State, e.OldState, e.Entity, e.Session).ToArray();

                if (dirtyFieldIndexes.Any(fieldIndex => ProcessPropertyName(e.Persister.PropertyNames[fieldIndex]) == "IsDeleted"))
                {
                    InsertDeleteAuditLog((Entity<int>)e.Entity, e.Session, AuditType.Delete);
                }

                UpdateAuditLog(e.OldState, e.State, e.Persister.PropertyNames, dirtyFieldIndexes, (Entity<int>)e.Entity, e.Session);
            }
        }

        /// <summary>
        /// Called when [post insert].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public void OnPostInsert(PostInsertEvent e)
        {
            if (IsAuditable(e.Entity))
            {
                InsertDeleteAuditLog((Entity<int>)e.Entity, e.Session, AuditType.Insert);
            }
        }

        /// <summary>
        /// Called when [post delete].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public void OnPostDelete(PostDeleteEvent e)
        {
            if (IsAuditable(e.Entity))
            {
                InsertDeleteAuditLog((Entity<int>)e.Entity, e.Session, AuditType.Delete);
            }
        }

        #region Private Helper Methods
        /// <summary>
        /// Determines whether the specified entity is auditable.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the specified entity is auditable; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsAuditable(object entity)
        {
            if (entity is AuditChangeLog)
                return false;

            var auditableAttr = entity.GetType().GetCustomAttributes(typeof(AuditableAttribute), false);

            return auditableAttr.Length > 0 && (entity is Entity<int>) && !(entity as Entity<int>).SkipAudit;
        }

        /// <summary>
        /// Updates the audit trail.
        /// </summary>
        /// <param name="oldstate">The oldstate.</param>
        /// <param name="newstate">The newstate.</param>
        /// <param name="names">The names.</param>
        /// <param name="dirtyIndexes">The dirty indexes.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="session">The session.</param>
        private void UpdateAuditLog(IList<object> oldstate, IList<object> newstate, IList<string> names, IEnumerable<int> dirtyIndexes, Entity<int> entity, ISession session)
        {
            if (oldstate == null || newstate == null)
                return;

            var changedValuesLog = new List<AuditChangeLog>();
            foreach (var dirtyIndex in dirtyIndexes)
            {
                if (oldstate[dirtyIndex] == newstate[dirtyIndex])
                    continue;

                if (names[dirtyIndex] != null && _nonAuditableFieldsList.Any(field => field.Equals(names[dirtyIndex], StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                var auditLog = CreateAuditLog(entity, AuditType.Update);

                var propertyName = ProcessPropertyName(names[dirtyIndex]);
                auditLog.PropertyName = propertyName;
                auditLog.OldPropertyValue = GetPropertyValue(oldstate[dirtyIndex], propertyName, entity);
                auditLog.NewPropertyValue = GetPropertyValue(newstate[dirtyIndex], propertyName, entity);


                changedValuesLog.Add(auditLog);
            }

            changedValuesLog.ForEach(session.SaveOrUpdate);
        }

        /// <summary>
        /// Processes the name of the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static string ProcessPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            if (propertyName.StartsWith("_") && propertyName.EndsWith("Xml"))
            {
                propertyName = Inflector.Titleize(propertyName.Replace("_", null).Replace("Xml", null));
            }

            return propertyName;
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="propertyState">State of the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private string GetPropertyValue(object propertyState, string propertyName, IEntity<int> entity)
        {
            if (propertyState != null)
            {
                var propertyType = propertyState.GetType();
                if (propertyType.Name.EqualsIgnoreCase("List`1") || /*propertyType.Name.EqualsIgnoreCase("List") ||*/
                    propertyType.Name.EqualsIgnoreCase("IEnumerable`1") /*|| propertyType.Name.EqualsIgnoreCase("IEnumerable")*/)
                {
                    if (propertyType.GenericTypeArguments[0].IsPrimitive)
                    {
                        switch (propertyType.GenericTypeArguments[0].Name.ToLower())
                        {
                            case "boolean":
                            case "int16":
                            case "int32":
                            case "int64":
                            case "decimal":
                            case "float":
                            case "double":
                                return XmlHelper.Serialize(propertyState).InnerXml;
                            default:
                                return null;
                        }
                    }
                    else // if(propertyType.GenericTypeArguments[0].Name.ToLower() == "string")
                    {
                        return XmlHelper.Serialize(propertyState).InnerXml;
                    }
                    
                }
                else if (propertyType == typeof(XmlDocument))
                {
                    XmlDocument xmlDoc = propertyState as XmlDocument;

                    if(xmlDoc == null) return null;

                    return XmlHelper.Serialize(propertyState).InnerXml;
                }

                return propertyState.ToString();
            }

            return null;
        }

        /// <summary>
        /// Inserts the delete audit log.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="session">The session.</param>
        /// <param name="auditType">Type of the audit.</param>
        private static void InsertDeleteAuditLog(Entity<int> entity, ISession session, AuditType auditType)
        {
            var auditLog = CreateAuditLog(entity, auditType);
            session.SaveOrUpdate(auditLog);
        }

        /// <summary>
        /// Creates the audit log.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="auditType">Type of the audit.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        private static AuditChangeLog CreateAuditLog(Entity<int> entity, AuditType auditType)
        {
            var entityType = entity.GetType();
            var auditableAttr = entityType.GetCustomAttributes(typeof(AuditableAttribute), false);

            if (!auditableAttr.Any())
                throw new InvalidOperationException(string.Format("A valid auditable attribute could not be found on \"{0}\".", entityType.Name));

            var attribute = auditableAttr.OfType<AuditableAttribute>().First();

            // var auditLog = (AuditChangeLog)Activator.CreateInstance(attribute.AuditLogType, new object[] { auditType, entity });
            var auditLog = (AuditChangeLog)Activator.CreateInstance(attribute.AuditLogType);
            auditLog.InitLog(entity);
            auditLog.Action = auditType;
            auditLog.Owner = entity;
            auditLog.OwnerId = entity.Id;
            auditLog.OwnerType = entity.GetType().Name;
            auditLog.ActionBy = MonahrqContext.GetUserName;
            auditLog.CreateDate = DateTime.Now;

            return auditLog;

        }

        #endregion

        private readonly string[] _nonAuditableFieldsList = { "Id", "SkipAudit", "IsChanged", "IsPersisted" };
    }

    internal class AuditPreUpdateEventListener : IPreUpdateEventListener, IPreInsertEventListener, IPreDeleteEventListener
    {
        /// <summary>
        /// Called when [pre update].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public bool OnPreUpdate(PreUpdateEvent e)
        {
            if (IsAuditable(e.Entity) && e.OldState != null)
            {
                var dirtyFieldIndexes = e.Persister.FindDirty(e.State, e.OldState, e.Entity, e.Session).ToArray();

                if (dirtyFieldIndexes.Any(fieldIndex => ProcessPropertyName(e.Persister.PropertyNames[fieldIndex]) == "IsDeleted"))
                {
                    InsertDeleteAuditLog((Entity<int>)e.Entity, e.Session, AuditType.Delete);
                }

                UpdateAuditLog(e.OldState, e.State, e.Persister.PropertyNames, dirtyFieldIndexes, (Entity<int>)e.Entity, e.Session);
            }
            return false;
        }

        /// <summary>
        /// Called when [pre insert].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public bool OnPreInsert(PreInsertEvent e)
        {
            if (e.Entity != null && IsAuditable(e.Entity))
            {
                InsertDeleteAuditLog((Entity<int>)e.Entity, e.Session, AuditType.Insert);
            }
            return false;
        }

        /// <summary>
        /// Called when [pre delete].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public bool OnPreDelete(PreDeleteEvent e)
        {
            if (e.Entity != null && IsAuditable(e.Entity))
            {
                InsertDeleteAuditLog((Entity<int>)e.Entity, e.Session, AuditType.Delete);
            }
            return false;
        }

        #region Private Helper Methods
        /// <summary>
        /// Determines whether the specified entity is auditable.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the specified entity is auditable; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAuditable(object entity)
        {
            var auditableAttr = entity.GetType().GetCustomAttributes(typeof(AuditableAttribute), false);

            return auditableAttr.Length > 0 && (entity is Entity<int>) && !(entity as Entity<int>).SkipAudit;
        }

        /// <summary>
        /// Updates the audit trail.
        /// </summary>
        /// <param name="oldstate">The oldstate.</param>
        /// <param name="newstate">The newstate.</param>
        /// <param name="names">The names.</param>
        /// <param name="dirtyIndexes">The dirty indexes.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="session">The session.</param>
        private void UpdateAuditLog(IList<object> oldstate, IList<object> newstate, IList<string> names, IEnumerable<int> dirtyIndexes, Entity<int> entity, ISession session)
        {
            if (oldstate == null || newstate == null)
                return;

            var changedValuesLog = new List<AuditChangeLog>();
            foreach (var dirtyIndex in dirtyIndexes)
            {
                if (oldstate[dirtyIndex] == newstate[dirtyIndex])
                    continue;

                if (names[dirtyIndex] != null && _nonAuditableFieldsList.Any(field => field.Equals(names[dirtyIndex], StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                var auditLog = CreateAuditLog(entity, AuditType.Update);

                var propertyName = ProcessPropertyName(names[dirtyIndex]);
                auditLog.PropertyName = propertyName;
                auditLog.OldPropertyValue = GetPropertyValue(oldstate[dirtyIndex], propertyName, entity);
                auditLog.NewPropertyValue = GetPropertyValue(newstate[dirtyIndex], propertyName, entity);


                changedValuesLog.Add(auditLog);
            }

            changedValuesLog.ForEach(session.SaveOrUpdate);
        }

        /// <summary>
        /// Processes the name of the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private string ProcessPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            if (propertyName.StartsWith("_") && propertyName.EndsWith("Xml"))
            {
                propertyName = Inflector.Titleize(propertyName.Replace("_", null).Replace("Xml", null));
            }

            return propertyName;
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="propertyState">State of the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private string GetPropertyValue(object propertyState, string propertyName, IEntity<int> entity)
        {
            if (propertyState != null)
            {
                var propertyType = propertyState.GetType();
                if (propertyType.Name.EqualsIgnoreCase("List`1") || /*propertyType.Name.EqualsIgnoreCase("List") ||*/
                    propertyType.Name.EqualsIgnoreCase("IEnumerable`1") /*|| propertyType.Name.EqualsIgnoreCase("IEnumerable")*/)
                {
                    if (propertyType.GenericTypeArguments[0].IsPrimitive)
                    {
                        switch (propertyType.GenericTypeArguments[0].Name.ToLower())
                        {
                            case "boolean":
                            case "int16":
                            case "int32":
                            case "int64":
                            case "decimal":
                            case "float":
                            case "double":
                                //return XmlHelper.Serialize(propertyState).InnerXml;
                                var objList = propertyState as IList;
                                if (objList == null) 
                                    return XmlHelper.Serialize(propertyState).InnerXml;

                                return string.Join(",", objList.OfType<object>().ToArray<object>());
                            default:
                                return null;
                        }
                    }

                    if(propertyType.GenericTypeArguments[0].Name.ToLower() == "string")
                    {
                        return XmlHelper.Serialize(propertyState).InnerXml;
                    }

                    return XmlHelper.Serialize(propertyState).InnerXml;
                }

                if (propertyType == typeof(XmlDocument))
                {
                    XmlDocument xmlDoc = propertyState as XmlDocument;

                    if(xmlDoc == null) return null;

                    return XmlHelper.Serialize(propertyState).InnerXml;
                }

                return propertyState.ToString();
            }

            return null;
        }

        /// <summary>
        /// Inserts the delete audit log.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="session">The session.</param>
        /// <param name="auditType">Type of the audit.</param>
        private static void InsertDeleteAuditLog(Entity<int> entity, ISession session, AuditType auditType)
        {
            var auditLog = CreateAuditLog(entity, auditType);
            session.SaveOrUpdate(auditLog);
        }

        /// <summary>
        /// Creates the audit log.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="auditType">Type of the audit.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        private static AuditChangeLog CreateAuditLog(Entity<int> entity, AuditType auditType)
        {
            var entityType = entity.GetType();
            var auditableAttr = entityType.GetCustomAttributes(typeof(AuditableAttribute), false);

            if (!auditableAttr.Any())
                throw new InvalidOperationException(string.Format("A valid auditable attribute could not be found on \"{0}\".", entityType.Name));

            var attribute = auditableAttr.OfType<AuditableAttribute>().First();

            // var auditLog = (AuditChangeLog)Activator.CreateInstance(attribute.AuditLogType, new object[] { auditType, entity });
            var auditLog = (AuditChangeLog)Activator.CreateInstance(attribute.AuditLogType);
            auditLog.InitLog(entity);
            auditLog.Action = auditType;
            auditLog.Owner = entity;
            auditLog.OwnerId = entity.Id;
            auditLog.OwnerType = entity.GetType().Name;
            auditLog.ActionBy = MonahrqContext.GetUserName;
            auditLog.CreateDate = DateTime.Now;

            return auditLog;
        }

        #endregion

        private readonly string[] _nonAuditableFieldsList = { "Id", "SkipAudit", "IsChanged", "IsPersisted" };
    }

    #region Reference Code - Please do not delete
    //public class AuditUpdateListener : IPostUpdateEventListener, IPostInsertEventListener
    //{
    //    private static string GetStringValueFromStateArray(object[] stateArray, int position)
    //    {
    //        var value = stateArray[position];

    //        return value == null || value.ToString() == string.Empty
    //                ? null
    //                : value.ToString();
    //    }

    //    public void OnPostUpdate(PostUpdateEvent @event)
    //    {
    //        if (@event.Entity is AuditChangeLog)
    //        {
    //            return;
    //        }

    //        var entityFullName = @event.Entity.GetType().FullName;

    //        if (@event.OldState == null)
    //        {
    //            throw new ArgumentNullException("No old state available for entity type '" + entityFullName +
    //                                            "'. Make sure you're loading it into Session before modifying and saving it.");
    //        }

    //        var dirtyFieldIndexes = @event.Persister.FindDirty(@event.State, @event.OldState, @event.Entity, @event.Session);

    //        var session = @event.Session.GetSession(EntityMode.Poco);

    //        foreach (var dirtyFieldIndex in dirtyFieldIndexes)
    //        {
    //            var oldValue = GetStringValueFromStateArray(@event.OldState, dirtyFieldIndex);
    //            var newValue = GetStringValueFromStateArray(@event.State, dirtyFieldIndex);

    //            if (oldValue == newValue)
    //            {
    //                continue;
    //            }

    //            var isVirtualDelete = @event.Persister.PropertyNames[dirtyFieldIndex] == "IsDeleted";

    //            session.Save(CreateAuditLog((Entity<int>)@event.Entity, AuditType.Update));

    //            //session.Save(new AuditChangeLog
    //            //{
    //            //    Entity = @event.Entity.GetType().Name,
    //            //    FieldName = @event.Persister.PropertyNames[dirtyFieldIndex],
    //            //    EntityFullName = entityFullName,
    //            //    OldValue = oldValue,
    //            //    NewValue = newValue,
    //            //    Username = Environment.UserName,
    //            //    EntityId = (int)@event.Id,
    //            //    AuditEntryType = "Update",
    //            //    Timestamp = DateTime.Now
    //            //});
    //        }

    //        session.Flush();
    //    }

    //    /// <summary>
    //    /// Creates the audit log.
    //    /// </summary>
    //    /// <param name="entity">The entity.</param>
    //    /// <param name="auditType">Type of the audit.</param>
    //    /// <returns></returns>
    //    /// <exception cref="System.InvalidOperationException"></exception>
    //    private static AuditChangeLog CreateAuditLog(Entity<int> entity, AuditType auditType)
    //    {
    //        var entityType = entity.GetType();
    //        var auditableAttr = entityType.GetCustomAttributes(typeof(AuditableAttribute), false);

    //        if (!auditableAttr.Any())
    //            throw new InvalidOperationException(string.Format("A valid auditable attribute could not be found on \"{0}\".", entityType.Name));

    //        var attribute = auditableAttr.OfType<AuditableAttribute>().First();

    //        // var auditLog = (AuditChangeLog)Activator.CreateInstance(attribute.AuditLogType, new object[] { auditType, entity });
    //        var auditLog = (AuditChangeLog)Activator.CreateInstance(attribute.AuditLogType);

    //        auditLog.Action = auditType;
    //        auditLog.Owner = entity;
    //        auditLog.OwnerType = entity.GetType().Name;
    //        auditLog.Owner = entity;
    //        auditLog.ActionBy = MonahrqContext.GetUserName;
    //        auditLog.CreateDate = DateTime.Now;

    //        return auditLog;

    //    }
    //}
    #endregion
}
