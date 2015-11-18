using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace XDocBase.Web.CFG
{
    public class ConfigData : ConfigurationSection
    {
        private ConfigData() { }

        #region Public Methods

        ///<summary>Get this configuration set from the application's default config file</summary>
        public static ConfigData Open()
        {
            if (instance != null)
            {
                return instance;
            }
            //try registry value
            RegistryKey key = Registry.CurrentUser;
            RegistryKey my = key.OpenSubKey("Software\\3DInformatica\\TestEgaf");
            if (my != null)
            {
                //MessageBox.Show("R:" + (String)(my.GetValue("Config") ?? ""));
                return Open((String)(my.GetValue("Config") ?? ""));
            }
            //assembly
            System.Reflection.Assembly assy = System.Reflection.Assembly.GetEntryAssembly();
            if (assy != null)
            {
                //MessageBox.Show("A:" + (String)(assy.Location ?? ""));
                return Open(assy.Location ?? "");
            }
            //no path
            return Open("");
        }

        ///<summary>Get this configuration set from a specific config file</summary>
        public static ConfigData Open(string path)
        {
            if (instance == null)
            {
                if (path.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase))
                    spath = path.Remove(path.Length - 7);
                else
                    spath = path;
                Configuration config = ConfigurationManager.OpenExeConfiguration(spath);
                if (config.Sections["ConfigData"] == null)
                {
                    instance = new ConfigData();
                    config.Sections.Add("ConfigData", instance);
                    config.Save(ConfigurationSaveMode.Modified);
                }
                else
                    instance = (ConfigData)config.Sections["ConfigData"];
            }
            return instance;
        }

        ///<summary>Create a full copy of the current properties</summary>
        public ConfigData Copy()
        {
            ConfigData copy = new ConfigData();
            string xml = SerializeSection(this, "ConfigData", ConfigurationSaveMode.Full);
            System.Xml.XmlReader rdr = new System.Xml.XmlTextReader(new System.IO.StringReader(xml));
            copy.DeserializeSection(rdr);
            return copy;
        }

        ///<summary>Save the current property values to the config file</summary>
        public void Save()
        {
            // The Configuration has to be opened anew each time we want to update the file contents.
            // Otherwise, the update of other custom configuration sections will cause an exception
            // to occur when we try to save our modifications, stating that another app has modified
            // the file since we opened it.
            Configuration config = ConfigurationManager.OpenExeConfiguration(spath);
            ConfigData section = (ConfigData)config.Sections["ConfigData"];
            //
            // TODO: Add code to copy all properties from "this" to "sect"
            //
            section.application = this.application;
            section.db = this.db;
            //
            config.Save(ConfigurationSaveMode.Full);
        }

        ///<summary>Alternate means to Save the current property values to the config file</summary>
        public void AltSave()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(spath);
            ConfigData section = (ConfigData)config.Sections["ConfigData"];
            section.LockItem = true;
            //Copy the object for each top-level property:
            foreach (ConfigurationProperty prop in section.Properties)
            {
                string name = prop.Name;
                section.SetPropertyValue(section.Properties[name], this[name], false);
            }
            config.Save(ConfigurationSaveMode.Full);
        }

        #endregion Public Methods

        #region Properties

        public static ConfigData Default
        {
            get { return defaultInstance; }
        }

        // TODO: Add your custom properties and elements here.
        // All properties should have both get and set accessors to implement the Save function correctly
        
        [ConfigurationProperty("application")]
        public KeyValueMap application
        {
            get
            {
                return this["application"] as KeyValueMap;
            }
            set
            {
                this["application"] = value;
            }
        }

        [ConfigurationProperty("db")]
        public KeyValueMap db
        {
            get
            {
                return this["db"] as KeyValueMap;
            }
            set
            {
                this["db"] = value;
            }
        }

        #endregion Properties

        #region Fields
        private static string spath;
        private static ConfigData instance = null;
        private static readonly ConfigData defaultInstance = new ConfigData();
        #endregion Fields





        protected override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
        {
            StringWriter sWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            XmlTextWriter xWriter = new XmlTextWriter(sWriter);
            xWriter.Formatting = Formatting.Indented;
            xWriter.Indentation = 4;
            xWriter.IndentChar = ' ';
            this.SerializeToXmlElement(xWriter, name);
            xWriter.Flush();
            return sWriter.ToString();
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
            application.Serialize(writer, "application");
            db.Serialize(writer, "db");
            return true;
        }
    }
}
