//===================================================================================
// Microsoft patterns & practices
// Composite Application Guidance for Windows Presentation Foundation and Silverlight
//===================================================================================
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===================================================================================
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//===================================================================================
using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Properties;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Microsoft.Practices.Prism.Commands
{
    /// <summary>
    /// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Execute"/> and <see cref="CanExecute"/>.
    /// It also implements the <see cref="IActiveAware"/> interface, which is useful when registering this command in a <see cref="CompositeCommand"/> that monitors command's activity.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <remarks>
    /// The constructor deliberately prevent the use of value types.
    /// Because ICommand takes an object, having a value type for T would cause unexpected behavior when CanExecute(null) is called during XAML initialization for command bindings.
    /// Using default(T) was considered and rejected as a solution because the implementor would not be able to distinguish between a valid and defaulted values.
    /// <para/>
    /// Instead, callers should support a value type by using a nullable value type and checking the HasValue property before using the Value property.
    /// <example>
    ///     <code>
    /// public MyClass()
    /// {
    ///     this.submitCommand = new DelegateCommand&lt;int?&gt;(this.Submit, this.CanSubmit);
    /// }
    /// 
    /// private bool CanSubmit(int? customerId)
    /// {
    ///     return (customerId.HasValue &amp;&amp; customers.Contains(customerId.Value));
    /// }
    ///     </code>
    /// </example>
    /// </remarks>
    public class DelegateCommand<T> : DelegateCommandBase        
    {        
        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, (o)=>true)
        {            
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command.  This can be null.</param>
        /// <exception cref="ArgumentNullException">When both <paramref name="executeMethod"/> and <paramref name="canExecuteMethod"/> ar <see langword="null" />.</exception>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod )
            : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);

#if !WINDOWS_PHONE
            Type genericType = typeof(T);

            // DelegateCommand allows object or Nullable<>.  
            // note: Nullable<> is a struct so we cannot use a class constraint.
            if (genericType.IsValueType)
            {
                if ((!genericType.IsGenericType) || (!typeof(Nullable<>).IsAssignableFrom(genericType.GetGenericTypeDefinition())))
                {
                    throw new InvalidCastException(Resources.DelegateCommandInvalidGenericPayloadType);
                }
            }
#endif
        }



        ///<summary>
        ///Determines if the command can execute by invoked the <see cref="Func{T,Bool}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command to determine if it can execute.</param>
        ///<returns>
        ///<see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        ///</returns>
        public bool CanExecute(T parameter)
        {
            return base.CanExecute(parameter);
        }

        ///<summary>
        ///Executes the command and invokes the <see cref="Action{T}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command.</param>
        public void Execute(T parameter)
        {
            base.Execute(parameter);
        }

        #region support for inline dependencies
        private readonly List<string> _dependentPropertyNames;
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand{T}"/> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="canExecuteMethod">The can execute method.</param>
        /// <param name="target">The target.</param>
        /// <param name="dependentPropertyNames">The dependent property names.</param>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod
            , INotifyPropertyChanged target, params string[] dependentPropertyNames)
            : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) {

            _dependentPropertyNames = new List<string>(dependentPropertyNames);
            target.PropertyChanged += TargetPropertyChanged;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand{T}"/> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="canExecuteMethod">The can execute method.</param>
        /// <param name="target">The target.</param>
        /// <param name="dependentPropertyExpressions">The dependent property expressions.</param>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod
            , INotifyPropertyChanged target, params Expression<Func<object>>[] dependentPropertyExpressions)
            : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) {

            _dependentPropertyNames = new List<string>();
            foreach (var body in dependentPropertyExpressions.Select(expression => expression.Body)) {
                var expression = body as MemberExpression;
                if (expression != null) _dependentPropertyNames.Add(expression.Member.Name);
                else {
                    var unaryExpression = body as UnaryExpression;
                    if (unaryExpression != null) {
                        _dependentPropertyNames.Add(((MemberExpression)unaryExpression.Operand).Member.Name);
                    }
                }
            }
            target.PropertyChanged += TargetPropertyChanged;

        }

        private void TargetPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (_dependentPropertyNames.Contains(e.PropertyName)) {
                RaiseCanExecuteChanged();
            }
        }
        #endregion


    }

    /// <summary>
    /// An <see cref="ICommand"/> whose delegates do not take any parameters for <see cref="Execute"/> and <see cref="CanExecute"/>.
    /// </summary>
    /// <seealso cref="DelegateCommandBase"/>
    /// <seealso cref="DelegateCommand{T}"/>
    public class DelegateCommand : DelegateCommandBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="DelegateCommand"/> with the <see cref="Action"/> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.</param>
        public DelegateCommand(Action executeMethod) : this(executeMethod, ()=>true)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DelegateCommand"/> with the <see cref="Action"/> to invoke on execution
        /// and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.</param>
        /// <param name="canExecuteMethod">The <see cref="Func{TResult}"/> to invoke when <see cref="ICommand.CanExecute"/> is called</param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
            :base((o) => executeMethod(), (o)=>canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
        }


        ///<summary>
        /// Executes the command.
        ///</summary>
        public void Execute()
        {
            Execute(null);
        }

        /// <summary>
        /// Determines if the command can be executed.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command can execute,otherwise returns <see langword="false"/>.</returns>
        public bool CanExecute()
        {
            return CanExecute(null);
        }

        #region support for inline dependencies
        private readonly List<string> _dependentPropertyNames;
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="canExecuteMethod">The can execute method.</param>
        /// <param name="target">The target.</param>
        /// <param name="dependentPropertyNames">The dependent property names.</param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod
            , INotifyPropertyChanged target, params string[] dependentPropertyNames)
            : base((o) => executeMethod(), (o) => canExecuteMethod()) {

            _dependentPropertyNames = new List<string>(dependentPropertyNames);
            target.PropertyChanged += TargetPropertyChanged;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="canExecuteMethod">The can execute method.</param>
        /// <param name="target">The target.</param>
        /// <param name="dependentPropertyExpressions">The dependent property expressions.</param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod
            , INotifyPropertyChanged target, params Expression<Func<object>>[] dependentPropertyExpressions)
            : base((o) => executeMethod(), (o) => canExecuteMethod()) {

            _dependentPropertyNames = new List<string>();
            foreach (var body in dependentPropertyExpressions.Select(expression => expression.Body)) {
                var expression = body as MemberExpression;
                if (expression != null) _dependentPropertyNames.Add(expression.Member.Name);
                else {
                    var unaryExpression = body as UnaryExpression;
                    if (unaryExpression != null) {
                        _dependentPropertyNames.Add(((MemberExpression)unaryExpression.Operand).Member.Name);
                    }
                }
            }
            target.PropertyChanged += TargetPropertyChanged;

        }

        private void TargetPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (_dependentPropertyNames.Contains(e.PropertyName)) {
                RaiseCanExecuteChanged();
            }
        }
        #endregion


    }
    
}
