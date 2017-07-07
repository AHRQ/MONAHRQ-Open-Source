using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Monahrq.DataSets.Controls
{
    /// <summary>
    /// Interaction logic for DataGridWithPopup.xaml
    /// </summary>
    public partial class DataGridWithPopup : UserControl
    {
        private Point initSegmentPoint { get; set; }

        public DataGridWithPopup()
        {
            InitializeComponent();
            initSegmentPoint = this.segment.Point;
        }

        public void UpdatePosition(object sender, System.Windows.RoutedEventArgs e)
        {
            var sb = this.FindResource("MovePopUpPointer") as Storyboard;
            var col = sb.Children[0] as PointAnimationUsingKeyFrames;
            var frame = col.KeyFrames[0] as EasingPointKeyFrame;

            var point = Mouse.GetPosition(this);

            frame.Value = new Point(initSegmentPoint.X, point.Y);
            sb.Begin();
        }
    }
}
