using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DistributeSystem
{
    public class NetworkPackageBin:NetworkPackage
    {
        static public int version = 1;
        public List<byte> SerializeBin()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(AddHeader());
            foreach(var item in DataList)
            {
                var dat = item.Value;
                if(dat is int)
                {
                    Console.WriteLine("INT");
                }
            }
            return null;
        }

        private List<byte> AddHeader()
        {
            List<byte> buff = new List<byte>();
            buff.Add((byte)version);
            buff.Add((byte)(DataList.Count));
            return buff;
        }
    }
}
