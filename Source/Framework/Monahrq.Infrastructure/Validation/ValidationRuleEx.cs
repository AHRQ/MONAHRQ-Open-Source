using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Monahrq.Infrastructure.Validation
{
	public abstract class ValidationRuleEx : ValidationRule
	{
		/// <summary>
		/// GetBoundValue pulls the underlying value out of the input 'value'.  It is used by a deriving custom ValidationRule
		/// to determine the value that was sent to the rule.  Most of the time, value directly holds the needed value.
		/// However, if the ValidationStep of the ValidationRule was set to 'UpdatedValue' or 'CommitedValue', wpf will
		/// instead send BindingExpression (binding to the target Property).  Thus this utility method can be used to get
		/// the needed value despite the given ValidationStep.
		/// 
		/// All custom ValidationRule should derive from ValidationRuleEx and use the GetBoundValue instead of using value directly
		/// to support having the Rule applied at different ValidationSteps.
		/// </summary>
		/// <example>
		///		<StringRangeRule Min="1" Max="200" .... ValidationStep="UpdatedValue" />
		///		-- Because StringRangeRule's ValidationStep was set to "UpdatedValue", WPF will send StringRangeRule::Validate
		///		   value parameter a BindingExpression instead of the assumed string.
		///		-- StringRangeRule would thus derive from ValidatoinRuleEx and
		///		   StringRangeRule::Validation(object value, ...) method would call:
		///				var strValue = (string)GetBoundValue(value);
		/// </example>
		/// <param name="value"></param>
		/// <returns></returns>
		static public object GetBoundValue(object value)
		{
			if (value is BindingExpression)
			{
				// ValidationStep was UpdatedValue or CommittedValue (validate after setting)
				// Need to pull the value out of the BindingExpression.
				BindingExpression binding = (BindingExpression)value;

				// Get the Property.  If cannot, return null.  
				var resolvedProperty = binding.GetType().GetProperty("ResolvedSourcePropertyName", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).GetValue(binding, null);
				if (resolvedProperty == null) return null;

				// Get the bound object and name of the property
				string resolvedPropertyName = resolvedProperty.ToString();
				object resolvedSource = binding.GetType().GetProperty("ResolvedSource", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).GetValue(binding, null);

				// Extract the value of the property
				object propertyValue = resolvedSource.GetType().GetProperty(resolvedPropertyName).GetValue(resolvedSource, null);

				return propertyValue;
			}
			else
			{
				return value;
			}
		}
		
		static public T GetBoundValue<T>(object value)  where T : class
		{
			return GetBoundValue(value) as T;
		}
	}
}
