using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReconSCHARPClient.Models
{
    [Serializable]
    [XmlRoot(ElementName = "EAEs")]
    public class ListOfEaEs : List<EAE>
    {
    }
}
