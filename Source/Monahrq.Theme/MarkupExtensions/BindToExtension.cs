using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using System.ComponentModel;
using System.Xaml;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Globalization;
using System.Windows.Controls;
using System.Collections.ObjectModel;


namespace Monahrq.Theme.MarkupExtensions
{
	//[MarkupExtensionReturnType(typeof(BindingExpression))]
	public class BindToExtension : MarkupExtension
	{

		#region Constructor Methods.
		public BindToExtension()
		{
			this.sourceReference = null;
		}

		public BindToExtension(string path)
		{
			this.path = path;
			this.sourceReference = null;
		}
		#endregion

		#region Process Path Methods.
		private bool ProcessPath(IServiceProvider serviceProvider)
		{
			////////////////////////////////////////////////////////////////////////////////
			//	Validation.
			////////////////////////////////////////////////////////////////////////////////
			if (string.IsNullOrWhiteSpace(this.path))
			{
				binding = new Binding();
				return true;
			}

			////////////////////////////////////////////////////////////////////////////////
			//	Variables.
			////////////////////////////////////////////////////////////////////////////////
			var parts = this.path.Split('.').Select(o => o.Trim()).ToArray();
			var partIndex = 0;

			RelativeSource oRelativeSource = null;
			Object oSource = null;
			string elementName = null;

			////////////////////////////////////////////////////////////////////////////////
			//	Determine and process the 'entry' binding type.
			////////////////////////////////////////////////////////////////////////////////
			if (parts[0].StartsWith("#"))
			{
				elementName = parts[0].Substring(1);
				partIndex++;
			}
			else if (parts[0].ToLower() == "ancestors" || parts[0].ToLower() == "ancestor")
			{
				if (parts.Length < 2) throw new Exception("Invalid path, expected exactly 2 identifiers ancestors.#Type#.[Path] (e.g. Ancestors.DataGrid, Ancestors.DataGrid.SelectedItem, Ancestors.DataGrid.SelectedItem.Text)");
				var sType = parts[1];
				var oType = (Type)new System.Windows.Markup.TypeExtension(sType).ProvideValue(serviceProvider);
				if (oType == null) throw new Exception("Could not find type: " + sType);
				oRelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, oType, 1);
				partIndex += 2;
			}
			else if (parts[0].ToLower() == "template" || parts[0].ToLower() == "templateparent" || parts[0].ToLower() == "templatedparent" || parts[0].ToLower() == "templated")
			{
				oRelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
				partIndex++;
			}
			else if (parts[0].ToLower() == "thiswindow" || parts[0].ToLower() == "window")
			{
				oRelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1);
				partIndex++;
			}
			else if (parts[0].ToLower() == "root")
			{
				IRootObjectProvider rootProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
				oSource = (rootProvider != null) ? rootProvider.RootObject : null;
				partIndex++;
			}
			else if (parts[0].ToLower() == "this")
			{
				oRelativeSource = new RelativeSource(RelativeSourceMode.Self);
				partIndex++;
			}

			////////////////////////////////////////////////////////////////////////////////
			//	Advanced path.
			////////////////////////////////////////////////////////////////////////////////
			var partsForPathString = parts.Skip(partIndex);
			IValueConverter callMethodConverter = null;

			////////////////////////////////////////////////////////////////////////////////
			//	Special processing for binding to Methods and Commands.
			////////////////////////////////////////////////////////////////////////////////
			if (partsForPathString.Any())
			{
				var sLastPartForPathString = partsForPathString.Last();

				if (sLastPartForPathString.EndsWith("()"))
				{
					// Retrieve target info.
					IProvideValueTarget provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
					DependencyObject targetObject = null;
					object targetProperty = null;
					if (provideValueTarget != null)
					{
						targetObject = provideValueTarget.TargetObject as DependencyObject;
						targetProperty = provideValueTarget.TargetProperty;
					}
					if (targetProperty != null && targetProperty is DependencyProperty)
					{
						partsForPathString = partsForPathString.Take(partsForPathString.Count() - 1);
						methodName = sLastPartForPathString.Remove(sLastPartForPathString.Length - 2);
						callMethodConverter = new CallMethodValueConverter(methodName);
					}
					else if (targetProperty != null && targetProperty.GetType().FullName.Equals("System.Reflection.RuntimeEventInfo"))
					//	else if (targetProperty != null  && targetProperty is System.Reflection.RuntimeEventInfo)
					//	RuntimeEventInfo is locked away where I cant even compare against the type directly.
					{
						InternalBindToMethod(oSource, sLastPartForPathString, targetObject, targetProperty);

						//  Return false indicating Parsing is done and we don't want to do any actual XAML binding.
						//  We've already bound the proxy directly to the event object.
						return false;
					}
				}
			}

			////////////////////////////////////////////////////////////////////////////////
			//	Compute the 'Path'
			////////////////////////////////////////////////////////////////////////////////
			var bindingPath = string.Join(".", partsForPathString.ToArray());

			////////////////////////////////////////////////////////////////////////////////
			//	Build the Binding element.
			////////////////////////////////////////////////////////////////////////////////
			if (string.IsNullOrWhiteSpace(bindingPath))
			{
				binding = new Binding();
			}
			else
			{
				binding = new Binding(bindingPath);
			}

			if (elementName != null)
			{
				binding.ElementName = elementName;
			}

			if (oRelativeSource != null)
			{
				binding.RelativeSource = oRelativeSource;
			}

			if (oSource != null)
			{
				binding.Source = oSource;
			}

			if (callMethodConverter != null)
			{
				binding.Converter = callMethodConverter;
			}
			else if (this.converter != null)
			{
				binding.Converter = this.converter;
			}


			////////////////////////////////////////////////////////////////////////////////
			//	Build - Propagate Properties.
			//	- It turns out that property propagation validation is problematic.  We were trying to
			//	  determine if the user was trying to overwrite a property that previous processing has
			//	  already explicitly set.  In this situation we wanted to throw error/exception.  
			//	- However, this is a problem in that several of Binding's properties are of value type and/or
			//	  get default values, so it is non trivial to determine if the above processing has set the value
			//	  or if it is just a default value; add to this that that default value can be different
			//	  based on which XAML property type is being bound to. 
			//	- So to make this work completely, we'd have to depend solely on the processing the above
			//	  does; i.e. keep track of which binding properties the processing does.
			////////////////////////////////////////////////////////////////////////////////			
			ValidatePropagateProperty(binding.AsyncState, AsyncState);
			//	ValidatePropagateProperty(binding.BindsDirectlyToSource 		,	BindsDirectlyToSource);
			ValidatePropagateProperty(binding.ConverterCulture, ConverterCulture);
			ValidatePropagateProperty(binding.ConverterParameter, ConverterParameter);
			ValidatePropagateProperty(binding.ElementName, ElementName);
			//	ValidatePropagateProperty(binding.IsAsync 						,	IsAsync);
			//	ValidatePropagateProperty(binding.Mode 							,	Mode);
			//	ValidatePropagateProperty(binding.NotifyOnSourceUpdated 		,	NotifyOnSourceUpdated);
			//	ValidatePropagateProperty(binding.NotifyOnTargetUpdated 		,	NotifyOnTargetUpdated);
			//	ValidatePropagateProperty(binding.NotifyOnValidationError 		,	NotifyOnValidationError);
			//ValidatePropagateProperty(binding.RelativeSource 				,	RelativeSource);
			//ValidatePropagateProperty(binding.Source 						,	Source);
			//ValidatePropagateProperty(binding.Path 						,	Path);
			ValidatePropagateProperty(binding.UpdateSourceExceptionFilter, UpdateSourceExceptionFilter);
			//	ValidatePropagateProperty(binding.UpdateSourceTrigger 			,	UpdateSourceTrigger);
			//	ValidatePropagateProperty(binding.ValidatesOnDataErrors 		,	ValidatesOnDataErrors);
			//	ValidatePropagateProperty(binding.ValidatesOnExceptions 		,	ValidatesOnExceptions);
			//	ValidatePropagateProperty(binding.ValidatesOnNotifyDataErrors   ,	ValidatesOnNotifyDataErrors);
			//ValidatePropagateProperty(binding.ValidationRules { get); }	,	ValidationRules { get); }
			ValidatePropagateProperty(binding.XPath, XPath);

			if (AsyncState.HasValue())					binding.AsyncState = AsyncState;
			if (BindsDirectlyToSource.HasValue)			binding.BindsDirectlyToSource = BindsDirectlyToSource.Value;
			if (ConverterCulture.HasValue())			binding.ConverterCulture = ConverterCulture;
			if (ConverterParameter.HasValue())			binding.ConverterParameter = ConverterParameter;
			if (ElementName.HasValue())					binding.ElementName = ElementName;
			if (IsAsync.HasValue)						binding.IsAsync = IsAsync.Value;
			if (Mode.HasValue)							binding.Mode = Mode.Value;
			if (NotifyOnSourceUpdated.HasValue)			binding.NotifyOnSourceUpdated = NotifyOnSourceUpdated.Value;
			if (NotifyOnTargetUpdated.HasValue)			binding.NotifyOnTargetUpdated = NotifyOnTargetUpdated.Value;
			if (NotifyOnValidationError.HasValue)		binding.NotifyOnValidationError = NotifyOnValidationError.Value;
			//binding.RelativeSource 				= RelativeSource;
			//binding.Source 						= Source;
			//binding.Path 							= Path;
			if (UpdateSourceExceptionFilter.HasValue()) binding.UpdateSourceExceptionFilter = UpdateSourceExceptionFilter;
			if (UpdateSourceTrigger.HasValue)			binding.UpdateSourceTrigger = UpdateSourceTrigger.Value;
			if (ValidatesOnDataErrors.HasValue)			binding.ValidatesOnDataErrors = ValidatesOnDataErrors.Value;
			if (ValidatesOnExceptions.HasValue)			binding.ValidatesOnExceptions = ValidatesOnExceptions.Value;
			if (ValidatesOnNotifyDataErrors.HasValue)	binding.ValidatesOnNotifyDataErrors = ValidatesOnNotifyDataErrors.Value;
			if (Delay.HasValue)							binding.Delay = Delay.Value;
			//binding.ValidationRules				= ValidationRules { get; }
			if (XPath.HasValue()) binding.XPath = XPath;

			////////////////////////////////////////////////////////////////////////////////
			//	Binding property set successfully.
			////////////////////////////////////////////////////////////////////////////////
			return true;
		}
		private void ValidatePropagateProperty<T>(T bindingProp, T userProp)
		{
			if (userProp != null && bindingProp != null)
			{
				throw new Exception("User value conflicts with BindTo functionality.");
			}
		}

		#endregion

		#region Process Proxy Methods.
		private void ProcessAbsoluteBinding(IServiceProvider serviceProvider)
		{
			if (binding == null) throw new ArgumentException("Binding == null");

			Binding absoluteBinding = null;

			//  ElementName gets replaced with a reference.
			if (binding.ElementName != null)
			{
				var reference = new Reference(binding.ElementName);
				var source = reference.ProvideValue(serviceProvider);
				if (source == null)
				{
					throw new ArgumentException("Could not resolve element");
				}
				absoluteBinding = CreateElementNameBinding(binding, source);
			}
			// RelativeSource cannot work with AbsoulteSource
			else if (binding.RelativeSource != null)
			{
				throw new ArgumentException("RelativeSource not supported with 'Absolute' SourceReference");
			}
			// Default to Root Object.
			else
			{
				var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
				if (rootObjectProvider == null)
				{
					throw new ArgumentException("rootObjectProvider == null");
				}
				absoluteBinding = CreateDataContextBinding((FrameworkElement)rootObjectProvider.RootObject, binding);
			}

			binding = absoluteBinding;
		}
		private static Binding CreateElementNameBinding(Binding original, object source)
		{
			var binding = new Binding()
			 {
				 Path = original.Path,
				 Source = source,
			 };
			SyncProperties(original, binding);
			return binding;
		}
		private static Binding CreateDataContextBinding(FrameworkElement rootObject, Binding original)
		{
			string path = string.Format("{0}.{1}", FrameworkElement.DataContextProperty.Name, original.Path.Path);
			var binding = new Binding(path)
			 {
				 Source = rootObject,
			 };
			SyncProperties(original, binding);
			return binding;
		}
		private static void SyncProperties(Binding source, Binding target)
		{
			foreach (var copyProperty in copyProperties)
			{
				var value = copyProperty.GetValue(source);
				copyProperty.SetValue(target, value);
			}
			foreach (var rule in source.ValidationRules)
			{
				target.ValidationRules.Add(rule);
			}
		}
		#endregion

		#region ProvideValue Method.
		private bool IsInDesignMode
		{
			get { return DesignerProperties.GetIsInDesignMode(dependencyObject); }
		}
		private static object DefaultValue(IServiceProvider serviceProvider)
		{
			var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
			if (provideValueTarget == null)
			{
				throw new ArgumentException("provideValueTarget == null");
			}
			var dependencyProperty = (DependencyProperty)provideValueTarget.TargetProperty;
			return dependencyProperty.DefaultMetadata.DefaultValue;
		}
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (IsInDesignMode) return DefaultValue(serviceProvider);


			if (!ProcessPath(serviceProvider)) return null;
			if (SourceReference == ESourceReference.Absolute) ProcessAbsoluteBinding(serviceProvider);
			var provideValue = binding.ProvideValue(serviceProvider);
			return provideValue;
		}
		#endregion

		#region Properties.

		#region BindTo Special Properties.
		[ConstructorArgument("path")]
		public string Path
		{
			get { return this.path; }
			set { this.path = value; }
		}
		[DefaultValue("")]
		[ConstructorArgument("converter")]
		public IValueConverter Converter
		{
			get { return this.converter; }
			set { this.converter = value; }
		}
		public ESourceReference SourceReference
		{
			get { return this.sourceReference.HasValue ? this.sourceReference.Value : ESourceReference.Relative; }
			set { this.sourceReference = value; }
		}
		#endregion
		#region Binding Properties.
		public object AsyncState { get; set; }
		public bool? BindsDirectlyToSource { get; set; }
		public CultureInfo ConverterCulture { get; set; }
		public object ConverterParameter { get; set; }
		public string ElementName { get; set; }
		public bool? IsAsync { get; set; }
		public BindingMode? Mode { get; set; }
		public bool? NotifyOnSourceUpdated { get; set; }
		public bool? NotifyOnTargetUpdated { get; set; }
		public bool? NotifyOnValidationError { get; set; }
		public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter { get; set; }
		public UpdateSourceTrigger? UpdateSourceTrigger { get; set; }
		public bool? ValidatesOnDataErrors { get; set; }
		public bool? ValidatesOnExceptions { get; set; }
		public bool? ValidatesOnNotifyDataErrors { get; set; }
		//public Collection<ValidationRule> ValidationRules { get; }
		public string XPath { get; set; }
		public int? Delay { get; set; }
		#endregion
		#endregion

		#region Variables.
		private Binding binding;
		private string path;
		private string methodName;
		private IValueConverter converter;
		private ESourceReference? sourceReference;

		private static readonly DependencyObject dependencyObject = new DependencyObject();
		private static readonly string[] doNotCopy = { "Path", "Source", "ElementName", "RelativeSource", "ValidationRules" };
		private static readonly PropertyInfo[] copyProperties = typeof(Binding).GetProperties().Where(x => !doNotCopy.Contains(x.Name)).ToArray();

		#endregion

		#region Types.
		public enum ESourceReference
		{
			Relative,
			Absolute,
		}
		#endregion

		#region Bind XAML Events to Methods.
		private void InternalBindToMethod(Object oSource, string sLastPartForPathString, DependencyObject targetObject, object targetProperty)
		{
			// For Event to Method bindings I think the goal is to not return a 'binding' or function, but
			// instead to grab the underlying TargetObject event and to add a handler to it directly; while
			// returning null from this Binding call.
			// The new Handler that is added to the event would actually be an proxy (ICommand?) call method
			// that would store the name of target method.
			// Proxy Method/Object:
			//	- At creation it would resolve the execute method and it's like named 'Can' method.
			//	- At point of raised event, It would check the Can method and Execute bound method.
			// The main trick is to find a way to retain this state information, and have it available to
			// a handler that can be bound to the targetProperty 'RuntimeEventInfo' object.

			//	Get Method Name.
			methodName = sLastPartForPathString.Remove(sLastPartForPathString.Length - 2);

			//	Get Handlers.
			Action<object, object> handler = null;
			var handlerInfo = oSource.GetType().GetMethod(methodName);
			if (handlerInfo == null)
				throw new Exception(String.Format("Error: Could not bind to method: {0}.  Method not found.", methodName));
			var handlerParameters = handlerInfo.GetParameters();
			if (handlerParameters.Count() == 0) handler = (object sender, object eventArgs) => handlerInfo.Invoke(oSource, new object[] { });
			else if (handlerParameters.Count() == 1) handler = (object sender, object eventArgs) => handlerInfo.Invoke(oSource, new object[] { sender });
			else if (handlerParameters.Count() == 2) handler = (object sender, object eventArgs) => handlerInfo.Invoke(oSource, new object[] { sender, eventArgs });
			else handler = null;


			Func<object, object, bool> canHandler = null;
			var canHandlerInfo = oSource.GetType().GetMethod("Can" + methodName);
			if (canHandlerInfo != null)
			{
				var canHandlerParameters = canHandlerInfo.GetParameters();
				if (canHandlerParameters.Count() == 0) canHandler = (object sender, object eventArgs) => { return (bool)canHandlerInfo.Invoke(oSource, new object[] { }); };
				else if (canHandlerParameters.Count() == 1) canHandler = (object sender, object eventArgs) => { return (bool)canHandlerInfo.Invoke(oSource, new object[] { sender }); };
				else if (canHandlerParameters.Count() == 2) canHandler = (object sender, object eventArgs) => { return (bool)canHandlerInfo.Invoke(oSource, new object[] { sender, eventArgs }); };
				else canHandler = null;
			}

			//  Formulate the proxy lambda method that performs the action of calling the action method and it 'Can' if present.
			//  Basically when this lambda is called, a check is made against the 'Can' method.  If that is true, or it doesn't exist. the
			//  main action handler is called.
			//	Maybe in future we can add a parameter passing.
			Action<object, object> proxyHandler = (object sender, object eventArgs) =>
			{
				if (canHandler == null || canHandler(sender, eventArgs))
					handler(sender, eventArgs);
			};

			//	Get Event being bound to.
			var targetPropertyName = (String)targetProperty.GetType().GetProperty("Name").GetValue(targetProperty);
			var targetEventInfo = targetObject.GetType().GetRuntimeEvent(targetPropertyName);

			//	Add (proxy) handler to Event.
			MethodInfo addMethod = targetEventInfo.GetAddMethod();
			MethodInfo removeMethod = targetEventInfo.GetRemoveMethod();
			var addParameters = addMethod.GetParameters();
			var delegateType = addParameters[0].ParameterType;
			MethodInfo handlerInvoke = typeof(Action<object, object>).GetMethod("Invoke");
			Delegate delegatex = handlerInvoke.CreateDelegate(delegateType, proxyHandler);

			addMethod.Invoke(targetObject, new object[] { delegatex });
			//Func<EventRegistrationToken> add = () => (EventRegistrationToken)addMethod.Invoke(targetObject, new object[] { delegatex });
			//Action<EventRegistrationToken> remove = t => removeMethod.Invoke(targetObject, new object[] { t });
		}
		#endregion

		#region Bind XAML Commands To Methods.
		private class CallMethodValueConverter : IValueConverter
		{

			#region Constructor Methods.
			public CallMethodValueConverter(string psMethodName)
			{
				msMethodName = psMethodName;
			}
			#endregion

			#region IValueConverter Methods.
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				if (value == null) return null;
				return new CallMethodCommand(value, msMethodName);
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				throw new NotImplementedException();
			}
			#endregion

			#region Variables.
			private string msMethodName;
			#endregion
		}

		private class CallMethodCommand : ICommand
		{

			#region Constructor Methods.
			public CallMethodCommand(object poObject, string psMethodName)
			{
				moObject = poObject;

				moMethodInfo = moObject.GetType().GetMethod(psMethodName);
				if (moMethodInfo == null) throw new Exception(String.Format("Method '{0}' does not exists to bind to.",psMethodName));

				var aParameters = moMethodInfo.GetParameters();
				if (aParameters.Length > 2) throw new Exception("You can only bind to methods that take 0 or 1 parameters.");

				moCanMethodInfo = moObject.GetType().GetMethod("Can" + psMethodName);
				if (moCanMethodInfo != null)
				{
					if (moCanMethodInfo.ReturnType != typeof(bool)) throw new Exception("'Can' method must return boolean.");

					var aCanParameters = moMethodInfo.GetParameters();
					if (aCanParameters.Length > 2) throw new Exception("You can only bind to a methods take take 0 or 1 parameters.");
					mbCanMethodAcceptsParameter = aParameters.Any();
				}

				mbMethodAcceptsParameter = aParameters.Any();
			}
			#endregion

			#region Execute Methods.
			public bool CanExecute(object parameter)
			{
				if (moCanMethodInfo == null) return true;

				var aParameters = !mbMethodAcceptsParameter ? null : new[] { parameter };
				return (bool)moCanMethodInfo.Invoke(moObject, aParameters);
			}

//#pragma warning disable 67 // CanExecuteChanged is not being used but is required by ICommand
//			public event EventHandler CanExecuteChanged;
//#pragma warning restore 67 // CanExecuteChanged is not being used but is required by ICommand
			public event EventHandler CanExecuteChanged
			{
				add { CommandManager.RequerySuggested += value; }
				remove { CommandManager.RequerySuggested -= value; }
			}

			public void Execute(object parameter)
			{
				var aParameters = !mbMethodAcceptsParameter ? null : new[] { parameter };
				moMethodInfo.Invoke(moObject, aParameters);
			}
			#endregion

			#region Variables.
			private readonly object moObject;

			private readonly MethodInfo moMethodInfo;
			private readonly bool mbMethodAcceptsParameter;

			private readonly MethodInfo moCanMethodInfo;
			private readonly bool mbCanMethodAcceptsParameter;
			#endregion
		}
		#endregion


	}

}
#region 
static class BindTo_GenericExtensions
{
	public static bool HasValue<T>(this T _this) where T : class
	{
		return _this != null;
	}
	public static T GetValueOrDefault<T>(this T _this, T defaultValue) where T : class
	{
		if (_this == null)
			return defaultValue;
		return _this as T;
	}
}
#endregion
