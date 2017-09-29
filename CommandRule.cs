//+---------------------------------------------------------------------------
//
//  File: CommandRule.cs
//
//  Purpose:
//    1) Declare CommandRule XML objects
//
//  History:
//  09-27-17 Kam-Tung Cheng: Created the file.
//
//----------------------------------------------------------------------------

using System.Xml;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DocuSign.DressChecklist.Model
{
    /// <summary>
    /// RequiredGroup
    /// </summary>
    public class RequiredGroup
    {
        /// <summary>
        /// The required item list.
        /// </summary>
        [XmlElement("requiredItem")]
        public List<int> RequiredList { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Response type
        /// </summary>
        [XmlAttribute("type")]
        public TemperatureType Type { get; set; }

        /// <summary>
        /// Response Message
        /// </summary>
        [XmlElement("message")]
        public string Message { get; set; }

        /// <summary>
        /// Response Error
        /// </summary>
        [XmlElement("error")]
        public bool Error { get; set; }

        /// <summary>
        /// The Required Items group
        /// </summary>
        [XmlElement("requiredItems")]
        public RequiredGroup RequiredGroup { get; set; }
    }

    /// <summary>
    /// ResponseGroup
    /// </summary>
    public class ResponseGroup
    {
        /// <summary>
        /// The response list.
        /// </summary>
        [XmlElement("response")]
        public List<Response> ResponseList { get; set; }
    }

    /// <summary>
    /// Command Rule
    /// </summary>
    public class CommandRule
    {
        /// <summary>
        /// Id
        /// </summary>
        [XmlAttribute("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [XmlAttribute("description")]
        public string Description { get; set; }

        /// <summary>
        /// The responses group
        /// </summary>
        [XmlElement("responses")]
        public ResponseGroup ResponseGroup { get; set; }
    }

    /// <summary>
    /// Command Rules
    /// </summary>
    [XmlRoot("commandRules")]
    public class CommandRules
    {
        /// <summary>
        /// Command rule list.
        /// </summary>
        [XmlElement("commandRule")]
        public List<CommandRule> CommandRuleList { get; set; }
    }
}
