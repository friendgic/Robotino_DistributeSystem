using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTest2
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkConfig config = new NetworkConfig();
            config.port = 11000;
            NetworkServer server = new NetworkServer(config);
            
            Console.ReadLine();
        }
    }
}
