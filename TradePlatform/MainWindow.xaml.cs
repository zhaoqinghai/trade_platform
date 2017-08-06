using Resource;
using System;
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

namespace TradePlatform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var type = ThemeType.Purple;
            while (true)
            {
                if(type == ThemeType.Purple)
                {
                    type = ThemeType.Amber;
                    ThemeSelector.Default.SetCurrentTheme(ThemeType.Amber);
                }
                else
                {
                    type = ThemeType.Purple;
                    ThemeSelector.Default.SetCurrentTheme(ThemeType.Purple);
                }
                await Task.Delay(2000);
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
    }
}
