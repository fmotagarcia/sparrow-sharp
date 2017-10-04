using System;
using Sparrow.Core;

namespace Sparrow.Samples.Desktop
{
    class MainClass
    {
        [STAThread]
        public static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            
            System.Windows.Application app = new System.Windows.Application();
            app.Run(new DesktopViewController(typeof(Benchmark), 960, 640));

        }
    }
}