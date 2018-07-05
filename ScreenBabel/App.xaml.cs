using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenBabel
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private Dictionary<string, IEnumerable<ResourceDictionary>> localizations;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            localizations = Current.Resources.MergedDictionaries.Select(res =>
            {
                var groups = Regex.Match(res.Source?.OriginalString, @"^.*/Localization/(.+)-(\w+)\.xaml$").Groups;
                if (groups.Count != 3)
                {
                    return null;
                }

                return new { key = groups[2].ToString(), value = res };
            }).Where(entry => entry != null).GroupBy(entry => entry.key, entry => entry.value).ToDictionary(group => group.Key, group => group.AsEnumerable());

            Localization(CultureInfo.InstalledUICulture);
        }

        private void Localization(string culture) { Localization(CultureInfo.CreateSpecificCulture(culture)); }
        private void Localization(CultureInfo culture)
        {
            var resouces = FindLocalization(culture);
            if (resouces == null)
            {
                throw new ArgumentException($"unsupport language: {culture}");
            }
            foreach (var res in resouces)
            {
                Current.Resources.MergedDictionaries.Remove(res);
                Current.Resources.MergedDictionaries.Add(res);
            }
        }
        private IEnumerable<ResourceDictionary> FindLocalization(CultureInfo culture)
        {
            if (localizations.ContainsKey(culture.Name))
            {
                return localizations[culture.Name];
            }
            if (String.IsNullOrEmpty(culture.Parent.Name))
            {
                return null;
            }
            return FindLocalization(culture.Parent);
        }
    }
}
