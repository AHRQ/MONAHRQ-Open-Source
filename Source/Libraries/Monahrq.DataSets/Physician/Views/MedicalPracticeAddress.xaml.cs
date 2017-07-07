using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Monahrq.DataSets.Physician.Views
{
    /// <summary>
    /// Interaction logic for MedicalPracticeAddress.xaml
    /// </summary>
    public partial class MedicalPracticeAddress
    {
        #region Dependency Property

        /// <summary>
        /// The edit command property
        /// </summary>
        public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(MedicalPracticeAddress), new FrameworkPropertyMetadata(null, OnEditCommandChanged));

        /// <summary>
        /// The remove command property
        /// </summary>
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register("RemoveCommand", typeof(ICommand), typeof(MedicalPracticeAddress), new FrameworkPropertyMetadata(null, OnRemoveCommandChanged));

        /// <summary>
        /// The preview command property
        /// </summary>
        public static readonly DependencyProperty PreviewCommandProperty = DependencyProperty.Register("PreviewCommand", typeof(ICommand), typeof(MedicalPracticeAddress), new FrameworkPropertyMetadata(null, OnPreviewCommandChanged));

        /// <summary>
        /// Gets or sets the edit command.
        /// </summary>
        /// <value>
        /// The edit command.
        /// </value>
        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the remove command.
        /// </summary>
        /// <value>
        /// The remove command.
        /// </value>
        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the preview command.
        /// </summary>
        /// <value>
        /// The preview command.
        /// </value>
        public ICommand PreviewCommand
        {
            get { return (ICommand)GetValue(PreviewCommandProperty); }
            set { SetValue(PreviewCommandProperty, value); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is medical practice name visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is medical practice name visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsMedicalPracticeNameVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is address selectable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is address selectable; otherwise, <c>false</c>.
        /// </value>
        public bool IsAddressSelectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edit button visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is edit button visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is delete button visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is delete button visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleteButtonVisible { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [preview command changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnPreviewCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MedicalPracticeAddress;

            if (control == null) return;

            var btn = control.FindName("PreviewColumn") as Button;
            if (btn != null) btn.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called when [remove command changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnRemoveCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MedicalPracticeAddress;

            if (control == null) return;

            var btn = control.FindName("RemoveColumn") as Button;
            if (btn != null) btn.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called when [edit command changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnEditCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MedicalPracticeAddress;

            if (control == null) return;

            var btn = control.FindName("EditColumn") as Button;
            if (btn != null) btn.Visibility = Visibility.Visible;
        }


        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicalPracticeAddress"/> class.
        /// </summary>
        public MedicalPracticeAddress()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = this;
            }

            Loaded += MedicalPracticeAddress_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the MedicalPracticeAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void MedicalPracticeAddress_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
