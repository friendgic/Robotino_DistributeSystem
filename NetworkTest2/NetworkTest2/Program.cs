using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistributeSystem;

namespace NetworkTest2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please select functions:");
            Console.WriteLine("[1] Run As Server");
            Console.WriteLine("[2] Run As Client");
            var input = Console.ReadKey();
            if (input.KeyChar == '1')
            {
                NetworkServer server = new NetworkServer();
                while (true)
                {
                    var command = Console.ReadLine();

                }
            }
            if (input.KeyChar == '2')
            {
                Console.Write("Please Input the Server Ip Address:");
                var serverip = Console.ReadLine();
                NetworkClient client = new NetworkClient(serverip);
                while (true)
                {
                    var command = Console.ReadLine();
                    
                }
            }

            Console.ReadLine();
        }
    }
}
