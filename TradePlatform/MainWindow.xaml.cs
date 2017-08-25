using ControlLib.Assists;
using MahApps.Metro.Controls;
using Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TradePlatform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            if ((bool)btn.IsChecked)
            {
                ThemeSelector.Default.SetCurrentTheme(ThemeType.Purple);
            }
            else
            {
                ThemeSelector.Default.SetCurrentTheme(ThemeType.Amber);
            }

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var btn = sender as CheckBox;
            if ((bool)btn.IsChecked)
            {
                ThemeSelector.Default.SetCurrentTheme(true);
            }
            else
            {
                ThemeSelector.Default.SetCurrentTheme(false);
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var value = 0;
            while (value < 100)
            {
                ButtonProgressAssist.SetValue(sender as DependencyObject, value);
                await Task.Delay(30);
                value++;
            }
            await Task.Delay(100);
            ButtonProgressAssist.SetValue(sender as DependencyObject, 0);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            icon.State = Resource.Icons.PackIconState.Traditional;

        }
    }
}
