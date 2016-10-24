using System;
using Sparrow.Core;
using System.Windows.Forms;

namespace Sparrow.Samples.Desktop
{
    class MainClass
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DesktopViewController dvc = new DesktopViewController(typeof(Benchmark), 960, 640);
            Application.Run(dvc);

        }
    }
}