using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace XDocBase.Web.CFG
{
    public class KeyValueMap : ConfigurationElementCollection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        public void Add(KeyValuePair pair){
            this[pair.Key] = pair;
        }

        public KeyValuePair this[int index]
        {
            get
            {
                return base.BaseGet(index) as KeyValuePair;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        new public KeyValuePair this[string Key]
        {
            get
            {
                return BaseGet(Key) as KeyValuePair;
            }
            set
            {
                if (base.BaseGet(Key) != null)
                {
                    base.BaseRemoveAt(base.BaseIndexOf(base.BaseGet(Key)));
                }
                this.BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new KeyValuePair();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((KeyValuePair)element).Key;
        }

        public String AsString(String Key)
        {
            return this[Key];
        }
        public Boolean AsBool(String Key)
        {
            return this[Key];
        }
        public KeyValueMap AsKeyValueMap(String Key)
        {
            return this[Key];
        }

        public bool Serialize(XmlWriter writer, string name)
        {
            return SerializeToXmlElement(writer, name);
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

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            if (writer == null)
                return false;
            foreach (KeyValuePair pair in this)
            {
                pair.Serialize(writer);
            }
            return true;
        }
    }
}
