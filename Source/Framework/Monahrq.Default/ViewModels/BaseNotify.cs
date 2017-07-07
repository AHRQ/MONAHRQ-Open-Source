using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Default.ViewModels
{
    /// <summary>
    /// class for norifying property change events
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class BaseNotify : IPartImportsSatisfiedNotification, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <exception cref="InvalidOperationException">NotifyPropertyChanged() can only by invoked within a property setter.</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void RaisePropertyChanged()
        {
            var frames = new System.Diagnostics.StackTrace();
            for (var i = 0; i < frames.FrameCount; i++)
            {
                var frame = frames.GetFrame(i).GetMethod() as MethodInfo;
                if (frame != null)
                    if (frame.IsSpecialName && frame.Name.StartsWith("set_"))
                    {
                        RaisePropertyChanged(frame.Name.Substring(4));
                        return;
                    }
            }
            throw new InvalidOperationException("NotifyPropertyChanged() can only by invoked within a property setter.");
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                // BUG: this gets null reference in ListCollectionView when user adds a state, then clicks Apply Context in Hospital tab
                try
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
                catch
                { }
            }
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyNames">The property names.</param>
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                RaisePropertyChanged(name);
            }
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = ExtractPropertyName(propertyExpression);
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Extracts the name of the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">propertyExpression</exception>
        /// <exception cref="ArgumentException">
        /// BaseNotify: Property is not a member;propertyExpression
        /// or
        /// BaseNotify: Property is not a property;propertyExpression
        /// or
        /// BaseNotify: Property has no getter;propertyExpression
        /// or
        /// BaseNotify: Property is static;propertyExpression
        /// </exception>
        protected string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(@"BaseNotify: Property is not a member", "propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(@"BaseNotify: Property is not a property", "propertyExpression");
            }

            var getMethod = property.GetGetMethod(true);

            if (getMethod == null)
            {

                throw new ArgumentException(@"BaseNotify: Property has no getter", "propertyExpression");
            }

            if (getMethod.IsStatic)
            {
                throw new ArgumentException(@"BaseNotify: Property is static", "propertyExpression");
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {

        }

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IEventAggregator Events { get; set; }
    }
}
