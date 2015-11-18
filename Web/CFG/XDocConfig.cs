using System;
using System.Collections.Generic;
using System.Text;
using XDocBase.WS;
using System.Collections;
using XDocBase.Web.CONTAINER;
using System.Security.Cryptography;

namespace XDocBase.Web.CFG
{
     [CLSCompliant(false)]
    public class XDocConfig
    {
        protected Session session = null;
        protected KeyValueMap _app = null;
        protected static MD5 _internalHash = null;

        public KeyValueMap app
        {
            get {
                if (_app == null)
                {
                    _app = ConfigData.Open().application; 
                }
                return _app;
            }
        }

        
        public XDocConfig(Session session)
        {
            this.session = session;
        }

        public String adjustSubstValue(String subst)
        {
            String ret = subst;
            if (subst[0] == '@' && subst[1] == '@')
            {
                ret = app.AsString(subst.Substring(2, subst.Length - 2)) ?? "";   
            }
            return ret;
        }

        public static String normalizeNum(String n, int len){
            String b = "00000000000000000000000000" + n;
            return b.Substring(b.Length - len);
        }

        //dd/mm/yyyy -> yyyymmdd
        public static String  formatDateToXW(String date){
	        string [] v = date.Split(new Char[] {'/'});
            
	        return v[2] + normalizeNum(v[1], 2) + normalizeNum(v[0], 2);
        }
        //yyyymmdd -> dd/mm/yyyy
        public static String  formatDateFromXW(String date){
	        return date.Substring(6,2) + "/" + date.Substring(4, 2) + "/" + date.Substring(0, 4);	
        }

        public static String  dateRangeXW(String d1, String d2){
	        if(d1 != "" && d2 != ""){
		        return "{" + formatDateToXW(d1) + "|" + formatDateToXW(d2) + "}";
	        }else if(d1 != "" && d2 == ""){
		        return formatDateToXW(d1);
	        }else if(d1 == "" && d2 != ""){
		        return "{19000101|" + formatDateToXW(d2) + "}";
	        }else return "";
        }

        public static string GetMd5Hash(byte[] input)
        {
            if (_internalHash == null)
            {
                _internalHash = MD5.Create();
            }
            
            // Convert the input string to a byte array and compute the hash.
            byte[] data = _internalHash.ComputeHash(input);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string GetMd5Hash(string input)
        {
            return GetMd5Hash(Encoding.UTF8.GetBytes(input)); 
        }

        public static bool VerifyMd5Hash(string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
