using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace Newtouch.CIS.Proxy.CMMPlatform.DTO.HLYYRequest
{
    [XmlRoot("root")]
    public class EngineRes
    {
        [XmlElement("message")] public Message Message { get; set; }

        [XmlElement("cfstate")] public string Cfstate { get; set; }

        public string cfh { get; set; }
    }

    public class Message
    {
        [XmlElement("infos")] public Infos Infos { get; set; }
    }

    public class Infos
    {
        [XmlElement("info")] public List<Info> InfoList { get; set; }
    }

    public class Info
    {
        [XmlElement("groupNo")] public string GroupNo { get; set; }

        [XmlElement("adminRoute")] public string AdminRoute { get; set; }

        [XmlElement("adminFrequency")] public string AdminFrequency { get; set; }

        [XmlElement("drugName")] public string DrugName { get; set; }

        [XmlElement("drugId")] public string DrugId { get; set; }

        [XmlElement("message")] public string MessageText { get; set; }

        [XmlElement("advice")] public string Advice { get; set; }

        [XmlElement("source")] public string Source { get; set; }

        [XmlElement("severity")] public string Severity { get; set; }

        [XmlElement("messageId")] public string MessageId { get; set; }

        [XmlElement("type")] public string Type { get; set; }
    }
}