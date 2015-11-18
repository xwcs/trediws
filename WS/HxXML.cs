using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XDocBase.Web.UI;
using System.Collections;

namespace XDocBase.WS
{
     [CLSCompliant(false)]
    public class HxXML {
        protected Boolean bRefresh = false;
	    protected XmlDocument   _dom  = null;
	    protected string        _xml  = "";

        protected Xsl           xslt  = null;    	

        public string xml
        {
            get {
                    if (bRefresh)
                    {
                        _xml = _dom.OuterXml;
                        bRefresh = false;
                    }
                    return _xml; 
                }
            set {   _xml = value;
                    _dom = new XmlDocument();
                    _dom.LoadXml(_xml);
                    _dom.NodeChanged += new XmlNodeChangedEventHandler(DomChangedEvent);
                    _dom.NodeInserted += new XmlNodeChangedEventHandler(DomChangedEvent);
                    _dom.NodeRemoved += new XmlNodeChangedEventHandler(DomChangedEvent);
                }
        }

        public XmlDocument dom
        {
            get { return _dom; }
        }
	
	    public void setXslFile(string xslFile)
        {
            if(this.xslt == null)
                this.xslt = new Xsl();
            this.xslt.xslLink = xslFile;
        }
    	
        public void setXslString(string xslString)
        {
            if (this.xslt == null)
                this.xslt = new Xsl();
            this.xslt.xslString = xslString;
	    }


        public void setXslParams(Hashtable _params)
        {
            if (this.xslt != null)
            {
                foreach (DictionaryEntry de in _params)
                    this.xslt[de.Key.ToString()] = de.Value.ToString();
            }
        }

        public void setXslParam(String pKey, String pVal)
        {
            if (this.xslt != null)
                this.xslt[pKey] = pVal;
        }
    	
	    public string getXslTransformed()
        {
            if (this.xslt != null)
                return this.xslt.transform(dom.InnerXml);
		    return "";
	    }
    	
        public HxXML(string xmlString)
        {
            this.xml = xmlString;
        }

        public void Refresh()
        {
            bRefresh = true;
        }

        private void DomChangedEvent(Object source, XmlNodeChangedEventArgs args)
        {
            bRefresh = true;
        }

        public XmlNode xQuery(string query){
		    if (dom != null) {			
                XmlElement root = dom.DocumentElement;
                XmlNode node = root.SelectSingleNode(query);
                //register namespaces
			    /*foreach(nsarray as v){
				    xpath->registerNamespace(v['prefix'],v['uri']);
			    }*/
			    if (node != null)
				    return node;
    			
		    }
            return null;
	    }

        public XmlNodeList xQueryExt(string query)
        {
            if (dom != null)
            {
                XmlElement root = dom.DocumentElement;
                XmlNodeList nodes = root.SelectNodes(query);
                //register namespaces
                /*foreach(nsarray as v){
                    xpath->registerNamespace(v['prefix'],v['uri']);
                }*/
                if (nodes != null)
                    return nodes;

            }
            return null;
        }

        public string transform(string xslFile, Hashtable _params){
		    if(dom != null){
                setXslFile(xslFile);
                setXslParams(_params);
                return this.xslt.transform(xml);
		    }
            return "";
	    }
    }

}