using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Data;
using WpfRichText.Ex.XamlToHtmlParser;

namespace WpfRichText.Ex.AttachedProperties
{
	/// <summary>
	/// Helper functions for RichTextbox.
	/// </summary>
	public static class RichTextboxAssistant
    {
		/// <summary>
		/// The bound document
		/// </summary>
		public static readonly DependencyProperty BoundDocument =
            DependencyProperty.RegisterAttached(
                "BoundDocument",
                typeof(string),
                typeof(RichTextboxAssistant),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnBoundDocumentChanged
            ));

		/// <summary>
		/// Called when [bound document changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnBoundDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as RichTextBox;
            if (box == null)
                return;

            // so we can update the View from the ViewModel data without that firing a circular event to update the ViewModel from that change to the View
            RemoveEventHandler(box);

            // This code has one of these problems...
            // Either Save button isn't enabled; or 
            // text insert position is always the beginning of the document; or 
            // the text control appears blank after loading.

            //string newXAML = GetBoundDocument(d);
            //box.Document.Blocks.Clear();

            //if (!string.IsNullOrEmpty(newXAML))
            //{
            //    using (var xamlMemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(newXAML)))
            //    {
            //        var parser = new ParserContext();
            //        parser.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            //        parser.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            //        var section = XamlReader.Load(xamlMemoryStream, parser) as Section;
            //        box.Document.Blocks.Add(section);
            //    }
            //}

            AttachEventHandler(box);
        }

		/// <summary>
		/// Removes the event handler.
		/// </summary>
		/// <param name="box">The box.</param>
		private static void RemoveEventHandler(RichTextBox box)
        {
            var binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null)
            {
                // TextChanged vs. LostFocus is selected based on the binding UpdateSourceTrigger in RichTextEditor.xaml
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    box.LostFocus -= HandleLostFocus;
                }
                else
                {
                    box.TextChanged -= HandleTextChanged;
                }
            }
        }

		/// <summary>
		/// Attaches the event handler.
		/// </summary>
		/// <param name="box">The box.</param>
		private static void AttachEventHandler(RichTextBox box)
        {
            var binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null)
            {
                // TextChanged vs. LostFocus is selected based on the binding UpdateSourceTrigger in RichTextEditor.xaml
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    box.LostFocus += HandleLostFocus;
                }
                else
                {
                    box.TextChanged += HandleTextChanged;
                }
            }
        }

		/// <summary>
		/// Handles the lost focus.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private static void HandleLostFocus(object sender, RoutedEventArgs e)
        {
            DoTextChangedOrLostFocus(sender);
        }

		/// <summary>
		/// Handles the text changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private static void HandleTextChanged(object sender, RoutedEventArgs e)
        {
            DoTextChangedOrLostFocus(sender);
        }

		/// <summary>
		/// Does the text changed or lost focus.
		/// </summary>
		/// <param name="sender">The sender.</param>
		private static void DoTextChangedOrLostFocus(object sender)
        {
            var box = sender as RichTextBox;
            var tr = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);

            using (var ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                string xamlText = ASCIIEncoding.Default.GetString(ms.ToArray());
                SetBoundDocument(box, xamlText);
            }
        }

		/// <summary>
		/// Gets the bound document.
		/// </summary>
		/// <param name="dp">The dp.</param>
		/// <returns></returns>
		public static string GetBoundDocument(DependencyObject dp)
        {
            var html = dp.GetValue(BoundDocument) as string;
            var xaml = string.Empty;

            //if (!string.IsNullOrEmpty(html))
            if (html != null)
                xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, false);

            return xaml;
        }

		/// <summary>
		/// Sets the bound document.
		/// </summary>
		/// <param name="dp">The dp.</param>
		/// <param name="value">The value.</param>
		public static void SetBoundDocument(DependencyObject dp, string value)
        {
            var xaml = value;
            var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, false);
            dp.SetValue(BoundDocument, html);
        }
    }
}