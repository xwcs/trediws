using System;
using System.IO;
using System.Text;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace XDocBase.Web.UI
{
     [CLSCompliant(false)]
    public class Xsl
    {
        protected XsltArgumentList xslArg = new XsltArgumentList();
        protected String _xslString = null;
        protected String _xslLink = null;

        public String this[String key]
        {
            get{ return (String)xslArg.GetParam(key, "");}
            set
            {
                if (xslArg.GetParam(key, "") != null) xslArg.RemoveParam(key, ""); 
                xslArg.AddParam(key, "", value);
            }
        }

        public String xslString
        {
            get { return _xslString; }
            set { _xslString = value; }
        }

        public String xslLink
        {
            get { return _xslLink; }
            set { _xslLink = value; }
        }

        public string transform(String src)
        {
            // Create a new transform object.
            XslCompiledTransform xslt = new XslCompiledTransform();
            try
            {
                if (xslLink != null && xslLink.Length > 0)
                {
                    // attempt to perform XSLT transform using transformLink.
                    xslt.Load(xslLink);
                }
                else
                {
                    if (xslString != null && xslString.Length > 0)
                    {
                        // attempt to perform XSLT transform using transform.
                        xslt.Load(new System.Xml.XmlTextReader(new System.IO.StringReader(xslString)));
                    }
                    else
                    {
                        // return the original content if there is nothing toprocess
                        return src;
                    }
                }

                // Read the current contents of the part into an XPathDocument.
                XPathDocument xml = new XPathDocument(new StringReader(src));

                // Create a stream on top of a string builder for output.
                StringWriter stmOut = new StringWriter(new StringBuilder());

                // Transform and return.
                xslt.Transform(xml, xslArg, stmOut);
                return stmOut.ToString();
            }
            catch (Exception E)
            {
                // return error message.
                throw E;
            }
        }
    }
}