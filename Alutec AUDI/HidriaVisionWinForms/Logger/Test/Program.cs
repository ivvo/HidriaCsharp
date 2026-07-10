using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logger.ImageLogger;
using System.Drawing;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            FileImageLogger Log = new FileImageLogger("./", "Station01", 1, 1, ErrorHandle);
            Log.Prepare();
            Log.Start();
            int i = 0;
            while(true)
            {
                Log.MaxNumberOfImages = 10;
                Log.AddEntry(new ImageLogEntry(new Bitmap(10, 10), DateTime.Now, 1, true));
                Thread.Sleep(new Random().Next() % 1000 + 100);
            }

            Log.Stop();
            Log.Dispose();
        }

        public static void ErrorHandle(Task t)
        {

        }
    }
}
