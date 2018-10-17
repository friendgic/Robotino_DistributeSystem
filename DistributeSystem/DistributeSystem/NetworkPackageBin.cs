using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DistributeSystem
{
    public enum DataType
    {
        NONE,
        INT,
        FLOAT,
        DOUBLE,
        STRING,
        LIST_INT,
        LIST_FLOAT,
        LIST_DOUBLE,
        LIST_BYTE,
        BYTE
    }
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
                var name = item.Key;
                byte namelength = (byte)name.Length;
                buffer.Add(namelength);
                for(int i = 0; i < namelength; i++)
                {
                    byte c = (byte) name[i];
                    buffer.Add(c);
                }

                List<byte> adding = new List<byte>();
                byte count = 0;
                byte type = 0;

                if (dat is int)
                {
                    adding = AddInt((int)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.INT;
                }
                if (dat is float)
                {
                    adding = AddFloat((float)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.FLOAT;
                }
                if (dat is double)
                {
                    adding = AddDouble((double)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.DOUBLE;
                }
                if (dat is string)
                {
                    adding = AddString((string)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.STRING;
                }
                if (dat is List<int>)
                {
                    adding = AddListInt((List<int>)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.LIST_INT;
                }
                if (dat is List<float>)
                {
                    adding = AddListFloat((List<float>)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.LIST_FLOAT;
                }
                if (dat is List<double>)
                {
                    adding = AddListDouble((List<double>)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.LIST_DOUBLE;
                }
                if (dat is List<byte>)
                {
                    adding = AddListByte((List<byte>)dat);
                    count = (byte)adding.Count;
                    type = (byte)DataType.LIST_BYTE;
                }
                if (dat is byte)
                {
                    adding = new List<byte>() { (byte)dat };
                    count = (byte)adding.Count;
                    type = (byte)DataType.BYTE;
                }

                buffer.Add(count);
                buffer.Add(type);
                buffer.AddRange(adding);
            }
            buffer.AddRange(AddTailer());
            return buffer;
        }
         

        private List<byte> AddListByte(List<byte> dat)
        {
            List<byte> buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(dat.Count));
            for (int i = 0; i < dat.Count; i++)
            {
                buf.Add(dat[i]);
            }
            return buf;
        }

        private List<byte> AddListDouble(List<double> dat)
        {
            List<byte> buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(dat.Count));
            for (int i = 0; i < dat.Count; i++)
            {
                buf.AddRange(BitConverter.GetBytes(dat[i]));
            }
            return buf;
        }

        private List<byte> AddListFloat(List<float> dat)
        {
            List<byte> buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(dat.Count));
            for (int i = 0; i < dat.Count; i++)
            {
                buf.AddRange(BitConverter.GetBytes(dat[i]));
            }
            return buf;
        }

        private List<byte> AddListInt(List<int> dat)
        {
            List<byte> buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(dat.Count));
            for(int i = 0; i < dat.Count; i++)
            {
                buf.AddRange(BitConverter.GetBytes(dat[i]));
            }
            return buf;
        }

        private List<byte> AddString(string value)
        {
            char[] chars = value.ToCharArray();
            byte[] byteArray = new byte[chars.Length];
            for(int i = 0; i < chars.Length; i++)
            {
                byteArray[i] = (byte)chars[i];
            }
            return new List<byte>(byteArray);
        }
        private List<byte> AddDouble(double value)
        {
            byte[] byteArray = BitConverter.GetBytes(value);
            return new List<byte>(byteArray);
        }

        private List<byte> AddFloat(float value)
        {
            byte[] byteArray = BitConverter.GetBytes(value);
            return new List<byte>(byteArray);
        }

        private List<byte> AddInt(int value)
        {
            byte[] byteArray = BitConverter.GetBytes(value);
            return new List<byte>( byteArray);
        }
        private List<byte> AddHeader()
        {
            List<byte> buff = new List<byte>();
            buff.Add((byte)version);
            buff.Add((byte)(DataList.Count));
            return buff;
        }
        private List<byte> AddTailer()
        {
            List<byte> buff = new List<byte>();
            buff.Add((byte)0xFF); 
            return buff;
        }

        ///////////////////////////Deseriallize/////////////////////////////////////

        public bool DeserializeBin(List<byte> raw)
        {
            DataList = new Dictionary<string, object>();
            if (raw.Count < 2) return false;
            version = raw[0];
            var count = raw[1];
            int index = 2;
            for(int i = 0; i < count; i++)
            {
                int nameLength = ReadAByte(raw,ref index);
                char[] charString = new char[nameLength];
                for (int k = 0; k < nameLength; k++)
                {
                    charString[k] = (char)ReadAByte(raw,ref index);
                }
                string name = new string(charString);

                int objLengt = ReadAByte(raw, ref index);
                DataType type = (DataType)ReadAByte(raw,ref index);

                byte[] objbyte = new byte[objLengt];
                for(int l = 0; l < objLengt; l++)
                {
                    objbyte[l] = ReadAByte(raw, ref index);
                }


                switch (type)
                {
                    case DataType.INT:
                        {
                            var dat = ReadAInt(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.FLOAT:
                        {
                            var dat = ReadAFloat(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.DOUBLE:
                        {
                            var dat = ReadADouble(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.STRING:
                        {
                            var dat = ReadAString(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_INT:
                        {
                            var dat = ReadALInt(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_FLOAT:
                        {
                            var dat = ReadALFloat(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_DOUBLE:
                        {
                            var dat = ReadALDouble(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_BYTE:
                        {
                            var dat = ReadALByte(objbyte);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.BYTE:
                        {
                            var dat = objbyte[0];
                            DataList.Add(name, dat);
                        }
                        break;
                }
            }

            var tailer = ReadAByte(raw, ref index);
            if (tailer != 0xFF) return false;
            return true;
        }
        private List<byte> ReadALByte(byte[] objbyte)
        {
            List<byte> buf = new List<byte>();
            int count = BitConverter.ToInt32(objbyte, 0);
            for (int i = 0; i < count; i++)
            {
                var one = objbyte[4 + i * sizeof(byte)];
                buf.Add(one);
            }
            return buf;
        }
        private List<double> ReadALDouble(byte[] objbyte)
        {
            List<double> buf = new List<double>();
            int count = BitConverter.ToInt32(objbyte, 0);
            for (int i = 0; i < count; i++)
            {
                var one = BitConverter.ToDouble(objbyte, 4+i * sizeof(double));
                buf.Add(one);
            }
            return buf;
        }

        private List<float> ReadALFloat(byte[] objbyte)
        {
            List<float> buf = new List<float>();
            int count = BitConverter.ToInt32(objbyte, 0);
            for (int i = 0; i < count; i++)
            {
                var one = BitConverter.ToSingle(objbyte, 4+i * sizeof(float));
                buf.Add(one);
            }
            return buf;
        }
        private List<int> ReadALInt(byte[] objbyte)
        {
            List<int> buf = new List<int>();
            int count = BitConverter.ToInt32(objbyte, 0);
            for (int i = 0; i < count; i++)
            {
                var one = BitConverter.ToInt32(objbyte, 4 + i * sizeof(int));
                buf.Add(one);
            }
            return buf;
        }

        private string ReadAString(byte[] objbyte)
        {
            char[] chars = new char[objbyte.Length];
            for(int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)objbyte[i];
            }
            string str = new string(chars);
            return str;
        }
        private double ReadADouble(byte[] objbyte)
        {
            return BitConverter.ToDouble(objbyte, 0);
        }
        private float ReadAFloat(byte[] objbyte)
        {
            return BitConverter.ToSingle(objbyte, 0);
        }
        private int ReadAInt(byte[] objbyte)
        {
            return BitConverter.ToInt32(objbyte, 0);
        }
        private byte ReadAByte(List<byte>raw,ref int index)
        {
            return raw[index++];
        }

        public override T Get<T>(string name)
        {
            if (DataList == null) throw new Exception("no data");
            if (!DataList.ContainsKey(name)) throw new Exception("no key");
            
            return (T)DataList[name];
        }
    }
}
