using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace XDocBase.Web
{
    public class Session
    {
        protected Dictionary<string, Object> values = null; 
        /*
         * Session classs should hide session handling
         * it can handle clasic .NET session or it will use file system
         * session trough file sistem wil be implemented later
         */
        public Session()
        {
            values = new Dictionary<string, object>();    
        }

        public String AsString(String Key)
        {
            try
            {
                return (String)(values[Key] ?? "");
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public Object this[String key]
        {
            get
            {
                try
                {
                    return (Object)(values[key] ?? null);
                }
                catch (Exception e)
                {
                    return null;
                }
                
            }
            set
            {
                values[key] = value;
            }
        }
    }
}


/*

public class Session
    {
        private String path;
        
        public Session()
        {
            path = System.IO.Path.GetTempPath();
        }

        public String AsString(String Key)
        {
            return (String)(BinaryRage.DB<String>.Get(Key, path) ?? "");
        }

        public Object this[String key]
        {
            get
            {
                try
                {
                    return (Object)(BinaryRage.DB<Object>.Get(key, path) ?? null);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return null;
                }
                
            }
            set
            {
                BinaryRage.DB<Object>.Insert(key, value, path);
            }
        }

        
    }
*/