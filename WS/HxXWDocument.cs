using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Web;
using System.Collections.Specialized;

namespace XDocBase.WS
{
     [CLSCompliant(false)]
    public class HxXWDocument
    {

	    protected int       _idIUnit;
	    protected HxXML     _doc;
	    protected bool      _isLoaded;
	    protected bool      _isNew;
	    protected HxXWDatabase _db;
	    protected String    _udName;
	    protected bool      _isLocked;


        public HxXML doc
        {
            get { return _doc; }
        }
        
        public XmlDocument dom
        {
            get { return _doc.dom; }
        }

        public String xml
        {
            get { return _doc.xml; }
        }

        public int idIUnit
        {
            get { return _idIUnit; }
        }

        public bool isLocked
        {
            get { return _isLocked; }
        }

        public HxXWDocument(String udName, HxXWDatabase db, int idIUnit) : this(udName, db, idIUnit, false)
        {
        }

        public HxXWDocument(String udName, HxXWDatabase db) : this(udName, db, -1, false)
        {
        }

	    public HxXWDocument(String udName, HxXWDatabase db, int idIUnit, bool bLock)
        {
	        _idIUnit    = -1;
	        _doc        = null;
	        _isLoaded   = false;
	        _isNew      = false;
	        _isLocked   = false;
            _udName     = udName;
		    _db         = db;

		    if(idIUnit > -1){
			    //try to load document
			    load(idIUnit, bLock);
		    }
	    }

	    virtual protected void decodeResponse(HxXWResponse r)
        {
		    //FIX: 08/05/2013 check for namespace of Response root element
		    XmlElement root = (XmlElement)r.xQuery("//Response");
		    String xmlNs = null;
		    if(root != null){
			    //we have so check eventual attribude xmlns:xw
			    xmlNs = root.GetAttribute("xmlns:xw");
		    }
            
            XmlNode entries = r.xQuery("//Document");
		    if (entries != null) {
                _idIUnit = Convert.ToInt32(entries.Attributes["idIUnit"].Value);
		    }else throw new Exception("Incorrect XML document!");
		    XmlElement node = (XmlElement) r.xQuery("//" + _udName);
		    if (node != null) {
			    XmlDocument d = new XmlDocument();
			    d.AppendChild(d.ImportNode(node, true));
			    _doc = new HxXML(d.OuterXml);
		    }else throw new Exception("Incorrect XML document!");
	    }

        public void load()
        {
            this.load(-1, false);
        }

        public void load(int idIUnit)
        {
            this.load(idIUnit, false);
        }

        public void load(int idIUnit, bool bLock)
        {
		    if(idIUnit > -1){
			    //try to load document
			    try{
				    HxXWResponse resp = _db.loadDocument(idIUnit, bLock, false, false);
				    decodeResponse(resp);
				    _isLoaded   = true;
				    _isNew      = false;
				    _isLocked   = bLock;
			    }catch{
				    _isLoaded   = false;
				    _isNew      = false;
				    _doc        = null;
				    _isLocked   = false;
			    }
		    }
	    }

	    public void create(String xml)
        {
		    _doc = new HxXML(xml);
		    _idIUnit    = -1;
		    _isLoaded   = false;
		    _isNew      = true;
	    }

	    public void save()
        {
		    if(!_isLoaded && !_isNew)
			    throw new Exception("Document is not loaded neither created!");
		    if(_isNew || _isLocked){
			    HxXWResponse resp = _db.saveDocument(_doc.xml, !_isNew, _idIUnit);
			    decodeResponse(resp);
			    _isNew      = false;
			    _isLoaded   = true;
			    _isLocked   = false;
		    }else{
			    throw new Exception("Only locked or new can be saved!");
		    }
	    }

	    public void  delete()
        {
		    if(!_isLoaded && !_isLocked)
			    throw new Exception("Document is not loaded with lock!");
		    _db.deleteDocument(_idIUnit);
		    _isNew      = false;
		    _isLoaded   = false;
		    _isLocked   = false;
	    }
    	
	    public void unlock()
        {
		    if(_idIUnit < 0 && _isLocked)
			    throw new Exception("Nothing to unlock!");
		    _db.unlockDocument(_idIUnit);
		    _isLocked = false;
	    }
    	
	    public void removeAttachment(String id)
        {
		    if(!_isLoaded && !_isLocked)
			    throw new Exception("Document is not loaded with lock!");
    		
		    //remove all <xw:file name="$id ...
		    XmlNode entry = _doc.xQuery("//xw:file[@name='" + id + "']");
		    if (entry != null) {
			    entry.ParentNode.RemoveChild(entry);
		    }
    		
		    entry = _doc.xQuery("//xw:file[@der_from='" + id + "']");
		    if (entry != null)
			    entry.ParentNode.RemoveChild(entry);

            doc.Refresh();
		    save();	
    			
		    _isNew      = false;
		    _isLoaded   = true;
		    _isLocked   = false;
	    }        
    }
}
