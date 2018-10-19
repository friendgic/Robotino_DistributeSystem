using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace DistributeSystem
{
    public class NetworkPackageCompress : NetworkPackageBin
    {
        public new byte[] SerializeJson()
        {
            var ori= base.SerializeJson();
            var zip=DataCompress. Zip(ori);
            return zip;
        }
        public byte[] SerializeBin(bool compress)
        {
            var ori=base.SerializeBin();
            if (compress)
            {
            var zip = DataCompress.ZipBin(ori);
                return zip;
            }
            else
            {
            return ori;

            }
        }

        public void DeserializeJson(byte[] zipData)
        {
            var unZip = DataCompress.Unzip(zipData);
            base.DeserializeJson(unZip);
        }
        public void DeserializeBin(byte[] oriData,bool depress)
        {
            if (depress)
            {
            var unZip = DataCompress.UnzipBin(oriData);
            base.DeserializeBin(unZip);
            }
            else
            {
                base.DeserializeBin(oriData);
            }
        }
    }
}
