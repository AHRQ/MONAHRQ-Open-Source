using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// 
	/// </summary>
	public enum MButtonDirections { None, Right, Left }
	/// <summary>
	/// 
	/// </summary>
	public enum MButtonSymbol { Arrow, Plus, Import }
	/// <summary>
	/// The mapped button 
	/// </summary>
	/// <seealso cref="System.Windows.Controls.Button" />
	public class MonahrqButton : Button
    {

		/// <summary>
		/// Initializes the <see cref="MonahrqButton"/> class.
		/// </summary>
		static MonahrqButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MonahrqButton), new FrameworkPropertyMetadata(typeof(MonahrqButton)));
        }

		/// <summary>
		/// Gets or sets the mouseover background.
		/// </summary>
		/// <value>
		/// The mouseover background.
		/// </value>
		public Brush MouseoverBackground
        {
            get { return (Brush)GetValue(MouseoverBackgroundProperty); }
            set { SetValue(MouseoverBackgroundProperty, value); }
        }

		/// <summary>
		/// The mouseover background property
		/// </summary>
		public static readonly DependencyProperty MouseoverBackgroundProperty
            = DependencyProperty.Register("MouseoverBackground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the mouseover foreground.
		/// </summary>
		/// <value>
		/// The mouseover foreground.
		/// </value>
		public Brush MouseoverForeground
        {
            get { return (Brush)GetValue(MouseoverForegroundProperty); }
            set { SetValue(MouseoverForegroundProperty, value); }
        }

		/// <summary>
		/// The mouseover foreground property
		/// </summary>
		public static readonly DependencyProperty MouseoverForegroundProperty
            = DependencyProperty.Register("MouseoverForeground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the mouseover border.
		/// </summary>
		/// <value>
		/// The mouseover border.
		/// </value>
		public Brush MouseoverBorder
        {
            get { return (Brush)GetValue(MouseoverBorderProperty); }
            set { SetValue(MouseoverBorderProperty, value); }
        }

		/// <summary>
		/// The mouseover border property
		/// </summary>
		public static readonly DependencyProperty MouseoverBorderProperty
            = DependencyProperty.Register("MouseoverBorder", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the disabled background.
		/// </summary>
		/// <value>
		/// The disabled background.
		/// </value>
		public Brush DisabledBackground
        {
            get { return (Brush)GetValue(DisabledBackgroundProperty); }
            set { SetValue(DisabledBackgroundProperty, value); }
        }

		/// <summary>
		/// The disabled background property
		/// </summary>
		public static readonly DependencyProperty DisabledBackgroundProperty
            = DependencyProperty.Register("DisabledBackground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the disabled foreground.
		/// </summary>
		/// <value>
		/// The disabled foreground.
		/// </value>
		public Brush DisabledForeground
        {
            get { return (Brush)GetValue(DisabledForegroundProperty); }
            set { SetValue(DisabledForegroundProperty, value); }
        }

		/// <summary>
		/// The disabled foreground property
		/// </summary>
		public static readonly DependencyProperty DisabledForegroundProperty
            = DependencyProperty.Register("DisabledForeground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the disabled border.
		/// </summary>
		/// <value>
		/// The disabled border.
		/// </value>
		public Brush DisabledBorder
        {
            get { return (Brush)GetValue(DisabledBorderProperty); }
            set { SetValue(DisabledBorderProperty, value); }
        }

		/// <summary>
		/// The disabled border property
		/// </summary>
		public static readonly DependencyProperty DisabledBorderProperty
            = DependencyProperty.Register("DisabledBorder", typeof(Brush), typeof(MonahrqButton));


		/// <summary>
		/// Gets or sets the pressed background.
		/// </summary>
		/// <value>
		/// The pressed background.
		/// </value>
		public Brush PressedBackground
        {
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }

		/// <summary>
		/// The pressed background property
		/// </summary>
		public static readonly DependencyProperty PressedBackgroundProperty
            = DependencyProperty.Register("PressedBackground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the pressed foreground.
		/// </summary>
		/// <value>
		/// The pressed foreground.
		/// </value>
		public Brush PressedForeground
        {
            get { return (Brush)GetValue(PressedForegroundProperty); }
            set { SetValue(PressedForegroundProperty, value); }
        }

		/// <summary>
		/// The pressed foreground property
		/// </summary>
		public static readonly DependencyProperty PressedForegroundProperty
            = DependencyProperty.Register("PressedForeground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the pressed border.
		/// </summary>
		/// <value>
		/// The pressed border.
		/// </value>
		public Brush PressedBorder
        {
            get { return (Brush)GetValue(PressedBorderProperty); }
            set { SetValue(PressedBorderProperty, value); }
        }

		/// <summary>
		/// The pressed border property
		/// </summary>
		public static readonly DependencyProperty PressedBorderProperty
            = DependencyProperty.Register("PressedBorder", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the focused background.
		/// </summary>
		/// <value>
		/// The focused background.
		/// </value>
		public Brush FocusedBackground
        {
            get { return (Brush)GetValue(FocusedBackgroundProperty); }
            set { SetValue(FocusedBackgroundProperty, value); }
        }

		/// <summary>
		/// The focused background property
		/// </summary>
		public static readonly DependencyProperty FocusedBackgroundProperty
            = DependencyProperty.Register("FocusedBackground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the focused foreground.
		/// </summary>
		/// <value>
		/// The focused foreground.
		/// </value>
		public Brush FocusedForeground
        {
            get { return (Brush)GetValue(FocusedForegroundProperty); }
            set { SetValue(FocusedForegroundProperty, value); }
        }

		/// <summary>
		/// The focused foreground property
		/// </summary>
		public static readonly DependencyProperty FocusedForegroundProperty
            = DependencyProperty.Register("FocusedForeground", typeof(Brush), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the focused border.
		/// </summary>
		/// <value>
		/// The focused border.
		/// </value>
		public Brush FocusedBorder
        {
            get { return (Brush)GetValue(FocusedBorderProperty); }
            set { SetValue(FocusedBorderProperty, value); }
        }

		/// <summary>
		/// The focused border property
		/// </summary>
		public static readonly DependencyProperty FocusedBorderProperty
            = DependencyProperty.Register("FocusedBorder", typeof(Brush), typeof(MonahrqButton));


		/// <summary>
		/// Gets or sets the symbol direction.
		/// </summary>
		/// <value>
		/// The symbol direction.
		/// </value>
		public MButtonDirections SymbolDirection
        {
            get { return (MButtonDirections)GetValue(SymbolDirectionProperty); }
            set { SetValue(SymbolDirectionProperty, value); }
        }

		/// <summary>
		/// The symbol direction property
		/// </summary>
		public static readonly DependencyProperty SymbolDirectionProperty
            = DependencyProperty.Register("SymbolDirection", typeof(MButtonDirections), typeof(MonahrqButton));

		/// <summary>
		/// Gets or sets the type of the symbol.
		/// </summary>
		/// <value>
		/// The type of the symbol.
		/// </value>
		public MButtonSymbol SymbolType
        {
            get { return (MButtonSymbol)GetValue(SymbolTypeProperty); }
            set { SetValue(SymbolTypeProperty, value); }
        }

		/// <summary>
		/// The symbol type property
		/// </summary>
		public static readonly DependencyProperty SymbolTypeProperty
            = DependencyProperty.Register("SymbolType", typeof(MButtonSymbol), typeof(MonahrqButton));

    }
}
