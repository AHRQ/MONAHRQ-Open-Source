using System;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using PropertyChanged;

namespace Monahrq.Reports.Model
{

    /// <summary>
    /// Model class for targeted audiences for the website.
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    [ImplementPropertyChanged]
    public class AudienceModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int Value { get; set; }

        /// <summary>
        /// Gets the <see cref="AudienceModel"/> enum.
        /// </summary>
        /// <value>
        /// The enum.
        /// </value>
        public Audience Enum { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudienceModel"/> class.
        /// </summary>
        /// <param name="audienceEnum">The  <see cref="Audience"/>  enum.</param>
        public AudienceModel(Audience audienceEnum)
        {
            Name = audienceEnum.GetDescription();
            Value = (int)audienceEnum;
            Enum = audienceEnum;
            //IsSelected = (audienceEnum == Audience.AllAudiences) ? true : false;
            IsSelected = (audienceEnum == Audience.Professionals) ? true : false;
            PropertyChanged += _audienceModelPropertyChanged;
        }

        /// <summary>
        /// Handles the audienceModelPropertyChanged event of the  control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void _audienceModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsSelected") return;

            if (IsSelected)
            {
                _onSelected(Enum);
            }
        }

      
        bool _IsSelected;

        /// <summary>
        /// Gets or sets a value indicating whether  <see cref="AudienceModel"/>  instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if  <see cref="AudienceModel"/>  instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }

            set { _IsSelected = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="AudienceModel"/> instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if <see cref="AudienceModel"/> instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }


        /// <summary>
        /// Occurs when [selected].
        /// </summary>
        public event SelectionHandler Selected;

        /// <summary>
        /// Triggers the event associated with <see cref="SelectionHandler"/>
        /// </summary>
        /// <param name="a"> <see cref="Audience"/></param>
        protected virtual void _onSelected(Audience a)
        {
            var handler = Selected;
            if (handler != null) handler(a, EventArgs.Empty);
        }


        /// <summary>
        /// Delegate to bind the event when audience is selected
        /// </summary>
        /// <param name="a">Enum <see cref="Audience"/></param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void SelectionHandler(Audience a, EventArgs e);
    }
}
