using ControlLib.Assists;
using MahApps.Metro.Controls;
using Resource;
using Resource.Icons;
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
        public List<PathIconModel> EntypoList { get; set; } = new List<PathIconModel>();
        public List<PathIconModel> ModernList { get; set; } = new List<PathIconModel>();
        public List<PathIconModel> AwesomeList { get; set; } = new List<PathIconModel>();
        public List<PathIconModel> MaterialList { get; set; } = new List<PathIconModel>();
        public List<PathIconModel> LightList { get; set; } = new List<PathIconModel>();
        public List<PathIconModel> OcticonsList { get; set; } = new List<PathIconModel>();
        public MainWindow()
        {
            InitializeComponent();
            
            foreach (var item in PackIconDataFactory.DataFactoryDict[PackIconState.Entypo])
            {
                EntypoList.Add(new PathIconModel() { DataPath = item.Key, KindText = item.Key.ToString() });
            }
            foreach (var item in PackIconDataFactory.DataFactoryDict[PackIconState.Modern])
            {
                ModernList.Add(new PathIconModel() { DataPath = item.Key, KindText = item.Key.ToString() });
            }
            foreach (var item in PackIconDataFactory.DataFactoryDict[PackIconState.Awesome])
            {
                AwesomeList.Add(new PathIconModel() { DataPath = item.Key, KindText = item.Key.ToString() });
            }
            foreach (var item in PackIconDataFactory.DataFactoryDict[PackIconState.Light])
            {
                LightList.Add(new PathIconModel() { DataPath = item.Key, KindText = item.Key.ToString() });
            }
            foreach (var item in PackIconDataFactory.DataFactoryDict[PackIconState.Material])
            {
                MaterialList.Add(new PathIconModel() { DataPath = item.Key, KindText = item.Key.ToString() });
            }
            foreach (var item in PackIconDataFactory.DataFactoryDict[PackIconState.Octicons])
            {
                OcticonsList.Add(new PathIconModel() { DataPath = item.Key, KindText = item.Key.ToString() });
            }

            this.DataContext = this;
            Init();

        }
        async void Init()
        {
            
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
            icon.State = Resource.Icons.PackIconState.Entypo;

        }
    }

    public class PathIconModel
    {
        public PackIconKind DataPath
        {
            get;set;
        }
        public string KindText
        {
            get; set;
        }
    }
}
