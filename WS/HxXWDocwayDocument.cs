using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Web;
using System.Collections.Specialized;

namespace XDocBase.WS
{
     [CLSCompliant(false)]
    public class HxXWDocwayDocument : HxXWDocument
    {

        protected HxXWPerson _person;
        protected String _nrecord;

        public String nrecord
        {
            get
            {
                if (!_isLoaded)
                    throw new Exception("Document is not loaded!");
                return _nrecord;
            }
        }

        public HxXWDocwayDocument(HxXWDatabase db, HxXWPerson pe)
            : this(db, pe, "", false)
        {
        }


        public HxXWDocwayDocument(HxXWDatabase db, HxXWPerson pe, String nrecord)
            : this(db, pe, nrecord, false)
        {
        }

        public HxXWDocwayDocument(HxXWDatabase db, HxXWPerson pe, String nrecord, bool bLock) : base("doc", db, -1, bLock)
        {
            _person = pe;
            _nrecord = nrecord;
            _udName = "doc";
            _db = db;

            //look for document in case of wanted $nrecord
            if (nrecord.Length > 0)
            {
                load(nrecord, bLock);
            }
            else
            {
                _idIUnit = -1;
            }
        }

        new public void load()
        {
            load("", false);
        }

        public void load(String nrecord)
        {
            load(nrecord, false);
        }


        public void load(String nrecord, bool bLock)
        {
            HxXWSelection resp = _db.executeQuery("[/doc/@nrecord]=\"" + nrecord + "\"");//_db.executeQueryWithFTP("[/doc/@nrecord]=\"" + nrecord + "\"");
            XmlNode entries = resp.xQuery("//Response/Item");
            if (entries != null)
                _idIUnit = Convert.ToInt16(entries.Attributes["idIUnit"]);
            if (_idIUnit > -1)
                base.load(_idIUnit, bLock);
        }

        override protected void decodeResponse(HxXWResponse r)
        {
            base.decodeResponse(r);
            _nrecord = _doc.dom.DocumentElement.Attributes["nrecord"].Value;
        }

        public void create()
        {
            this.create("Ogetto", "", "");
        }

        new public void create(String sObject)
        {
            this.create(sObject, "", "");
        }

        public void create(String sObject, String repertorio)
        {
            this.create(sObject, repertorio, "");
        }

        public void create(String sObject, String repertorio, String extra)
        {
            String d = "";
            d = "<doc tipo=\"varie\" nrecord=\".\" data_prot=\"" + DateTime.Now.ToString("yyyyMMdd") + "\">";
            d += repertorio;
            d += "	<autore>" + _person.parameters["nocgn"] + " (" + _person.parameters["uff_name"] + ")</autore>";
            d += "	<oggetto>" + sObject + "</oggetto>";
            d += "	<classif cod=\"00/00\"/>";
            d += "	<rif_interni>";
            d += "		<rif_interno  diritto=\"RPA\" nome_persona=\"" + _person.parameters["nocgn"] + "\" nome_uff=\"" + _person.parameters["uff_name"] + "\"/>";
            d += "	</rif_interni>";
            d += extra;
            d += "</doc>";
            ((HxXWDocument)this).create(d);
        }

        public void cancel()
        {
            if (!_isLoaded && !_isLocked)
                throw new Exception("Document is not loaded with lock!");
            HxXWResponse resp = _db.cancelDocument(_idIUnit);
            _isNew = false;
            _isLoaded = true;
            _isLocked = false;
        }

        public void annulla()
        {
            if (!_isLoaded && !_isLocked)
                throw new Exception("Document is not loaded with lock!");
            //remove all <xw:file name="$id ...
            XmlNode entries = _doc.xQuery("/doc");
            if (entries != null)
            {
                XmlNode x = entries.ChildNodes[0];
                x.Attributes["annullato"].Value = "si";
            }
            _doc.Refresh();
            save();
            _isNew = false;
            _isLoaded = true;
            _isLocked = false;

        }

        public void makeBozza()
        {
            if (!_isNew)
                throw new Exception("Document is not new!");
            //remove all <xw:file name="$id ...
            XmlNode entries = _doc.xQuery("/doc");
            if (entries != null)
            {
                XmlElement x = (XmlElement)entries;
                x.SetAttribute("bozza","si");
            }
            _doc.Refresh();
        }

        public String getAttachmentIdForFile(String file)
        {
            ////*[name()='xw:file' and @title='XWDOC-00000484-Documento1.doc' and not (descendant::*[name()='xw:file'])]
            XmlElement node = (XmlElement)_doc.xQuery("//*[name()='xw:file' and @title='" + file + "' and not (descendant::*[name()='xw:file'])]");
            if (node != null)
            {
                return node.GetAttribute("name");
            }
            return "";
        }

        public String oggetto
        {
            get
            {
                XmlElement node = (XmlElement)_doc.xQuery("//oggetto");
                return node.InnerText;
            }

            set {
                XmlElement node = (XmlElement)_doc.xQuery("//oggetto");
                node.InnerText = value;
                _doc.Refresh();
            }
            
        }

        public String getRepertorio(String code, String desc)
        {
            String d = "";
            d += "<repertorio numero = \"" + code + "^" + _person.parameters["AmmAoo"] + "-" + DateTime.Now.ToString("YY") + ".\" cod = \"" + code + "\" >";
            d += desc;
            d += "</repertorio>";
            return d;
        }
    }
}
