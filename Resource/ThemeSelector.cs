using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resource
{
    public class ThemeSelector
    {
        bool _currentIsDark;

        ThemeProvider _themeProvider;

        static ThemeSelector _themeSelector;

        ThemeType _currentTheme;

        private ConcurrentDictionary<DarkLightTheme, ResourceDictionary> _dictResource;

        public static ThemeSelector Default
        {
            get
            {
                _themeSelector = _themeSelector ?? new ThemeSelector();
                return _themeSelector;
            }
        }

        public void SetCurrentTheme(bool isDark,bool isAnimation)
        {
            if (isDark == _currentIsDark)
                return;

            _currentIsDark = isDark;
            var existingResourceDictionary = Application.Current.Resources.MergedDictionaries
            .Where(rd => rd.Source != null)
            .Where(rd => Regex.Match(rd.Source.OriginalString, @"(\/Resource;component\/BrushBox\/(DarkTheme)|(LightTheme))").Success).ToList().FirstOrDefault();
            if (isAnimation)
            {
                var type = _currentIsDark ? DarkLightTheme.Dark : DarkLightTheme.Light;
                var replaceDiction = _dictResource.GetOrAdd(type,
                    new ResourceDictionary()
                    {
                        Source =
                            new Uri(
                                $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/DarkTheme.xaml")
                    });

                if (replaceDiction != null && existingResourceDictionary != null)
                    foreach (var item in existingResourceDictionary.Keys)
                    {
                        var extingBrush = existingResourceDictionary[item] as SolidColorBrush;
                        var replaceBrush = replaceDiction[item] as SolidColorBrush;
                        if (extingBrush == null || replaceBrush == null)
                            continue;
                        if (extingBrush.IsFrozen)
                        {
                            existingResourceDictionary[item] = replaceBrush;
                            continue;
                        }
                        var animation = new ColorAnimation
                        {
                            From = extingBrush.Color,
                            To = replaceBrush.Color,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300))
                        };
                        //existingResourceDictionary[item] = replaceBrush;
                        extingBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                    }
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Remove(existingResourceDictionary);
                var key = isDark ? DarkLightTheme.Dark : DarkLightTheme.Light;
                Application.Current.Resources.MergedDictionaries.Add(_dictResource.GetOrAdd(key, new ResourceDictionary()));
            }
        }

        public void SetCurrentTheme(bool isDark)
        {
            SetCurrentTheme(isDark, false);
        }

        public void SetDefaultTheme(bool isDark)
        {
            var existingResourceDictionary = Application.Current.Resources.MergedDictionaries
               .Where(rd => rd.Source != null)
               .Where(rd => Regex.Match(rd.Source.OriginalString, @"(\/Resource;component\/BrushBox\/(DarkTheme)|(LightTheme))").Success).ToList();
            _currentIsDark = isDark;
            if (existingResourceDictionary.Any())
            {
                foreach (var item in existingResourceDictionary)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(item);
                }
            }
            var darkBrushSource = isDark ?  $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/DarkTheme.xaml" : $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/LightTheme.xaml";
            var darkBrushSourceSourceDictionary = new ResourceDictionary() { Source = new Uri(darkBrushSource) };
            Application.Current.Resources.MergedDictionaries.Add(darkBrushSourceSourceDictionary);

            var darkColors = new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/DarkAnimationColors.xaml") };
            Application.Current.Resources.MergedDictionaries.Add(darkColors);

            var lightColors = new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/LightAnimationColors.xaml") };
            Application.Current.Resources.MergedDictionaries.Add(lightColors);


        }

        private ThemeSelector() : this(assmbly:Assembly.GetExecutingAssembly())
        {

        }

        private ThemeSelector(Assembly assmbly)
        {
            var assmblyName = assmbly.GetName().Name;

           
            Task.Run(() =>
            {
                _dictResource = new ConcurrentDictionary<DarkLightTheme, ResourceDictionary>();
                _dictResource.GetOrAdd(DarkLightTheme.Dark, new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{assmblyName};component/BrushBox/DarkTheme.xaml")});
                _dictResource.GetOrAdd(DarkLightTheme.Light, new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{assmblyName};component/BrushBox/LightTheme.xaml") });
               
            });

        }
        

        /// <summary>
        /// 可以在配置文件进行设置
        /// </summary>
        public void SetDefaultTheme(ThemeType type)
        {

            _themeProvider = _themeProvider ?? new ThemeProvider();

            var existingResourceDictionary = Application.Current.Resources.MergedDictionaries
              .Where(rd => rd.Source != null)
              .Where(rd => Regex.Match(rd.Source.OriginalString, @"(\/Resource;component\/BrushBox\/(Accent)|(Primary))").Success).ToList();
            if (existingResourceDictionary.Any())
            {
                foreach (var item in existingResourceDictionary)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(item);
                }
            }
            var accentColorSource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/ColorBox/{type}_accent.xaml";
            var accentColorSourceDictionary = new ResourceDictionary() { Source = new Uri(accentColorSource) };
            Application.Current.Resources.MergedDictionaries.Add(accentColorSourceDictionary);

            var accentSource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/Accent/Accent.xaml";
            var accentSourceDictionary = new ResourceDictionary() { Source = new Uri(accentSource) };
            Application.Current.Resources.MergedDictionaries.Add(accentSourceDictionary);

            var primaryColorSource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/ColorBox/{type}_primary.xaml";
            var primaryColorSourceDictionary = new ResourceDictionary() { Source = new Uri(primaryColorSource) };
            Application.Current.Resources.MergedDictionaries.Add(primaryColorSourceDictionary);

            var primarySource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/Primary/Primary.xaml";
            var primarySourceDictionary = new ResourceDictionary() { Source = new Uri(primarySource) };
            Application.Current.Resources.MergedDictionaries.Add(primarySourceDictionary);

            //_currentTheme = (ThemeType)(Enum.GetValues(typeof(ThemeType)).Length - (int)type);
            //SetCurrentTheme(type,false);
            _currentTheme = type;
        }

        public void SetCurrentTheme(ThemeType type)
        {
            var accentColorSource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/ColorBox/{_currentTheme}_accent.xaml";
            var primaryColorSource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/ColorBox/{_currentTheme}_primary.xaml";
            var accentSource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/Accent/Accent.xaml";
            var primarySource = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/Primary/Primary.xaml";
            if (type != _currentTheme)
            {
                _currentTheme = type;
                var list = Application.Current.Resources.MergedDictionaries.Where(rd => rd.Source.OriginalString == accentColorSource || rd.Source.OriginalString == primaryColorSource || rd.Source.OriginalString == accentSource || rd.Source.OriginalString == primarySource).ToList();

                foreach (var item in list)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(item);

                }
                var accentColorSource1 = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/ColorBox/{type}_accent.xaml";
                var accentColorSourceDictionary = new ResourceDictionary() { Source = new Uri(accentColorSource1) };
                Application.Current.Resources.MergedDictionaries.Add(accentColorSourceDictionary);
                var primaryColorSource1 = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/ColorBox/{type}_primary.xaml";
                var primaryColorSourceDictionary = new ResourceDictionary() { Source = new Uri(primaryColorSource1) };
                Application.Current.Resources.MergedDictionaries.Add(primaryColorSourceDictionary);
                var accentSource1 = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/Accent/Accent.xaml";
                var accentSourceDictionary = new ResourceDictionary() { Source = new Uri(accentSource1) };
                Application.Current.Resources.MergedDictionaries.Add(accentSourceDictionary);
                var primarySource1 = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/BrushBox/Primary/Primary.xaml";
                var primarySourceDictionary = new ResourceDictionary() { Source = new Uri(primarySource1) };
                Application.Current.Resources.MergedDictionaries.Add(primarySourceDictionary);
            }
        }
    }

    public enum ThemeType
    {
        Amber,
        Purple
    }

    public enum DarkLightTheme
    {
        Dark,
        Light
    }

    public enum AccentBrushes
    {
        AccentLightBrush,
        AccentLightForegroundBrush,
        AccentMidBrush,
        AccentMidForegroundBrush,
        AccentDarkBrush,
        AccentDarkForegroundBrush
    }

    public enum PrimaryBrushes
    {
        PrimaryLightBrush,
        PrimaryLightForegroundBrush,
        PrimaryMidBrush,
        PrimaryMidForegroundBrush,
        PrimaryDarkBrush,
        PrimaryDarkForegroundBrush
    }

}
