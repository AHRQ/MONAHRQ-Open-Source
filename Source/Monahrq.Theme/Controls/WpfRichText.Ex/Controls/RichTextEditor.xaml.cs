using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// This code comes from http://www.codeproject.com/Articles/37169/WPF-RichText-Editor
// Another version (maybe better) is here: http://www.codeproject.com/Articles/66054/A-Bindable-WPF-RichTextBox
// Another version (maybe better) is here: http://www.shawnduggan.com/?p=54
// Could also try this, but maybe it doesn't have DP for binding: http://wpftoolkit.codeplex.com/

namespace WpfRichText.Ex.Controls
{
    /// <summary>
    /// Interaction logic for BindableRichTextbox.xaml
    /// </summary>
    public partial class RichTextEditor : UserControl
    {
        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(RichTextEditor),
          new PropertyMetadata(string.Empty));
        
        public RichTextEditor()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { 
                SetValue(TextProperty, value);
            }
        }
    }
}
