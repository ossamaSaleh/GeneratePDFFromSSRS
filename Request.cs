using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOCD.CS.Report.Generate
{
    public class Request
    {
        public string AccountKey { get; set; }
        public string Operation { get; set; }
        public string Culture { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
        public string Tags { get; set; }
        public string Base64Content { get; set; }
    }
}
