
namespace Monahrq.Theme.MarkupExtensions
{
	using Monahrq.Infrastructure.Assert.Extensions;
	using Monahrq.Theme.Converters;
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// Implements a markup extension that allows instances of <see cref="DateTimeConverter"/> to be easily created.
    /// </summary>
    /// <remarks>
    /// This markup extension allows instance of <see cref="DateTimeConverter"/> to be easily created inline in a XAML binding.
    /// See the example below.
    /// </remarks>
    /// <example>
    /// The following shows how to use the <c>DateTimeConverterExtension</c> inside a binding to convert values to local time:
    /// <code lang="xml">
    /// <![CDATA[
    /// <Label Content="{Binding StartTime, Converter={DateTimeConverter TargetKind=Local}}"/>
    /// ]]>
    /// </code>
    /// </example>
    public sealed class PercentOfConverterExtension : MarkupExtension
    {

		#region Constructor Methods.
		public PercentOfConverterExtension()
		{
			this.percent = null;
		}
		public PercentOfConverterExtension(Double? percent)
		{
			this.percent = percent;
		}
		#endregion

		#region Methods.
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var percentageOfConverter = new PercentOfConverter(this.percent);
			return percentageOfConverter;
		}
		#endregion
		
		#region Properties.
		[ConstructorArgument("percent")]
		public Double? Percent
		{
			get { return percent; }
			set { percent = value; }
		}
		#endregion
		
		#region Variables.
        private Double? percent;
		#endregion
    }
}