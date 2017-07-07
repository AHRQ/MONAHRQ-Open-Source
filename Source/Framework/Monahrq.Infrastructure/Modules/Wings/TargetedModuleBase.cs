using System.ComponentModel.Composition;
using System.Reflection;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Sdk.Modules.Wings
{
    /// <summary>
    /// A <see cref="WingModule"/> for a <see cref="Target"/> type defined by <typeparamref name="T"/>
    /// </summary>
    /// <seealso cref="DatasetRecord"/>
    /// <typeparam name="T">The <see cref="DatasetRecord"/> type that this <see cref="WingModule"/> describes</typeparam>
    public abstract class TargetedModuleBase<T> : WingModule, ITargetedModuleBase
        where T : DatasetRecord
    {
        /// <summary>
        /// Gets the <see cref="IDomainSessionFactoryProvider"/>
        /// </summary>
        [Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
        protected IDomainSessionFactoryProvider SessionFactoryProvider
        {
            get;
            private set;
        }

        /// <summary>
        /// Provides a reference to the <see cref="WingTargetAttribute"/> decorating this <see cref="TargetedModuleBase{T}"/>
        /// </summary>
        public WingTargetAttribute TargetAttribute
        {
            get
            {
                //todo: cache the result!
                var attr = typeof(T).GetCustomAttribute<WingTargetAttribute>();
                if (attr == null) // TODO: Really guys? What is wrong with this code? This is not defensive propgramming at all. Jason
                {
                    attr = new WingTargetAttribute(string.Empty, attr.Guid.ToString(), false);
                }
                return attr;
            }
        }
    }
}