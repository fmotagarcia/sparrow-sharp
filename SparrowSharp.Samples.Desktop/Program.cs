
using Sparrow.Samples;
using Sparrow.Core;

namespace CoreWin
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new DesktopViewController(typeof(Benchmark), 960, 640);
        }
        
    }
}
