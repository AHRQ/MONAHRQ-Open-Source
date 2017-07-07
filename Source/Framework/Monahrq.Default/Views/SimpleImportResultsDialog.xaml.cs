using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Monahrq.Sdk.Events;

namespace Monahrq.Default.Views
{
    // replaced this window with Notepad to avoid lousy text layout
    public partial class SimpleImportResultsDialog : Window
    {
        public SimpleImportResultsDialog(ISimpleImportCompletedPayload importResults)
        {
            InitializeComponent();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(System.IO.File.ReadAllText(importResults.ErrorFile));
            var document = new FlowDocument(paragraph);
            FlowDocReader.Document = document;
        }
    }
}
