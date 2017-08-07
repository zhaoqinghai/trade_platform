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
    public partial class MainWindow : Window
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
    }
}
