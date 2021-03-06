﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
 
using Newtonsoft.Json;

namespace DistributeSystem
{ 
    public class NetworkPackage
    {
        //[JsonProperty]
        public Dictionary<string, object> DataList;
        private Socket targetSocket;
        private List<Socket> targetSockets;

        [JsonIgnore]
        public Socket _target { get { return targetSocket; } }

        [JsonIgnore]
        public List<Socket> _targets { get { return targetSockets; } }

        public void SetTarget(Socket target)
        {
            targetSocket = target;
        }
        public void SetTargets(List<Socket> targets)
        {
            targetSockets = targets;
        }
        public NetworkPackage()
        {
            DataList = new Dictionary<string, object>();
        }
        public bool isEmpty()
        {
            return DataList.Count == 0;
        }
        public void Reset()
        {
            DataList = new Dictionary<string, object>();
            SetTarget(null);
            SetTargets(null);
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

        
        public Object Get(string name)
        {
            if (DataList == null) return null;
            if (!DataList.ContainsKey(name)) return null;

            return DataList[name];
        }

        public virtual T Get<T>(string name)
        {
            object obj = Get(name);
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
