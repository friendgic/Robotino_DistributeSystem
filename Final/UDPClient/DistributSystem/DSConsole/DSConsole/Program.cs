using System;
using DistributeSystem;
namespace DSConsole
{
    public class MainClass
    {
        public static RobotAgent robotAgent = new RobotAgent();
        public static string port="11000";
        public static void Main(string[] args)
        {
            Init();
            Console.WriteLine("\t\tWelcome to Distributesystem 1.0");
            Console.WriteLine("\t\ttype help to get help");

            string cmd = "";
            while(cmd!="end"){
                Console.Write("Command:");
                cmd = Console.ReadLine();
                switch(cmd){
                    case "end":
                        break;
                    case "help":
                        Help();
                        break;
                    case "start":
                        Start();
                        break;
                    case "port":
                        SetPort();
                        break;
                    case "name":
                        SetName();
                        break;
                    case "test":
                        Test();
                        break;
                    case "neighbors":
                        Neighbors();
                        break;
                    case "friends":
                        Friends();
                        break;
                    case "connect":
                        Connect();
                        break;
                    default:
                        Console.WriteLine("Command not find");
                        break;
                }
            }

        }

        private static void Neighbors()
        {
            var neighbors = robotAgent.GetConnectedAgentFromBC();
            for (int i = 0; i < neighbors.Count; i++)
            {
                var item = neighbors[i];

                Console.WriteLine(i.ToString() + ": "+ neighbors[i]);
            }
        }

        private static void Connect()
        {
            var neighbors = robotAgent.GetConnectedAgentFromBC();
            Neighbors();
            Console.Write ("Please input the index of the client you want to connect: ");
            var input = Console.ReadLine();
            int index = int.Parse(input);
            robotAgent.AddTarget(neighbors[index]);
        }

        private static void Friends()
        {
            var friends = robotAgent.friends;
            for (int i = 0; i < friends.Count; i++)
            {
                var item = friends[i];

                Console.WriteLine(i.ToString() + ": " + item.localIP +":"+ item.localPort);
            }
        }

        private static void Test()
        {
            Console.WriteLine("Now,test the data change, press any key to change the x. test 50 times");
            for (int i = 0; i < 50;i++){
                var x = robotAgent.parameters.v_Movement[0];
                robotAgent.parameters.SetSpeedX(x+1);
                Console.WriteLine("Test " + i.ToString() + ": x=" + x.ToString());
                Console.ReadKey();
            }
        }

        public static void Start()
        {
            int p = int.Parse(port);
            robotAgent.Start(p);
            Console.WriteLine("Local ip:" + robotAgent.localIP);
        }

        public static void SetPort()
        {
            Console.Write("Please input new port:");
            port = Console.ReadLine();
        }
        public static void SetName()
        {
            Console.Write("Please input new Name:");
            var newName = Console.ReadLine();
            Configure.name = newName;
            Configure.Save();
        }

        public static void Init(){

        }
        public static void Help()
        {
            Console.WriteLine("start\t- start client");
            Console.WriteLine("port\t- set port");
            Console.WriteLine("name\t- set client name");
            Console.WriteLine("Test\t- test change parameter");
            Console.WriteLine("friends\t-see friends");
            Console.WriteLine("connect\t-connect with ...");
            Console.WriteLine("neighbors\t-see neighbors");
        }
    }
}
