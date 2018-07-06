using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;

namespace ScreenBabel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int BorderSize;

        public MainWindow()
        {
            InitializeComponent();
            BorderSize = (int)(double)Resources["MainBorderSize"];
            Util.Recognition.Prepare(this);
            Closed += (sender, e) => Properties.Settings.Default.Save();
        }

        private Task Capture()
        {
            var left = (int)Left + BorderSize;
            var width = (int)MainInnerArea.ActualWidth;

            var topOffset = (MainTitleArea.IsVisible ? (int)MainTitleArea.ActualHeight : BorderSize);
            var top = (int)Top + topOffset;
            var height = (int)MainInnerArea.ActualHeight - topOffset;

            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));
            }
            return Util.Recognition.Recognize(bitmap);
        }

        #region Buttons


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            var setting = new Component.Setting();
            setting.ShowDialog();
        }

        #endregion

        #region Control

        private void ShowTitle_Click(object sender, RoutedEventArgs e)
        {
            MainTitleArea.Visibility = MainTitleArea.IsVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        private DispatcherTimer timer;
        private void CaptureAuto_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(IntervalInput.Text, @"^\d+$"))
            {
                MessageBox.Show((string)TryFindResource("Text.Tip.InvalidIntervalTime"));
                return;
            }
            var interval = int.Parse(IntervalInput.Text);

            CaptureAuto.Visibility = Visibility.Collapsed;
            CaptureAuto_Small.Visibility = Visibility.Collapsed;
            CaptureStop.Visibility = Visibility.Visible;
            CaptureStop_Small.Visibility = Visibility.Visible;
            IntervalInput.IsReadOnly = true;
            IntervalInput.IsEnabled = false;
            CaptureOnce.IsEnabled = false;
            CaptureOnce_Small.IsEnabled = false;

            if (timer != null)
            {
                CaptureStop_Click(null, null);
            }
            timer = new DispatcherTimer();
            timer.Tick += (_sender, _e) => Capture();
            timer.Interval = new System.TimeSpan(0, 0, 0, 0, interval);
            timer.Start();
        }

        private void CaptureStop_Click(object sender, RoutedEventArgs e)
        {
            CaptureAuto.Visibility = Visibility.Visible;
            CaptureAuto_Small.Visibility = Visibility.Visible;
            CaptureStop.Visibility = Visibility.Collapsed;
            CaptureStop_Small.Visibility = Visibility.Collapsed;
            IntervalInput.IsReadOnly = false;
            IntervalInput.IsEnabled = true;
            CaptureOnce.IsEnabled = true;
            CaptureOnce_Small.IsEnabled = true;

            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void CaptureOnce_Click(object sender, RoutedEventArgs e)
        {
            CaptureAuto.IsEnabled = false;
            CaptureAuto_Small.IsEnabled = false;
            CaptureOnce.IsEnabled = false;
            CaptureOnce_Small.IsEnabled = false;
            var task = Capture();
            var context = SynchronizationContext.Current;
            task.ContinueWith(_task => context.Post(state => {
                CaptureAuto.IsEnabled = true;
                CaptureAuto_Small.IsEnabled = true;
                CaptureOnce.IsEnabled = true;
                CaptureOnce_Small.IsEnabled = true;
            }, null));
        }

        private void IntervalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var visibility = ActualWidth < 350 ? Visibility.Collapsed : Visibility.Visible;
            foreach (var element in new UIElement[] { TitleLabel, ButtonGrid, Exit_Small })
            {
                element.Visibility = visibility;
            }
        }

        #endregion
    }
}
