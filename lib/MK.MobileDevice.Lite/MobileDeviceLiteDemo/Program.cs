using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MK.MobileDevice.Lite;

namespace MobileDeviceLiteDemo
{
    class Program
    {
        static iOSDeviceMK iph;
        static void Main(string[] args)
        {            
            iph = new iOSDeviceMK();
            Console.Clear();
            Console.WriteLine(@"MK.MobileDevice.Lite
(c) 2015 MK Industries and ExaPhaser Industries
Created and Developed by MK and ExaPhaser in collaboration.
Source code is available on GitHub at http://github.com/mkindustries.

");
            iph.Connect += Iph_Connect;
            iph.Disconnect += Iph_Disconnect;
            while (true)
            { }
        }

        private static void Iph_Disconnect(object sender, ITMDConnectEventArgs args)
        {
            Console.WriteLine("iOS Device Disconnected.");
        }

        private static void Iph_Connect(object sender, ITMDConnectEventArgs args)
        {
            Console.WriteLine("iOS Device Connected in USB Multiplexing Mode.");
            Console.WriteLine("Device is named {0}, an {1} running iOS {2}", iph.DeviceName, iph.DeviceProductType, iph.DeviceVersion);
            bool isPhone = iph.DeviceProductType.StartsWith("iPhone");
            if (isPhone)
                Console.WriteLine("Phone Number {0}",iph.DevicePhoneNumber);
        }
    }
}
