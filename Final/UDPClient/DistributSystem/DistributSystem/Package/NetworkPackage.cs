using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;using System.Text;
 
using Newtonsoft.Json;

namespace DistributeSystem
{ 
    public class NetworkPackage
    {
        public Dictionary<string, object> DataList=new Dictionary<string, object>();
        [JsonIgnore]
        public List<string> targetIPs=new List<string>();
        [JsonIgnore]
        public List<int> targetPorts=new List<int>();

        public void AddTarget(string ip,int port)
        {
            for(int i = 0; i < targetIPs.Count; i++)
            {
                var item = targetIPs[i];
                var item2 = targetPorts[i];
                if(item==ip && item2 == port)
                {
                    return;
                }
            }
            targetIPs.Add(ip);
            targetPorts.Add(port);
        }

        public bool isEmpty()
        {
            return DataList.Count == 0;
        }
        public void Reset()
        {
            DataList = new Dictionary<string, object>();
            targetIPs = new List<string>();
            targetPorts = new List<int>();
        }
 
        public void Add(string name, Object obj)
        {
                if (name == "") { return; }
            if (!DataList.ContainsKey(name))
            {
                DataList.Add(name, obj);
            }
            else
            {
                DataList[name] = obj;
            }
        }

        override public string ToString()
        {
            string str = "";
            foreach(var item in DataList)
            {
                str = str + item.Key + "=" + item.Value.ToString()+"\n";
            }
            return str;
        }
     
        public virtual string SerializeJson()
        {
            string str;
            str = JsonConvert.SerializeObject(this)+ "<EOF>";
            return str;
        }

        public virtual void DeserializeJson(string data)
        {
            try
            {
                var endpos = data.IndexOf("<EOF>");
                if (endpos > -1)
                {
                    data = data.Remove(endpos, 5);//remove <EOF>
                }
                NetworkPackage old = JsonConvert.DeserializeObject<NetworkPackage>(data);
                DataList = old.DataList;
            }
            catch
            {
                Console.WriteLine("Deserialize Data Error");
            }
        }

        
        public Object GetFromJson(string name)
        {
            if (DataList == null) return null;
            if (!DataList.ContainsKey(name)) return null;

            return DataList[name];
        }

        public virtual T GetFromJson<T>(string name)
        {
            object obj = GetFromJson(name);
            if (obj != null)
            {
                string str = JsonConvert.SerializeObject(obj);
                return JsonConvert.DeserializeObject<T>(str);
            }
            else
            {
                return default(T);
            }
        }

    }
}
