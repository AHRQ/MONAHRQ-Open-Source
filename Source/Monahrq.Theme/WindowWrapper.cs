using System;
using System.Windows;

namespace Monahrq.Theme
{
    /// <summary>
    /// Defines a wrapper for the <see cref="Window"/> class that implements the <see cref="IWindow"/> interface.
    /// </summary>
    public class WindowWrapper : IWindow
    {
        private readonly Window _window;

        /// <summary>
        /// Initializes a new instance of <see cref="WindowWrapper"/>.
        /// </summary>
        public WindowWrapper()
        {
            _window = new Window();
            _window.WindowStyle = WindowStyle.None;
            _window.Height = 436;
            _window.Width = 676;
            _window.ResizeMode=ResizeMode.NoResize;
            _window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        /// <summary>
        /// Ocurrs when the <see cref="Window"/> is closed.
        /// </summary>
        public event EventHandler Closed
        {
            add { _window.Closed += value; }
            remove { _window.Closed -= value; }
        }

        /// <summary>
        /// Gets or Sets the content for the <see cref="Window"/>.
        /// </summary>
        public object Content
        {
            get { return _window.Content; }
            set { _window.Content = value; }
        }

        /// <summary>
        /// Gets or Sets the <see cref="Window.Owner"/> control of the <see cref="Window"/>.
        /// </summary>
        public object Owner
        {
            get { return _window.Owner; }
            set { _window.Owner = value as Window; }
        }

        /// <summary>
        /// Gets or Sets the <see cref="FrameworkElement.Style"/> to apply to the <see cref="Window"/>.
        /// </summary>
        public Style Style
        {
            get { return _window.Style; }
            set { _window.Style = value; }
        }

        /// <summary>
        /// Opens the <see cref="Window"/>.
        /// </summary>
        public void Show()
        {
            try
            {
                if (_window != null && _window.Owner != null)
                    _window.Show();
            }
            catch (Exception)
            {}
        }

        /// <summary>
        /// Closes the <see cref="Window"/>.
        /// </summary>
        public void Close()
        {
            try
            {
                if (_window != null)
                    _window.Close();
            }
            catch (Exception)
            { }
        }
    }
}