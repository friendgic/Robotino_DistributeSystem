using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTest
{
    class MainProgram
    {
        public static int Main(String[] args)
        {
            Robot robot = new Robot();
            Console.WriteLine("Please select functions:");
            Console.WriteLine("[1] Run As Server");
            Console.WriteLine("[2] Run As Client");
            Console.WriteLine("[3] Run As Listenner");
            var input = Console.ReadLine(); 
            if (input == "1")
            {
                AsynchronousSocketListener.StartListening();

            }
            if (input == "2")
            {
                Console.Write("Please Input the Server Ip Address:");
                var str = Console.ReadLine();
                Console.Write("Please Input the Robot Ip Address:");
                var str2 = Console.ReadLine();
                robot.Init(str2);
               
                AsynchronousClient.StartClient(str,false,robot);

            }
            if (input == "3")
            {
                Console.Write("Please Input the Server Ip Address:");
                var str = Console.ReadLine();
                Console.Write("Please Input the Robot Ip Address:");
                var str2 = Console.ReadLine();
                robot.Init(str2);
                AsynchronousClient.StartClient(str,true,robot);
            }
            return 0;
        }
    }
}
