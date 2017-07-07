using System;
using System.Globalization;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using PropertyChanged;

namespace Monahrq.Websites.ViewModels
{
    [ImplementPropertyChanged]
    public class HospitalViewModel : BaseNotify, ISelectable
    {
        private bool _isSelected;

        public HospitalViewModel()
        {
            
        }

        public HospitalViewModel(Hospital hospital)
        {
            Hospital = hospital;
            CCR = hospital.CCR.HasValue ? hospital.CCR.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
            Hospital.PropertyChanged += _reportPropertyChanged;

        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnValueChnaged();
            }
        }

        public Hospital Hospital { get; set; }
        public string CCR  { get; set; }

        public event EventHandler IsValueChanged;

        private void OnValueChnaged()
        {
            if (IsValueChanged != null) IsValueChanged(this, new EventArgs());
        }

        private void _reportPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged();
        }
    }
}
