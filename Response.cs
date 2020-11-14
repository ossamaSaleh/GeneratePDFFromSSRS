using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOCD.CS.Report.Generate
{
    public class Response
    {

        public class Rootobject
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Culture { get; set; }
            public string FileName { get; set; }
            public string TransactionHash { get; set; }
            public string FileHash { get; set; }
            public string StampedFile { get; set; }
            public string StampTransactionHash { get; set; }
            public string StampFileHash { get; set; }
            public byte[] StampContent { get; set; }
            public string ZipFile { get; set; }
            public string UploadZipHash { get; set; }
            public string TimeStamp { get; set; }
            public string Owner { get; set; }
            public string Tags { get; set; }
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public string AccountKey { get; set; }
            public string VirtualURL { get; set; }
        }

    }
}
