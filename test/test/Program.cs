using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ConsoleApplication1
{
    class Program
    {

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Com_construct();

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_setAddress(int id, [MarshalAs(UnmanagedType.LPStr)] string address);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_connect(int id);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_destroy(int id);  

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OmniDrive_construct();

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OmniDrive_setComId(int OdometryId,int ComId);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OmniDrive_setVelocity(int OmniDriveId, float Vx, float Vy, float Omega);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OmniDrive_destroy(int OmniDriveId);

        static void Main(string[] args)
        {
            Console.WriteLine("This is C# program");

            int comId = Com_construct();
            Console.WriteLine("comId={0}", comId);

            bool ok = Com_setAddress(comId, "192.168.2.105");
            Console.WriteLine("Com_setAddress returned {0}", ok);

            ok = Com_connect(comId);
            Console.WriteLine("Com_connect returned {0}", ok);

            int driverID = OmniDrive_construct();
            Console.WriteLine("Odometry_construct returned {0}", driverID);

            ok = OmniDrive_setComId(driverID, comId);
            Console.WriteLine("OmniDrive_setComId returned {0}", ok);

            while (true)
            {
                ok= OmniDrive_setVelocity(driverID, 10/1000, 10/1000, 10);
            Console.WriteLine("OmniDrive_setVelocity returned {0}", ok);
                var k=Console.ReadKey();
                if (k.KeyChar == 'z') break;
            }
            ok= OmniDrive_destroy(driverID);
            Console.WriteLine("OmniDrive_destroy returned {0}", ok);
             Console.ReadKey();

            ok = Com_destroy(comId);
            Console.WriteLine("Com_destroy returned {0}", ok);
             Console.ReadKey();

        }
    }
}