using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Xml;

namespace XDocBase.Web.CFG
{
    public class KeyValuePair : ConfigurationElement
    {
        public KeyValuePair(String key, String value)
        {
            Key = key;
            Value = value;
        }

        public KeyValuePair()
        {

        }

        public KeyValuePair(XmlNode n, System.Xml.XmlReader reader)
        {
            parseNode(n, reader);
        }

        public KeyValuePair(string key)
        {
            Key = key;
        }

        [ConfigurationProperty("key",
            DefaultValue = "someKey",
            IsRequired = true,
            IsKey = true)]
        public string Key
        {
            get
            {
                return (string)this["key"];
            }
            set
            {
                this["key"] = value;
            }
        }

        [ConfigurationProperty("value")]
        public object Value
        {
            get
            {
                return this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

        public string AsString
        {
            get
            {
                return (string)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }
        public static implicit operator string(KeyValuePair p)
        {
            if (p == null) return "";
            return p.AsString;
        }

        public bool AsBool
        {
            get
            {
                string v = (string)this["value"]; 
                return v == "true" || v == "TRUE" || v == "True" || v == "yes" || v == "Yes" || v == "YES" || v == "Si" || v == "si" || v == "SI";
            }
            set
            {
                this["value"] = value ? "true" : "false";
            }
        }
        public static implicit operator bool(KeyValuePair p)
        {
            return p.AsBool;
        }

        public Int32 AsInt32
        {
            get
            {
               return Convert.ToInt32(this["value"]);
            }
            set
            {
                this["value"] = value;
            }
        }

        public KeyValueMap Values
        {
            get
            {
                if (Value is KeyValueMap)
                {

                    return Value as KeyValueMap;
                }
                else
                {
                    return null;
                }
            }
        }

        public static implicit operator KeyValueMap(KeyValuePair p)
        {
            return p.Values;
        }

        public bool Serialize(XmlWriter writer)
        {
            return SerializeToXmlElement(writer, "add");
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            if (writer == null)
                return false;
            writer.WriteStartElement(elementName);
            bool success = SerializeElement(writer, false);
            writer.WriteEndElement();
            return success;

        }

        protected override bool SerializeElement(
        System.Xml.XmlWriter writer,
        bool serializeCollectionKey)
            {
                if (writer != null)
                {
                    writer.WriteAttributeString("key", Key);
                    writer.WriteAttributeString("value", (String)Value);
                    return true;
                }

                return false;
            }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            string x = reader.ReadOuterXml();
            XmlDocument _dom = new XmlDocument();
            _dom.PreserveWhitespace = false;
            _dom.LoadXml(x);
            parseNode(_dom.DocumentElement, reader);
        }
        
        protected void parseNode(XmlNode n, System.Xml.XmlReader reader)
        {
            if (n.Attributes["key"] == null)
            {
                throw new ConfigurationErrorsException("Missing [\"key\"] attribute!", reader);
            }
            else
            {
                Key = n.Attributes["key"].Value;
            }

            //here decide what to do , there can by Map so make children
            if (n.Attributes["value"] != null)
            {
                Value = n.Attributes["value"].Value;
            }
            else
            {
                if (n.HasChildNodes)
                {
                    Boolean isFirst = true;
                    KeyValueMap pc = new KeyValueMap();
                    Boolean cdata = false;
                    foreach (XmlNode c in n.ChildNodes)
                    {
                        if (c.NodeType == XmlNodeType.Element)
                        {
                            isFirst = false; //after first regular node we set first on false
                            KeyValuePair kp = new KeyValuePair(c, reader);
                            pc[kp.Key] = kp;
                        }else if(isFirst && (c.NodeType == XmlNodeType.Text || c.NodeType == XmlNodeType.CDATA)){
                            Value = c.Value;
                            cdata = true;
                            break; //we have value so break for
                        }
                    }
                    if (!cdata) Value = pc;
                }
                else
                {
                    throw new Exception("Config value with key = \"" + Key + "\" has no value!");
                }
            }
        }
    }
}