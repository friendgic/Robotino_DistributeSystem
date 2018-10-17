﻿using System;
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
        public new byte[] SerializeBin()
        {
            var ori=base.SerializeBin();
            var zip = DataCompress.ZipBin(ori);
            return zip;
        }

        public void DeserializeJson(byte[] zipData)
        {
            var unZip = DataCompress.Unzip(zipData);
            base.DeserializeJson(unZip);
        }
        public new void DeserializeBin(byte[] zipData)
        {
            var unZip = DataCompress.UnzipBin(zipData);
            base.DeserializeBin(unZip);
        }
    }
}
