using System;
using System.Diagnostics;
using System.Threading;
using SmartDeviceFirmware;

namespace SmartCurtainsFirmware
{
    public class Program
    {
        static Thread programThread;

        public static void Main()
        {
            SmartCurtains smartCurtains = new SmartCurtains();

            while (true) ;
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
