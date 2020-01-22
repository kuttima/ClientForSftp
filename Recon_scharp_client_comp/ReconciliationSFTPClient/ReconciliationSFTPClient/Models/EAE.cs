using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ReconSCHARPClient.Models
{
    [Serializable]
    [XmlRoot(ElementName = "EAE")]
    public class EAE
    {
        public string BatchID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string EAENumber { get; set; }
        public string ProtocolNumber { get; set; }
        public string ParticipantIdentifier { get; set; }
        public string PrimaryAE { get; set; }
        public string MedDRACodePT { get; set; }
        public string OnsetDate { get; set; }
        public string RelationshipToPrimaryAE { get; set; }
        public string SeverityGrade { get; set; }

        [XmlArray("ICHSeriousnessCriteria")]
        [XmlArrayItem("ICHSeriousnessCriterionDescription")]
        public List<ICHSeriousnessCriterion> SeriousnessCriteria { get; set; }
        public string DateOfDeath { get; set; }
    }
}
