using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XDocBase.WS
{
     [CLSCompliant(false)]
    public class HxXWResponse : HxXML
    {
       	protected string _pageIndex = "";
	    protected string _pageCount = "";
	    protected string _seleSize = "";
	    protected string _canNext = "";
	    protected string _canLast = "";
	    protected string _canFirst = "";
	    protected string _canPrev = "";
        protected string _idIUnit = "";
	    //protected xwns = array();
    	

        public string pageIndex
        {
            get { return _pageIndex; }
        }

        public string pageCount
        {
            get { return _pageCount; }
        }

        public string seleSize
        {
            get { return _seleSize; }
        }
    	
        public string canNext
        {
            get { return _canNext; }
        }

        public string canLast
        {
            get { return _canLast; }
        }

        public string canFirst
        {
            get { return _canFirst; }
        }

        public string canPrev
        {
            get { return _canPrev; }
        }

        public string idIUint
        {
            get { return _idIUnit; }
        }

        /*
	     * Extends <Response ..> with navigation attributes
	     * selId - selection id of apartenance
	     * followDsk - desktop to show 
	     * dbid - id of DB connection
	     */
	    public void addNavigationParams(String selId, String followDsk, String dbid){
		    if(this.dom != null){
			    XmlElement root = dom.DocumentElement;
                XmlNode node = root.SelectSingleNode("//Response");
			    if (node != null) {
                    XmlAttribute att = this.dom.CreateAttribute("selId");
                    att.Value = selId;
				    node.Attributes.Append(att);

                    att = this.dom.CreateAttribute("followDsk");
                    att.Value = followDsk;
				    node.Attributes.Append(att);

                    att = this.dom.CreateAttribute("dbid");
                    att.Value = dbid;
				    node.Attributes.Append(att);
			    }			
		    }
	    }

        public HxXWResponse(string xmlString) : base(xmlString)
        {
		    if (this.dom != null ) {
			    //xwuri = this->dom->documentElement->lookupnamespaceURI('xw');
			    //this->xwns[] = array('prefix' => 'xw', 'uri' => xwuri);
                XmlElement root = dom.DocumentElement;
                XmlNode node = root.SelectSingleNode("//Response");

                if (node != null) {
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if(node.Attributes.Item(i).Name.CompareTo("pageIndex") == 0)
                            _pageIndex = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("pageCount") == 0)
                            _pageCount = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("seleSize") == 0)
                            _seleSize = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("canNext") == 0)
                            _canNext = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("canLast") == 0)
                            _canLast = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("canPrev") == 0)
                            _canPrev = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("canFirst") == 0)
                            _canFirst = node.Attributes.Item(i).Value;
                    } 
			    }
                node = root.SelectSingleNode("//Document");
                if (node != null) 
                    _idIUnit = node.Attributes["idIUnit"].Value;
		    }
	    }

    }
}
