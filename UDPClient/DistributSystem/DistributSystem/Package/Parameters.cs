using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeSystem 
{
    public class Parameters
    {
        #region Init
        private object locker = new object();
        public string v_name = "Robotino";
        public bool c_name=false;

        public List<float> v_Movement = new List<float>(3) { 0, 0, 0 };
        public bool c_Movement = false;

        public List<float> v_DisSensor = new List<float>(9) { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public bool c_DisSensor = false;
        #endregion

        #region Interface
        public string name
        {
            get
            {
                return v_name;
            }
            set
            {
                v_name = value;
                c_name = true;
            }
        }
        public float speed_x
        {
            get { return v_Movement[0]; }
            set {
                lock (locker)
                {

                v_Movement[0] = value; c_Movement = true;
                }
                }
        }
        public float speed_y
        {
            get { return v_Movement[1]; }
            set
            {
                lock (locker)
                { v_Movement[1] = value; c_Movement = true; }
            }
        }
        public float rot
        {
            get { return v_Movement[2]; }
            set 
            {  lock (locker){ v_Movement[2] = value; c_Movement = true; } }
        }
        public bool changed
        {
            get
            {
                return c_Movement | c_DisSensor | c_name;
            }
        }

        public void MarkAllChanged()
        {
            lock (locker)
            {
                c_Movement = true;
                c_DisSensor = true;
                c_name = true;
            }
        }
        public void SetDisSensor(int n, float val)
        {
            lock (locker)
            {
                if (v_DisSensor[n] != val)
                {
                    v_DisSensor[n] = val;
                    c_DisSensor = true;
                }
            }
        }

        public void BuildUpPackage(ref NetworkPackageCompress packageCompress)
        {
            lock (locker)
            {
                if (c_Movement)
                {
                    packageCompress.Add("c_Movement", v_Movement);
                    c_Movement = false;
                }
                if (c_DisSensor)
                {
                    packageCompress.Add("c_DisSensor", v_DisSensor);
                    c_DisSensor = false;
                }
                if (c_name)
                {
                    packageCompress.Add("c_Name", v_name);
                    c_name = false;
                }
            }
        }
        public void ReceivePackage(NetworkPackageCompress pack)
        {
            lock (locker)
            {
                List<float> dat = new List<float>();
                if (pack.GetFromBin("c_Movement", out dat))
                {
                    v_Movement = dat;
                }
                List<float> dat2 = new List<float>();
                if (pack.GetFromBin("c_DisSensor", out dat2))
                {
                    v_DisSensor = dat2; 
                }
                string str;
                if(pack.GetFromBin("c_Name",out str))
                {
                    v_name = str;
                }
            }
        }

        #endregion

    }
}
