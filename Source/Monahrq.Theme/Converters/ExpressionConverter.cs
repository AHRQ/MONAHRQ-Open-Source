namespace Monahrq.Theme.Converters
{
	using ExpressionEvaluator;
	using Monahrq.Infrastructure.Assert;
	using System;
	using System.Globalization;
	using System.IO;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Markup;
	using Monahrq.Theme.MarkupExtensions;
	using System.Collections.Generic;
	using System.Dynamic;
	using System.Text.RegularExpressions;



	/// <summary>
	/// Converts C# code in a string to an compiled/executed result.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	/// <seealso cref="System.Windows.Data.IMultiValueConverter" />
	[ContentProperty("Expression")]
	[ValueConversion(typeof(object), typeof(object))]
	public sealed class ExpressionConverter : IValueConverter, IMultiValueConverter
	{
		#region Constructors.
		/// <summary>
		/// Initializes a new instance of the ExpressionConverter class.
		/// </summary>
		public ExpressionConverter():this("",ExpressionConverterExtension.EExpressionType.Simple,"",false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ExpressionConverter class with the specified expression.
		/// </summary>
		/// <param name="expression">The expression (see <see cref="Expression" />).</param>
		public ExpressionConverter(string expression):this(expression,ExpressionConverterExtension.EExpressionType.Simple,"",false)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionConverter"/> class.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="expressionType">Type of the expression.</param>
		/// <param name="returnName">Name of the return.</param>
		/// <param name="useDoubleSingleQuoteAsQuote">if set to <c>true</c> [use double single quote as quote].</param>
		public ExpressionConverter(
			string expression, 
			ExpressionConverterExtension.EExpressionType expressionType,
			string returnName,
			bool useDoubleSingleQuoteAsQuote)
		{
			this.Expression = expression;
			this.expressionType = expressionType;
			this.returnName = returnName;
			this.useDoubleSingleQuoteAsQuote = useDoubleSingleQuoteAsQuote;
		}
		#endregion

		#region Convert Methods.
		/// <summary>
		/// Attempts to convert the specified value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return InternalConvert(value, targetType, parameter, culture);
		}

		/// <summary>
		/// Attempts to convert the specified value back.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DependencyProperty.UnsetValue;
		}

		/// <summary>
		/// Attempts to convert the specified values.
		/// </summary>
		/// <param name="values">The values to convert.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value.
		/// </returns>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			return InternalConvert(values, targetType, parameter, culture);
		}

		/// <summary>
		/// Attempts to convert back the specified values.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetTypes">The types of the binding target properties.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// Converted values.
		/// </returns>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}


		/// <summary>
		/// Internals the convert.
		/// </summary>
		/// <typeparam name="O"></typeparam>
		/// <param name="value">The value.</param>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		private object InternalConvert<O>(O value, Type targetType, object parameter, CultureInfo culture)
		{
			//  Format expression.
			var tempExpression = expression;
			if (useDoubleSingleQuoteAsQuote) tempExpression = tempExpression.Replace("''", "\"");
			var compilableExpression =
				value is object[] ?
					String.Format(tempExpression, value as object[]) :
					String.Format(tempExpression, value);


			// Evaluate expression.
			switch (expressionType)
			{
				case ExpressionConverterExtension.EExpressionType.Simple:
					{
						var compiledExpressionA = new CompiledExpression(compilableExpression);
						exceptionHelper.ResolveAndThrowIf(compiledExpressionA == null, "NoExpression");
						return compiledExpressionA.Eval();
					}
				case ExpressionConverterExtension.EExpressionType.ReturnObject:
					{
						string roName = "__result";
						string rvName = "__value";
						//compilableExpression = compilableExpression.Replace(returnName, String.Format("{0}.{1}",roName,rvName));
						compilableExpression =
							Regex.Replace(
								compilableExpression, 
								String.Format(@"\b{0}\b",returnName), 
								String.Format("{0}.{1}", roName, rvName));

						var returnVarB = new ExpandoObject();
						var returnVarDicB = (IDictionary<string, Object>) returnVarB;
						returnVarDicB[rvName] = new Object();


						var typesB = new TypeRegistry();
						typesB.RegisterSymbol(roName, returnVarB);

						var compiledExpressionB = new CompiledExpression
						{
							StringToParse = compilableExpression,
							TypeRegistry = typesB,
							ExpressionType = CompiledExpressionType.StatementList
						};

						exceptionHelper.ResolveAndThrowIf(compiledExpressionB == null, "NoExpression");
						compiledExpressionB.Eval();

						return returnVarDicB[rvName];
					}
				case ExpressionConverterExtension.EExpressionType.ReturnExpandoObject:
					{
						//compilableExpression =
						//	Regex.Replace(
						//		compilableExpression,
						//		String.Format(@"\b{0}\b", returnName),
						//		String.Format("{0}.{1}", roName, rvName));

						var returnVarD = new ExpandoObject();
						var returnVarDicD = (IDictionary<string, Object>)returnVarD;


						var typesD = new TypeRegistry();
						typesD.RegisterSymbol(returnName, returnVarD);

						var compiledExpressionB = new CompiledExpression
						{
							StringToParse = compilableExpression,
							TypeRegistry = typesD,
							ExpressionType = CompiledExpressionType.StatementList
						};

						exceptionHelper.ResolveAndThrowIf(compiledExpressionB == null, "NoExpression");
						compiledExpressionB.Eval();

						return returnVarD;
					}
				case ExpressionConverterExtension.EExpressionType.ReturnDictionary:
					{	
						// Pretty sure this doesn't work yet.
						// Ultimately I'd like to parse the code and find any variable that is not formally defined within
						// to have its value assigned to a variable defined for it in the dictionary/ExpandoObject.
						throw new NotImplementedException();
						
						Dictionary<string, Object> returnVarC = new Dictionary<string, Object>();
						var typesC = new TypeRegistry();
						typesC.RegisterSymbol(returnName, returnVarC);

						var compiledExpressionC = new CompiledExpression
						{
							StringToParse = compilableExpression,
							TypeRegistry = typesC
						};

						exceptionHelper.ResolveAndThrowIf(compiledExpressionC == null, "NoExpression");
						compiledExpressionC.Eval();

						return returnVarC;
					}
				default:
					return DependencyProperty.UnsetValue;
			}
		}
		#endregion

		#region Properties.
		/// <summary>
		/// Gets or sets the expression for this <c>MathConverter</c>.
		/// </summary>
		/// <value>
		/// The expression.
		/// </value>
		[ConstructorArgument("expression")]
		public string Expression
		{
			get
			{
				return this.expression;
			}

			set
			{
				this.expression = value;
				//this.compiledExpression = null;
			}
		}
		#endregion

		#region Variables.
		/// <summary>
		/// The exception helper
		/// </summary>
		private static readonly ExceptionHelper exceptionHelper = new ExceptionHelper(typeof(ExpressionConverter));
		/// <summary>
		/// The expression
		/// </summary>
		private string expression;
		/// <summary>
		/// The use double single quote as quote
		/// </summary>
		private bool useDoubleSingleQuoteAsQuote;
		/// <summary>
		/// The expression type
		/// </summary>
		private ExpressionConverterExtension.EExpressionType expressionType;
		/// <summary>
		/// The return name
		/// </summary>
		private string returnName;
	//	private CompiledExpression compiledExpression;
		#endregion
	}
}