using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TradePlatform
{
    public class Startup
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }


    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private App app;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            System.Diagnostics.Process[] oldfmdzec = System.Diagnostics.Process.GetProcessesByName("fmdzec");
            System.Diagnostics.Process[] oldfwhmdzec = System.Diagnostics.Process.GetProcessesByName("WHSafe");

            if (oldfmdzec.Length > 0 || oldfwhmdzec.Length > 0)
            {
                if (oldfwhmdzec.Length > 0)
                    MessageBox.Show("发现有仓储安全卫士在运行，请退出之后再运行");
                else
                    MessageBox.Show("发现有旧版飞马安全卫士在运行，请退出旧版之后再运行");
                Application.Current.Shutdown();
                return false;
            }
            // First time app is launched
            app = new App();
            app.InitializeComponent();
            app.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            //app.Activate();
        }
    }
}
