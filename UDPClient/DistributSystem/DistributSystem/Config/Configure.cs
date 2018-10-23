using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DistributeSystem
{
    public class Configure
    {
        static public string name = "Robot 1";
        static public string IPprefix = "192.168";

        #region Interface
        static public void Save()
        {
            NetworkPackage pack = new NetworkPackage();
            pack.Add("name", name);
            pack.Add("IPprefix",IPprefix);
            string str = pack.SerializeJson();

            string path = System.Environment.CurrentDirectory + @"/Confige.dat";
            File.WriteAllText(path, str, Encoding.Default);
        }

        static public void Read()
        {
            string path = System.Environment.CurrentDirectory + @"/Confige.dat";
            if (!File.Exists(path))
                Save();
            string readText = File.ReadAllText(path, Encoding.Default);
            try
            {
                NetworkPackage dp = new NetworkPackage();
                dp.DeserializeJson(readText);
                name = (string)dp.GetFromJson("name");
                IPprefix = (string)dp.GetFromJson("IPprefix");
            }
            catch (Exception e)
            {
                Read();
            }
        }

        #endregion
    }
}
