using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XDocBase.Web.CONTAINER
{
    [Serializable]
    public class Map : Dictionary<String, Object>
    {
        public Map(Dictionary<String, Object> src) : base(src){}
        public Map() : base(){}
        public Map(SerializationInfo info, StreamingContext context) : base(info, context){} 

        public String AsString(String Key){
            if (this.ContainsKey(Key))
            {
                return (String)this[Key];
            }
            else
            {
                return "";
            }
        }
        public Boolean AsBool(String Key)
        {
            try
            {
                Boolean btmp = (Boolean)this[Key];
                return btmp;
            }
            catch
            {
                String v = this[Key] as String;
                if(v != null)
                    return v == "true" || v == "TRUE" || v == "True" || v == "yes" || v == "Yes" || v == "YES" || v == "Si" || v == "si" || v == "SI";
            }
            return false;
        }
        public Map AsMap(String Key)
        {
            if (this.ContainsKey(Key))
            {
                return (Map)this[Key];
            }
            else
            {
                return new Map();
            }            
        }                
    }
}