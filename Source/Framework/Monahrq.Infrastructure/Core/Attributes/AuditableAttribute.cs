using System;

namespace Monahrq.Infrastructure.Core.Attributes
{
    /// <summary>
    /// This attribute is only indented to be used as a means to identify auditable
    /// domain entities when being modified so use sparingly. Should 
    /// be placed only at the top most level in the object model heiarchy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AuditableAttribute : Attribute
    {
        public AuditableAttribute(Type auditLogType)
        {
            AuditLogType = auditLogType;
        }

        public Type AuditLogType { get; private set; }
    }
}
