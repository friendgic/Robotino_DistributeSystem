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

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern float PowerManagement_voltage(int id);
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PowerManagement_construct();
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PowerManagement_setComId(int id, int comId);
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PowerManagement_destroy(int id);  

        static void Main(string[] args)
        {
            Console.WriteLine("This is C# program");

            int comId = Com_construct();
            Console.WriteLine("comId={0}", comId);
            
            //bool ok = Com_setAddress(comId, "127.0.0.1");
            //bool ok = Com_setAddress(comId, "169.254.7.210");
            bool ok = Com_setAddress(comId, "172.26.1.103");
            Console.WriteLine("Com_setAddress returned {0}", ok);

            ok = Com_connect(comId);
            Console.WriteLine("Com_connect returned {0}", ok);

            int driverID = OmniDrive_construct();
            Console.WriteLine("OmniDrive_construct returned {0}", driverID);

            ok = OmniDrive_setComId(driverID, comId);
            Console.WriteLine("OmniDrive_setComId returned {0}", ok);

            int powerID = PowerManagement_construct();
            Console.WriteLine("PowerManagement_construct returned {0}", powerID);

            ok = PowerManagement_setComId(powerID, comId);
            Console.WriteLine("PowerManagement_setComId returned {0}", ok);




            while (true)
            {
                ok= OmniDrive_setVelocity(driverID, 0.04f, 0f, 0f);
                Console.WriteLine("OmniDrive_setVelocity returned {0}", ok);
                var value = PowerManagement_voltage(powerID);
            Console.WriteLine("PowerManagement_voltage returned {0}", value);

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