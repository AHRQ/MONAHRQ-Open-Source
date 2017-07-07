using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity;
using Monahrq.Sdk.Events;


namespace Monahrq.Sdk.Model
{
    /// <summary>
    ///     Base class for items that implement property changed
    /// </summary>
    /// <remarks>
    ///     Code for this section was inspired by two other projects: 
    ///     1. The PRISM implementation and MVVM guidance here: http://compositewpf.codeplex.com/
    ///     2. The Caliburn.Micro framework here: http://caliburnmicro.codeplex.com/ 
    /// </remarks>
    public abstract class BaseNotify : BaseNotifyBase, INotifyDataErrorInfo //,  INotifyPropertyChanged, IDisposable
    {
        #region Commands

        /// <summary>
        /// Gets or sets the view import sample command.
        /// </summary>
        /// <value>
        /// The view import sample command.
        /// </value>
        public DelegateCommand<string> ViewImportSampleCommand { get; set; }

        #endregion

        #region INotifyDataErrorInfo

        /// <summary>
        /// The errors
        /// </summary>
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _errors =
            new ConcurrentDictionary<string, IEnumerable<string>>();

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve validation errors for; or null or <see cref="F:System.String.Empty" />, to retrieve entity-level errors.</param>
        /// <returns>
        /// The validation errors for the property or entity.
        /// </returns>
        public IEnumerable GetErrors(string propertyName)
        {
            IEnumerable<string> errors;
            _errors.TryGetValue(propertyName ?? string.Empty, out errors);
            return errors;
        }

        /// <summary>
        /// Clears the errors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpresssion">The property expresssion.</param>
        protected virtual void ClearErrors<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            if (!_errors.ContainsKey(propertyName)) return;

            IEnumerable<string> removed;
            _errors.TryRemove(propertyName, out removed);
            RaiseErrorsChanged(propertyExpresssion);
        }

        /// <summary>
        /// Gets a value that indicates whether the entity has validation errors.
        /// </summary>
        public virtual bool HasErrors
        {
            get { return _errors.Any(); }
        }

        /// <summary>
        /// Raises the errors changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaiseErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises the errors changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpresssion">The property expresssion.</param>
        protected virtual void RaiseErrorsChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Validates the asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task ValidateAsync()
        {
            return Task.Run(() => Validate());
        }

        /// <summary>
        /// The lock
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public virtual void Validate()
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();

                Validator.TryValidateObject(this, validationContext, validationResults, true);

                _errors.ToList().ForEach(x =>
                {
                    if ((validationResults.All(r => r.MemberNames.All(m => m != x.Key))))
                    {
                        IEnumerable<string> outList;
                        var tryRemove = _errors.TryRemove(x.Key, out outList);
                        RaiseErrorsChanged(x.Key);
                    }
                });

                var q = from r in validationResults
                        from m in r.MemberNames
                        group r by m
                            into g
                        select g;

                q.ToList().ForEach(x =>
                {
                    var messages = x.Select(r => r.ErrorMessage).ToList();
                    if (_errors.ContainsKey(x.Key))
                    {
                        IEnumerable<string> outList;
                        var tryRemove = _errors.TryRemove(x.Key, out outList);
                    }
                    _errors.TryAdd(x.Key, messages);
                    RaiseErrorsChanged(x.Key);
                });

            }
        }

        /// <summary>
        /// Determines whether the specified properties to check has error.
        /// </summary>
        /// <param name="propertiesToCheck">The properties to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified properties to check has error; otherwise, <c>false</c>.
        /// </returns>
        public bool HasError(List<string> propertiesToCheck)
        {
            return propertiesToCheck != null && _errors.Keys.Any(propertiesToCheck.Contains);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IDisposable" />
    public abstract class BaseNotifyBase : INotifyPropertyChanged, IDisposable
    {
        #region Constructor.
        public BaseNotifyBase() : base()
        {
            interceptor = null;
        }
        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected internal virtual void RaisePropertyChanged(string propertyName)
        {
            if (interceptor != null && !interceptor.OnRaisePropertyChanged(this, propertyName))
                return;

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event for each of the properties.
        /// </summary>
        /// <param name="propertyNames">The properties that have a new value.</param>
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                RaisePropertyChanged(name);
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property that has a new value</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = ExtractPropertyName(propertyExpression);
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Extracts the property name from the property expression
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyExpression">An expression that evaluates to the property</param>
        /// <returns>The property name</returns>
        /// <remarks>
        /// Use this to take an expression like <code>() => MyProperty</code> and evaluate to the
        /// string "MyProperty"
        /// </remarks>
        protected string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(@"propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(Resources.BaseNotify_ExtractPropertyName_NotAMember, @"propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(Resources.BaseNotify_ExtractPropertyName_NotAProperty, @"propertyExpression");
            }

            var getMethod = property.GetGetMethod(true);

            if (getMethod == null)
            {
                // this shouldn't happen - the expression would reject the property before reaching this far
                throw new ArgumentException(Resources.BaseNotify_ExtractPropertyName_NoGetter, @"propertyExpression");
            }

            if (getMethod.IsStatic)
            {
                throw new ArgumentException(Resources.BaseNotify_ExtractPropertyName_Static, @"propertyExpression");
            }

            return memberExpression.Member.Name;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposed)
        {
            if (disposed) return;
            disposed = true;
        }

        #endregion

        #region Interceptor.
        private IBaseNotifyInterceptor interceptor;
        /// <summary>
        /// Sets the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        public void SetInterceptor(IBaseNotifyInterceptor interceptor) { this.interceptor = interceptor; }
        #endregion
    }
}
