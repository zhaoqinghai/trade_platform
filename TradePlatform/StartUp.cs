using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradePlatform
{
    public class StartUp
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
            app = new App();
            app.InitializeComponent();
            app.Run();
            return false;
        }



        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            app.ActiveApp();
        }
    }
}
