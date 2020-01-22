using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconSCHARPClient.Models
{
    public class BatchLog
    {
        public string BatchID { get; set; }
        public string FileSource { get; set; }
        public string BatchTransactionStatus { get; set; }
        public string BatchException { get; set; }
        public string FileName { get; set; }

    }
}
