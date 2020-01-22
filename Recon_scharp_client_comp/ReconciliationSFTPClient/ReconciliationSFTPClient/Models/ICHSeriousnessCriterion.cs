using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ReconSCHARPClient.Models
{
    [Serializable]
    public class ICHSeriousnessCriterion
    {
        public string ParticipantIdentifier  { get; set; }

        [XmlText]
        public string ICHSeriousnessCriterionDescription { get; set; }
    }
}