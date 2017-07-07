
namespace Monahrq.Theme.MarkupExtensions
{
	using Monahrq.Infrastructure.Assert.Extensions;
	using Monahrq.Theme.Converters;
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Markup;

	/// <summary>
	/// 
	/// </summary>
	[ContentProperty("Steps")]
	[MarkupExtensionReturnType(typeof(Binding))]
	public sealed class SequenceExtension : MarkupExtension
    {

        /// <summary>
        /// Initializes a new instance of the SequenceExtension class.
        /// </summary>
        public SequenceExtension():base()
        {
			this.steps = new Collection<Object>();
			this.path = null;
        }
		public SequenceExtension(Collection<Object> steps)
        {
			this.steps = steps;
			this.path = null;
		}
		public SequenceExtension(string path):this()
		{
			this.steps = new Collection<Object>();
			this.path = path;
		}

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
			bindTo = new BindToExtension(this.path);
			var converterSequence = new ConverterSequence();

			if (this.Steps != null)
				foreach (var step in this.Steps) converterSequence.Steps.Add(step);

			bindTo.Converter = converterSequence;

			return bindTo.ProvideValue(serviceProvider); ;

        }


		#region Properties.
		[ConstructorArgument("steps")]
		public Collection<Object> Steps
		{
			get { return this.steps; }
			set { this.steps = value; }
		}
		[ConstructorArgument("path")]
		public string Path
		{
			get { return this.path; }
			set { this.path = value; }
		}
		#endregion

		#region Variables.
		private string path;
		private BindToExtension bindTo;
		private Collection<Object> steps;
		#endregion
    }
}
