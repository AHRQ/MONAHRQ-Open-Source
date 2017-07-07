﻿#if !SILVERLIGHT40

namespace Monahrq.Theme.MarkupExtensions
{
	using Monahrq.Infrastructure.Assert;
	using Monahrq.Theme.Converters;
	using System;
	using System.Windows.Markup;

	/// <summary>
	/// Implements a markup extension that allows instances of <see cref="ExpressionConverter"/> to be easily created.
	/// See <see cref="ExpressionConverter"/> for more information on supported expressions.
	/// </summary>
	/// <remarks>
	/// This markup extension allows instance of <see cref="ExpressionConverter"/> to be easily created inline in a XAML binding.
	/// See the example below.
	/// </remarks>
	/// <example>
	/// The following shows how to use the <c>ExpressionConverterExtension</c> inside a binding to calculate double a given value:
	/// <code lang="xml">
	/// <![CDATA[
	/// <Label Content="{Binding Value, Converter={ExpressionConverter {} {0} * 2}}"/>
	/// ]]>
	/// </code>
	/// </example>
	public sealed class ExpressionConverterExtension : MarkupExtension
	{
		#region Types.
		public enum EExpressionType
		{
			Simple,
			ReturnObject,
			ReturnDictionary,
			ReturnExpandoObject
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Initializes a new instance of the ExpressionConverterExtension class.
		/// </summary>
		public ExpressionConverterExtension()
		{
			this.expression = "";
			this.useDoubleSingleQuoteAsQuote = false;
		}

		/// <summary>
		/// Initializes a new instance of the ExpressionConverterExtension class with the given expression.
		/// </summary>
		/// <param name="expression">
		/// The expression.
		/// </param>
		public ExpressionConverterExtension(string expression)
		{
			this.expression = expression;
			this.useDoubleSingleQuoteAsQuote = false;
		}
		#endregion


		#region ProvideValue Methods.
		/// <summary>
		/// Provides an instance of <see cref="ExpressionConverter"/> based on <see cref="Expression"/>.
		/// </summary>
		/// <param name="serviceProvider">
		/// An object that can provide services.
		/// </param>
		/// <returns>
		/// The instance of <see cref="ExpressionConverter"/>.
		/// </returns>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			exceptionHelper.ResolveAndThrowIf(this.expression == null, "NoExpression");
			return new ExpressionConverter(
				this.expression,
				this.expressionType,
				this.returnName,
				this.useDoubleSingleQuoteAsQuote);
		}
		#endregion


		#region Properties.
		/// <summary>
		/// Gets or sets the expression to use in the <see cref="ExpressionConverter"/>.
		/// </summary>
		[ConstructorArgument("expression")]

		public string Expression
		{
			get { return this.expression; }
			set { this.expression = value; }
		}

		public bool UseDoubleSingleQuoteAsQuote
		{
			get { return this.useDoubleSingleQuoteAsQuote; }
			set { this.useDoubleSingleQuoteAsQuote = value; }
		}
		public EExpressionType ExpressionType
		{
			get { return expressionType; }
			set { expressionType = value; }
		}
		public string ReturnName
		{
			get { return returnName; }
			set { returnName = value; }
		}
		#endregion


		#region Variables.
		private static readonly ExceptionHelper exceptionHelper = new ExceptionHelper(typeof(ExpressionConverterExtension));
		private string expression;
		private bool useDoubleSingleQuoteAsQuote;
		private EExpressionType expressionType;
		private string returnName;
		#endregion
	}
}

#endif