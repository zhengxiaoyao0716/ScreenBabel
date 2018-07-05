using System;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace ScreenBabel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly int BorderSize;

        public MainWindow()
        {
            InitializeComponent();
            BorderSize = (int)(double)Resources["MainBorderSize"];
            Util.Recognition.Prepare();
        }

        private void Capture()
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
            Util.Recognition.Recognize(bitmap);
        }

        #region Buttons

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            // TODO .
        }

        #endregion

        #region Control

        private void ShowTitle_Click(object sender, RoutedEventArgs e)
        {
            MainTitleArea.Visibility = MainTitleArea.IsVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CaptureAuto_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(IntervalInput.Text, @"^\d+$"))
            {
                IntervalInput.Text = ""; // TODO alert
                return;
            }
            CaptureAuto.Visibility = Visibility.Collapsed;
            CaptureAuto_Small.Visibility = Visibility.Collapsed;
            CaptureStop.Visibility = Visibility.Visible;
            CaptureStop_Small.Visibility = Visibility.Visible;
            IntervalInput.IsReadOnly = true;
        }

        private void CaptureStop_Click(object sender, RoutedEventArgs e)
        {
            CaptureAuto.Visibility = Visibility.Visible;
            CaptureAuto_Small.Visibility = Visibility.Visible;
            CaptureStop.Visibility = Visibility.Collapsed;
            CaptureStop_Small.Visibility = Visibility.Collapsed;
            IntervalInput.IsReadOnly = false;
        }

        private void CaptureOnce_Click(object sender, RoutedEventArgs e)
        {
            Capture();
        }

        private void IntervalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth < 430)
            {
                TitleLabel.Visibility = Visibility.Collapsed;
                ButtonGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                TitleLabel.Visibility = Visibility.Visible;
                ButtonGrid.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}
