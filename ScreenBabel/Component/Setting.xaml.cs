using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace ScreenBabel.Component
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        private bool dirty;
        public Setting()
        {
            InitializeComponent();
            ScreenBabel.Resources.Setting.Default.PropertyChanged += (sender, e) => {
                dirty = true;
            };
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!dirty)
            {
                return;
            }
            ScreenBabel.Resources.Setting.Default.Save();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            //MessageBox.Show(String.Format((string)TryFindResource("Text.Tip.SettingsSaved"), config.FilePath));
            Process.Start(Path.GetDirectoryName(config.FilePath));
            dirty = false;
        }
    }
    public class RecognitionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null;
            }
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null;
            }
            return (bool)value ? parameter : Resources.Setting.Default.RecognitionMode;
        }
    }
}
