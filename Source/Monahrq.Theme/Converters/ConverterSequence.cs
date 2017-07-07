namespace Monahrq.Theme.Converters
{
	using Monahrq.Infrastructure.Assert;
	using System;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Markup;


	/// <summary>
	/// <example>
	/// <code lang="xml">
	/// <![CDATA[
	/// <Label>
	///     <Label.Visibility>
	///         <Binding Path="StartDateTime">
	///             <Binding.Converter>
	///							<ConverterGroupEx>
	///								<ConverterGroupStepEx>
	///									<ExpressionConverter {} {0} == {1} />
	///								</ConverterGroupStepEx>
	///								<ConverterGroupStepEx>
	///									<BooleanToVisibilityConverter UserHidden=false />
	///								</ConverterGroupStepEx>
	///							</ConverterGroupEx>
	///             </Binding.Converter>
	///         </Binding>
	///     </Label.Content>
	/// </Label>
	/// ]]>
	/// </code>
	/// </example>
	/// 
	///	The below is the same as above but skips the 'Step' breakdown.  The key here is that if a Converter is
	///	placed directly under the ConverterGroupEx, then it is treated as a single step.
	/// <example>
	/// <code lang="xml">
	/// <![CDATA[
	/// <Label>
	///     <Label.Visibility>
	///			<Binding Path="GetStringData">
	///				<Binding.Converter>
	///					<ConverterSequence>
	///						<ExpressionConverter>
	///							<ExpressionConverter.Expression>
	///								<![CDATA[	"{0}" != "Medicare Provider Charge Data" &&
	///											"{0}" != "Nursing Home Compare Data"	]].............>  <NOTE: the dots should be removed.>
	///							</ExpressionConverter.Expression>
	///						</ExpressionConverter>
	///						<BooleanToVisibilityConverter  UseHidden="False" />
	///					</ConverterSequence>
	///				</Binding.Converter> 
	///			</Binding>
	///     </Label.Visibility>
	/// </Label>
	/// ]]>
	/// </code>
	/// </example>
	/// </summary>
	
    [ContentProperty("Steps")]
    [ValueConversion(typeof(object), typeof(object))]
	public class ConverterSequence : IValueConverter, IMultiValueConverter
	{
		#region Constructor Methods.
		/// <summary>
        /// Initializes a new instance of the MultiConverterGroup class.
        /// </summary>
		public ConverterSequence()
        {
            this.steps = new Collection<Object>();
			this.DataContext = null;

        }
		public ConverterSequence(Object dataContext)
		{
			this.steps = new Collection<Object>();
			this.DataContext = dataContext;
		}
		#endregion

		#region IValueConverter Methods.
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (this.Steps.Count == 0)
				return DependencyProperty.UnsetValue;


			// TargetType is the source type of the next Converter except when on the last converter. 
			// The last Converter uses the targetType from XAML specified in the arguments parameters
		//	var nextTargetType =
		//		(converterIndex == Converters.Count - 1) ?
		//			targetType :
		//			cachedMetaData[Converters[converterIndex + 1]].ValueConversionAttr.SourceType;
			

				return RunConverter(this, value, targetType, parameter, culture);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
// 			if (this.Converters.Count == 0)
// 			{
// 				return DependencyProperty.UnsetValue;
// 			}
// 
// 			var convertedValue = value;
// 
// 			for (var i = this.Converters.Count - 1; i >= 0; --i)
// 			{
// 				convertedValue = this.Converters[i].ConvertBack(convertedValue, targetType, parameter, culture);
// 			}
// 
// 			return convertedValue;
		}

		private object RunConverter(Object objConverter,object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool isIValueConverter = objConverter is IValueConverter;
			bool isIMultiValueConverter = objConverter is IMultiValueConverter;
			bool isValueArray = value is object[];
			
			if (objConverter is ConverterSequence)
			{
				var nextValue = value;
				var converterGroup = objConverter as ConverterSequence;
				foreach (var subObjConverter in converterGroup.Steps)
				{
					nextValue = RunConverter(subObjConverter, nextValue, targetType, parameter, culture);
					if (nextValue == Binding.DoNothing) break;
				}
				value = nextValue;
			}
			// If objConverter is both a IValueConverter and a IMultiValueConverter, 
			// base choice on whether the input value is of array type.
			else if (isIValueConverter && !(isIMultiValueConverter && isValueArray))
			{
				var converter = objConverter as IValueConverter;
				value = converter.Convert(value, targetType, parameter, culture);
			}
			else if (isIMultiValueConverter)
			{
				//  The decision currently being made is that since we are here, and we are going to run this
				//  IMultiValueConverter; if the 'nextValue' isn't an Object[] (i.e. it came from a IValueConverter),
				//  We are going to convert it into a single entry Object[].  This makes IValueConverters chainable TO
				//  IMultivalueConverters.
				if (value != null && !(value is object[]))
				{
					value = new Object[] { value };
					//	throw new Exception("Invalid argument being passed to IMultiValueConverter");
				}

				var converter = (IMultiValueConverter)objConverter;
				value = converter.Convert(value as object[], targetType, parameter, culture);
			}
			else if (objConverter is ConverterAccumulator)
			{
				var converterAccumulator = objConverter as ConverterAccumulator;
				var nextValues = new object[converterAccumulator.Converters.Count];

				int index = 0;
				foreach (var subObjConverter in converterAccumulator.Converters)
				{
					var nextValue = RunConverter(subObjConverter, value, targetType, parameter, culture);
					if (nextValue == Binding.DoNothing) break;
					nextValues[index++] = nextValue;
				}
				value = nextValues;
			}
			else
			{
				value = objConverter;
			}

			return value;
		}
		#endregion

		#region IMultiValueConverter Methods.
		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (this.Steps.Count == 0)
				return DependencyProperty.UnsetValue;

			return RunConverter(this, values, targetType, parameter, culture);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetTypes"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion


		#region Properties.
		/// <summary>
		/// Gets the collection of <see cref="ConverterAccumulator"/>s in this <c>ConverterGroupEx</c>.
		/// We would like Object type limited to either ConverterGroupStepEx, IValueConverter or IMultiValueConverter.
		/// </summary>
		public Collection<Object> Steps
		{
			get { return this.steps; }
		}

		public object DataContext
		{
			get
			{
				return this.dataContext;
				//return (object)GetValue(DataContextProperty);
			}
			set
			{
				this.dataContext = value;
				//SetValue(DataContextProperty, value);
			}
		}
		#endregion

		#region DataContext DependecyProperty.
// 		public static readonly DependencyProperty DataContextProperty =
// 			DependencyProperty.Register(
// 				"DataContext",
// 				typeof(object),
// 				typeof(ConverterSequence),
// 				new PropertyMetadata(default(object), OnDataContextPropertyChanged));
// 		private static void OnDataContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
// 		{
// 			// AutocompleteTextBox source = d as AutocompleteTextBox;
// 			// Do something...
// 		}
		#endregion

		#region Variables.
		private readonly Collection<Object> steps;
		private object dataContext;
		#endregion

	}
}
