using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DatasetTitlePeriod.xaml
    /// </summary>
    public partial class DatasetTitlePeriod : UserControl
    {
        bool bLoaded;                       // avoid multiple calculations before the binding is effected
        string OriginalText;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTitlePeriod"/> class.
        /// </summary>
        public DatasetTitlePeriod()
        {
            InitializeComponent();
            bLoaded = false;

            // tap into change notification of the FrameworkElement.ActualWidthProperty
            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualWidthProperty, typeof(FrameworkElement));
            descriptor.AddValueChanged(this.MyPanel, new EventHandler(OnActualWidthChanged));
        }

        /// <summary>
        /// Handles the Loaded event of the StackPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            bLoaded = true;

            // use the binding expression to get the original text, not the text in the control itself, because after the first pass here we truncate the text!
            var bindingExpression = BindingOperations.GetBindingExpression(txtName, TextBlock.TextProperty);
            var source = bindingExpression.ResolvedSource as Dataset;
            if (source == null)
                return;
            OriginalText = source.File;
            OnResize();
        }

        /// <summary>
        /// Called when [actual width changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnActualWidthChanged(object sender, EventArgs e)
        {
            OnResize();
        }

        /// <summary>
        /// Called when [resize].
        /// </summary>
        void OnResize()
        {
            var myActualWidth = MyPanel.ActualWidth;

            if (bLoaded)
            {
                var textSize = MeasureTextSize(OriginalText, this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize);

                // compare the width of the window (user control) to the width of the text, depending on fontsize too
                if (textSize.Width > myActualWidth)
                {
                    // get width of ... that will be appended
                    string ellipsis = "...";
                    var ellipsisSize = MeasureTextSize(ellipsis, this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize);

                    // Get the width that has to be cut. Ellipsis width must be added.
                    var widthToTruncate = textSize.Width - myActualWidth + ellipsisSize.Width;

                    // Get the width of an "average" char.
                    // Uses little x as an "average" char, but if the text we're chopping is all little 'i',
                    // x is too big and it will chop more than necessary; or if original text is all cap W, this won't remove enough.
                    var charSize = MeasureTextSize("x", this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize);

                    // get the number of chars to truncate
                    int numCharsToTruncate = (int)(widthToTruncate / charSize.Width + 1);

                    // chop chars and append ellipsis until it fits
                    string text = OriginalText;
                    while (textSize.Width > myActualWidth && numCharsToTruncate < OriginalText.Length)
                    {
                        text = OriginalText.Substring(0, OriginalText.Length - numCharsToTruncate - 1) + ellipsis;
                        textSize = MeasureTextSize(text, this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize);

                        //Debug.WriteLine(string.Format("\n TextWidth: {0}   PanelActualwidth: {1}", textSize.Width, MyActualWidth));

                        // truncate more chars until it fits
                        ++numCharsToTruncate;
                    }

                    BindingOperations.ClearBinding(txtName, TextBlock.TextProperty);

                    // update the UI control with the new text
                    txtName.Text = text;
                }
            }
        }

        /// <summary>
        /// Get the required height and width of the specified text.
        /// </summary>
        public static Size MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            var ft = new FormattedText(text,
                                        CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                        fontSize,
                                        Brushes.Black);
            return new Size(ft.Width, ft.Height);
        }
    }
}
