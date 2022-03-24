using System;
using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class Recommendation
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Place { get; set; }
        [DataMember]
        public DateTime ArrangmentDate { get; set; }
        [DataMember]
        public string Details { get; set; }

    }
}
