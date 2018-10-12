using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTest
{
    class MainProgram
    {
        public static int Main(String[] args)
        {
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
                AsynchronousClient.StartClient(str);

            }
            if (input == "3")
            {
                Console.Write("Please Input the Server Ip Address:");
                var str = Console.ReadLine();
                AsynchronousClient.StartClient(str,true);

            }
            return 0;
        }
    }
}
