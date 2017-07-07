namespace Monahrq.Theme.Converters
{
	using Monahrq.Infrastructure.Assert;
	using Monahrq.Infrastructure.Assert.Extensions;
	using System;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Markup;

	/// <summary>
	/// An implementation of <see cref="IValueConverter"/> and <see cref="IMultiValueConverter"/> that returns the bound data specified
	/// by <see cref="Indexes"/>.
	/// </summary>
	/// <remarks>
	/// The following example shows how a <c>IndexerConverter</c> can be used to get the data at an index of multiple values:
	/// <code lang="xml">
	/// <![CDATA[
	/// <Label>
	///     <Label.Content>
	///         <MultiBinding Converter="{Indexer 1}">
	///             <Binding Path="Name"/>
	///             <Binding Path="Dob"/>
	///         </MultiBinding>
	///     </Label.Content>
	/// </Label>
	/// ]]>
	/// </code>
	/// </example>
	[ContentProperty("Index")]
	[ValueConversion(typeof(object), typeof(string))]
	public class Indexer : IValueConverter, IMultiValueConverter
	{

		#region Constructors.
		public Indexer()
		{
			this.index = null;
		}
		public Indexer(int index)
		{
			this.index = index;
		}
		#endregion

		#region IValueConverter Methods.
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			exceptionHelper.ResolveAndThrowIf(this.Index == null, "NoIndex");
			exceptionHelper.ResolveAndThrowIf(this.Index != 0, "IndexIsOutOfRange");
			return value;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return 0;
		}
		#endregion

		#region IMultiValueConverter Methods.
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			exceptionHelper.ResolveAndThrowIf(this.Index == null, "NoIndex");
			exceptionHelper.ResolveAndThrowIf(this.Index >= values.Length || this.Index < 0, "IndexIsOutOfRange");
			return values[this.Index.Value];
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
		#endregion


		#region Properties.
		/// <summary>
		/// Gets or sets the format string to use when converting bound data.
		/// </summary>
		[ConstructorArgument("index")]
		public int? Index
		{
			get { return this.index; }
			set { this.index = value; }
		}
		#endregion

		#region Variables.
		private static readonly ExceptionHelper exceptionHelper = new ExceptionHelper(typeof(FormatConverter));
		private int? index;
		#endregion
	}
}