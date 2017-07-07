using System;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using PropertyChanged;

namespace Monahrq.Websites.Model
{
    [ImplementPropertyChanged]
    public class AudienceModel:BaseViewModel 
    {
        public string Name { get; set; }
        public int Value { get; set; }
       
        public Audience Enum { get; private set; }

        public AudienceModel(Audience audienceEnum)
        {
            Name = audienceEnum.GetDescription();
            Value = (int) audienceEnum;
            Enum = audienceEnum;
            IsSelected = false;
            PropertyChanged += _audienceModelPropertyChanged;
        }
    
        private void _audienceModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsSelected") return;

            if (IsSelected)
            {
                _onSelected(Enum);
            }
        }

        bool _IsSelected;
        public bool IsSelected {
            get
            {
                return _IsSelected;
            }

            set { _IsSelected = value; }
        }

        public bool IsEnabled { get; set; }

        public event SelectionHandler Selected;

        protected virtual void _onSelected(Audience a)
        {
            var handler = Selected;
            if (handler != null) handler(a, EventArgs.Empty);
        }

        public delegate void SelectionHandler(Audience a, EventArgs e);
    }
}
