using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Monahrq.Sdk.StateMachine
{

    public class TriggerAttribute : Attribute { }

    /// <summary>
    /// Base class for state machines. Implement the WalkStates() method to use.
    public abstract class StateMachine
    {
        private readonly Action<Exception> _exceptionCallback;
        protected abstract IEnumerable<IStateStep> _workflow();
        private readonly IEnumerator<IStateStep> _currentState;

        /// <summary>
        /// Is set to the trigger that was called most recently. Always contains a reference
        /// to a child class property/field decorated with the [Trigger] attribute.
        /// </summary>
        protected Action Trigger { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine"/> class.
        /// </summary>
        /// <param name="exceptionCallback">The exception callback.</param>
        protected StateMachine(Action<Exception> exceptionCallback)
        {
            _exceptionCallback = exceptionCallback;
            _geTriggers();
            _currentState = _workflow().GetEnumerator();
            _currentState.MoveNext();
        }

        /// <summary>
        /// Invoked - moves to next item
        /// </summary>
        private void Invoked()
        {
            Action invoker = null;

            if (!_currentState.MoveNext())
                return;

            invoker = () =>
                {
                  //  Trigger = invoker;

                    var next = _currentState.Current;
                    next.Invoked = Invoked;
                    Trigger = next.Invoked;

                    // call it
                    try
                    {
                        next.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _currentState.Dispose();
                        _exceptionCallback(ex);
                    }
                };
        }
        #region Helper Methods

        /// <summary>
        /// Verifies the type of the member.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="InvalidStateMachineException">Fields/properties decorated with [Trigger] must be of type Action</exception>
        private static void VerifyMemberType(Type type)
        {
            if (type != typeof (Action))
            {
                throw new InvalidStateMachineException(
                    "Fields/properties decorated with [Trigger] must be of type Action");
            }
        }

        /// <summary>
        /// Finds all fields and properties that have the [Trigger] attribute, and assigns a trigger action to them.
        /// </summary>
        private void _geTriggers()
        {
            var type = GetType();
            foreach (var field in TriggerMembers(type.GetFields))
            {
                VerifyMemberType(field.FieldType);
                field.SetValue(this, MakeTrigger());
            }
            foreach (var prop in TriggerMembers(type.GetProperties))
            {
                VerifyMemberType(prop.PropertyType);
                prop.SetValue(this, MakeTrigger(), null);
            }
        }

        /// <summary>
        /// Makes the trigger.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private object MakeTrigger()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invalids the trigger.
        /// </summary>
        /// <exception cref="InvalidTriggerException">Invalid trigger!</exception>
        protected void InvalidTrigger()
        {
            throw new InvalidTriggerException("Invalid trigger!");
        }

        /// <summary>
        /// Gets all members returned by the `getMembers` method that have the TriggerAttribute set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getMembers">The get members.</param>
        /// <returns></returns>
        private static IEnumerable<T> TriggerMembers<T>(Func<BindingFlags, T[]> getMembers) where T : MemberInfo
        {
            return getMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof (TriggerAttribute), false).Any());
        }

        #endregion
    }
}
