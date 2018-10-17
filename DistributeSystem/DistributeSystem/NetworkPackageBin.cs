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
        public virtual byte[] SerializeBin()
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
                byte type = 0;

                if (dat is int)
                {
                    adding = AddInt((int)dat);
                    type = (byte)DataType.INT;
                }
                if (dat is float)
                {
                    adding = AddFloat((float)dat);
                    type = (byte)DataType.FLOAT;
                }
                if (dat is double)
                {
                    adding = AddDouble((double)dat);
                    type = (byte)DataType.DOUBLE;
                }
                if (dat is string)
                {
                    adding = AddString((string)dat);
                    type = (byte)DataType.STRING;
                }
                if (dat is List<int>)
                {
                    adding = AddListInt((List<int>)dat);
                    type = (byte)DataType.LIST_INT;
                }
                if (dat is List<float>)
                {
                    adding = AddListFloat((List<float>)dat);
                    type = (byte)DataType.LIST_FLOAT;
                }
                if (dat is List<double>)
                {
                    adding = AddListDouble((List<double>)dat);
                    type = (byte)DataType.LIST_DOUBLE;
                }
                if (dat is List<byte>)
                {
                    adding = AddListByte((List<byte>)dat);
                    type = (byte)DataType.LIST_BYTE;
                }
                if (dat is byte)
                {
                    adding = new List<byte>() { (byte)dat };
                    type = (byte)DataType.BYTE;
                }

                buffer.Add(type);
                buffer.AddRange(adding);
            }
            buffer.AddRange(AddTailer());
            return buffer.ToArray();
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
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(chars.Length));
            buf.AddRange(byteArray);
            return buf;
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

        public virtual void DeserializeBin(byte[] raw)
        {
            DataList = new Dictionary<string, object>();
            if (raw.Length < 2) throw new Exception("data count<2")  ;
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

                DataType type = (DataType)ReadAByte(raw,ref index);

                switch (type)
                {
                    case DataType.INT:
                        {
                            var dat = ReadAInt(raw,ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.FLOAT:
                        {
                            var dat = ReadAFloat(raw,ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.DOUBLE:
                        {
                            var dat = ReadADouble(raw, ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.STRING:
                        {
                            var dat = ReadAString(raw, ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_INT:
                        {
                            var dat = ReadALInt(raw, ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_FLOAT:
                        {
                            var dat = ReadALFloat(raw, ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_DOUBLE:
                        {
                            var dat = ReadALDouble(raw, ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.LIST_BYTE:
                        {
                            var dat = ReadALByte(raw, ref index);
                            DataList.Add(name, dat);
                        }
                        break;
                    case DataType.BYTE:
                        {
                            var dat = ReadAByte(raw, ref index);

                            DataList.Add(name, dat);
                        }
                        break;
                }
            }

            var tailer = ReadAByte(raw, ref index);
            if (tailer != 0xFF) throw new Exception("data tailer error");
           
        }
        private List<byte> ReadALByte(byte[] objbyte, ref int index)
        {
            List<byte> buf = new List<byte>();
            int count = ReadAInt(objbyte, ref index);
            for (int i = 0; i < count; i++)
            {
                var one = ReadAByte(objbyte, ref index);
                buf.Add(one);
            }
            return buf;
        }
        private List<double> ReadALDouble(byte[] objbyte, ref int index)
        {
            List<double> buf = new List<double>();
            int count = ReadAInt(objbyte, ref index);
            for (int i = 0; i < count; i++)
            {
                var one = ReadADouble(objbyte, ref index);
                buf.Add(one);
            }
            return buf;
        }

        private List<float> ReadALFloat(byte[] objbyte, ref int index)
        {
            List<float> buf = new List<float>();
            int count = ReadAInt(objbyte,ref index);
            for (int i = 0; i < count; i++)
            {
                var one = ReadAFloat(objbyte,ref index);
                buf.Add(one);
            }
            return buf;
        }
        private List<int> ReadALInt(byte[] objbyte, ref int index)
        {
            List<int> buf = new List<int>();
            int count = ReadAInt(objbyte, ref index);
            for (int i = 0; i < count; i++)
            {
                var one = ReadAInt(objbyte,ref index);
                buf.Add(one);
            }
            return buf;
        }

        private string ReadAString(byte[] objbyte, ref int index)
        {
            int length = ReadAInt(objbyte, ref index);

            char[] chars = new char[length];
            for(int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)ReadAByte(objbyte,ref index);
            }
            string str = new string(chars);
            return str;
        }
        private double ReadADouble(byte[] objbyte, ref int index)
        {
            var value = BitConverter.ToDouble(objbyte, index);
            index += sizeof(double);
            return value;
        }
        private float ReadAFloat(byte[] objbyte, ref int index)
        {
            var value = BitConverter.ToSingle(objbyte, index);
            index = index + sizeof(float);
            return value;
        }
        private int ReadAInt(byte[] objbyte,ref int index)
        {
            var value = BitConverter.ToInt32(objbyte, index);
            index = index + sizeof(int);
            return value;
        }
        private byte ReadAByte(byte[]raw,ref int index)
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
