using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XDocBase.WS
{
     [CLSCompliant(false)]
    public class HxXWSelection : HxXML
    {
    	protected String _id = "";
    	protected String _name = "";
    	protected int _size = 0;

        public string id
        {
            get { return _id; }
        }
	
        public string name
        {
            get { return _name; }
        }

        public int size
        {
            get { return _size; }
        }

        public HxXWSelection(string xmlString) : base(xmlString)
        {
		    if (this.dom != null ) {
			    //xwuri = this->dom->documentElement->lookupnamespaceURI('xw');
			    //this->xwns[] = array('prefix' => 'xw', 'uri' => xwuri);
                XmlElement root = dom.DocumentElement;
                XmlNode node = root.SelectSingleNode("//Selection[@active=\"true\" and @size>0]");

                if (node != null) {
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if(node.Attributes.Item(i).Name.CompareTo("id") == 0)
                            _id = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("name") == 0)
                            _name = node.Attributes.Item(i).Value;
                        if (node.Attributes.Item(i).Name.CompareTo("size") == 0)
                            _size = Convert.ToInt32(node.Attributes.Item(i).Value);
                    } 
			    }
		    }
	    }
    }
}