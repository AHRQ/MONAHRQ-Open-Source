using System;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Websites.ViewModels
{
	[ImplementPropertyChanged]
	public class BatchMeasureModel : BaseViewModel, ISelectable
	{

		#region Constructors.
		public BatchMeasureModel()
		{
			//ScaleBy = null;
			NumeratorOverride = null;
			DenominatorOverride = null;
			PerformMarginSuppressionOverride = null;
			IsMinScaleByRadioButtonChecked = null;
			IsMediumScaleByRadioButtonChecked = null;
			IsMaxScaleByRadioButtonChecked = null;
			IsSelected = true;

			ProxyMeasureModel = ProxyMeasureModel ?? new MeasureModel();
			ProxyWebsiteMeasure = ProxyWebsiteMeasure ?? new WebsiteMeasure();
		}
		public BatchMeasureModel(MeasureModel m)
			: this()
		{
			ProxyMeasureModel = m;
		}
		public BatchMeasureModel(WebsiteMeasure wm)
			: this()
		{
			ProxyWebsiteMeasure = wm;
		}

		#endregion




		#region Properties.
		public string NumeratorOverride { get; set; }
		public string DenominatorOverride { get; set; }
		public bool? PerformMarginSuppressionOverride { get; set; }
		public bool? IsMinScaleByRadioButtonChecked { get; set; }
		public bool? IsMediumScaleByRadioButtonChecked { get; set; }
		public bool? IsMaxScaleByRadioButtonChecked { get; set; }
		public decimal? ScaleBy { get { return ProxyMeasureModel.ScaleBy.ConvertTo<decimal?>(defaultValues: null); } }
		private bool _isBatchSelected;
		public bool IsSelected
		{
			get { return _isBatchSelected; }
			set
			{
				_isBatchSelected = value;
				RaiseValueChanged();
			}
		}
		public MeasureModel ProxyMeasureModel { get; set; }
		public WebsiteMeasure ProxyWebsiteMeasure { get; set; }
		#endregion


		#region Events.
		public event EventHandler SelectedChanged;
		private void RaiseValueChanged()
		{
			if (SelectedChanged != null)
			{
				SelectedChanged(this, new EventArgs());
			}
		}
		#endregion

		#region Variables.
		///// <summary>
		///// Used to handle/include any complex logic within MeasureModel.
		///// Didn't want to duplicate logic if not needed.
		///// </summary>
		//private MeasureModel proxyMeasureModel;
		//private bool? isMinScaleByRadioButtonChecked;
		//private bool? isMediumScaleByRadioButtonChecked;
		//private bool? isMaxScaleByRadioButtonChecked;
		#endregion
	}
}
