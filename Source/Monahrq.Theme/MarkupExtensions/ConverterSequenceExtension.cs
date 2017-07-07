
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
    public sealed class ConverterSequenceExtension : MarkupExtension
    {
        /// <summary>
        /// Initializes a new instance of the ExpressionConverterExtension class.
        /// </summary>
        public ConverterSequenceExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ExpressionConverterExtension class with the given expression.
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        public ConverterSequenceExtension(object dataContext)
        {
            this.dataContext = dataContext;
        }

        /// <summary>
        /// Gets or sets the expression to use in the <see cref="ExpressionConverter"/>.
        /// </summary>
        [ConstructorArgument("dataContext")]
        public object DataContext
        {
            get { return this.dataContext; }
            set { this.dataContext = value; }
        }

        /// <summary>
		/// Provides an instance of <see cref="ConverterSequenceExtension"/> based on <see cref="Expression"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// An object that can provide services.
        /// </param>
        /// <returns>
        /// The instance of <see cref="ExpressionConverter"/>.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
			exceptionHelper.ResolveAndThrowIf(this.dataContext == null, "NoDataContext");
			return new ConverterSequence(this.dataContext);
		}

		#region Variables.
		private static readonly ExceptionHelper exceptionHelper = new ExceptionHelper(typeof(ConverterSequenceExtension));
		private object dataContext;
		#endregion
    }

}
