using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using XDocBase.Web;
using System.Xml;
using System.Web;
using XDocBase.Web.CONTAINER;
using XDocBase.Web.CFG;



namespace XDocBase.WS
{
    [CLSCompliant(false)]
    public class HxXWPerson
    {
	    protected Session _session = null;
	    protected String _user = "";
        protected HxXWResponse _presp = null;
        protected HxXWResponse _uresp = null;
        protected XDocBase.Web.CONTAINER.Map _params = null;
        protected Boolean _isUnique = true;

        public HxXWResponse presp
        {
            get { return _presp; }
        }

        public HxXWResponse uresp
        {
            get { return _uresp; }
        }

        public Boolean isUnique
        {
            get { return _isUnique; }
        }

        public XDocBase.Web.CONTAINER.Map parameters
        {
            get { return _params; }
        }

        public string this[string Key]
        {
            get
            {
                return (string)_params[Key];
            }
        }

        public Object getParameter(String pname)
        {
            return _params[pname];
        }

        public Boolean testWRight(String r){
		    //test for rights    
		    //DW-$r-V-CompRep or DW-$r-V-VisRep = R
		    //DW-$r-V-InsRep W
		    int posI = (_params["rights"] as String).IndexOf("DW-"+r+"-V-InsRep"); 
		    return posI >= 0;
	    }
    	
	    public Boolean testRRight(String r){
		    //test for rights    
		    //DW-$r-V-CompRep or DW-$r-V-VisRep = R
		    //DW-$r-V-InsRep W
            int posC = (_params["rights"] as String).IndexOf("DW-"+r+"-V-CompRep");
            int posV = (_params["rights"] as String).IndexOf("DW-"+r+"-V-VisRep");
		    return  posC >= 0 || posV >= 0;
	    }

        public HxXWPerson(Session session, HxXWDatabase acl) : this(session, acl, false)
        {
        }

        public HxXWPerson(Session session, HxXWDatabase acl, bool forceLoad)
        {
		    String locuser = "";
            String matricola = "";
            String possibleDocwayIIdUnit = "";
            XmlNode node = null;

            _session = session;
		    if(_session != null){
                Object tmp = null;
			    if(!forceLoad)
				    tmp = _session["ACL_USER"];
    			
			    if(tmp == null){
                    acl.open();
                    //try check user by iIdUnit first
                    possibleDocwayIIdUnit = _session.AsString("DOCWAY_USER_IIDUNIT");
                    if (possibleDocwayIIdUnit == "")
                    {
                        //first try to take session user
                        locuser = _session.AsString("DOCWAY_USER_NAME");
                        if (locuser == "")
                        {
                            //try current auth user
                            locuser = Environment.UserName;
                            int idx = locuser.LastIndexOf('\\');
                            if (idx > -1)
                                locuser = locuser.Substring(idx + 1);
                        }
                        /*
                         * try to check up user for matricola containig it can be user$matricola
                         */
                        string[] v = locuser.Split(new Char[] { '$' });
                        string mq = "";
                        if(v.Length == 2){
						    locuser = v[0];
						    matricola = v[1]; //[/persona_interna/@matricola/]
						    mq = "and [/persona_interna/@matricola/]=\""+v[1]+"\"";
					    }else{
						    locuser = v[0];
						    matricola = "";
					    }

                        _presp = acl.loadFirst("[persint_loginname]=\"" + locuser + "\"" + mq, false);
                        //verify last result size
                        if (acl.lastResultSize > 1)
                        {
                            _isUnique = false;
                            return; //escape function
                        }
                        //take iIdUnit
					    node = _presp.xQuery("//Document");
					    if (node != null) {
						    possibleDocwayIIdUnit = node.Attributes["idIUnit"].Value;
					    }

                        //take persona interna
                        node = _presp.xQuery("//persona_interna");
                        if (node == null)
                        {
                            //we didnt find user in ACL so lets try default one
                            locuser = ConfigData.Open().application.AsString("xDocDefaultDocwayUser") ?? "";
                            if (locuser == "") throw new Exception("Missing user to log on!");
                            _presp = acl.loadFirst("[persint_loginname]=\"" + locuser + "\"", false);
                            //take iIdUnit
                            node = _presp.xQuery("//Document");
                            if (node != null)
                            {
                                possibleDocwayIIdUnit = node.Attributes["idIUnit"].Value;
                            }
                            //take persona interna
                            node = _presp.xQuery("//persona_interna");
                        }
                    }
                    else
                    {
                        //we have iIdUnit in session
                        ///first try to take session user
                        locuser = _session.AsString("DOCWAY_USER_NAME");
                        if (locuser == "")
                        {
                            //try current auth user
                            locuser = Environment.UserName;
                            int idx = locuser.LastIndexOf('\\');
                            if (idx > -1)
                                locuser = locuser.Substring(idx + 1);
                        }
					    //in case of matricola presence
					    string[] v = locuser.Split(new Char[] { '$' });
                        string mq = "";
                        if(v.Length == 2){
						    locuser = v[0];
						    matricola = v[1]; //[/persona_interna/@matricola/]
						    mq = "and [/persona_interna/@matricola/]=\""+v[1]+"\"";
					    }else{
						    locuser = v[0];
						    matricola = "";
					    }

					    //load record by iIdUnit
					    _presp = acl.loadDocument(Convert.ToInt32(possibleDocwayIIdUnit), false, true, false);
                        node = _presp.xQuery("//persona_interna");
                    }

                    //if we find user work on it
                    if (node != null)
                    {
                        _params = new XDocBase.Web.CONTAINER.Map();
                        _params["nome"] = node.Attributes["nome"].Value;
                        _params["cognome"] = node.Attributes["cognome"].Value;
                        _params["cod_uff"] = node.Attributes["cod_uff"].Value;
                        _params["AmmAoo"] = (String)node.Attributes["cod_amm"].Value + (String)node.Attributes["cod_aoo"].Value;
                        _params["matricola"] = node.Attributes["matricola"].Value;
                        matricola = node.Attributes["matricola"].Value;
                        _params["cgnno"] = _params["cognome"] + " " + _params["nome"];
                        _params["nocgn"] = _params["nome"] + " " + _params["cognome"];

                        //make list of inserable repertories
                        /* xpath for to take some repertory right
                         * //right[starts-with(@cod,'DW-DOCINTRANET') and ends-with(@cod,'-V-InsRep')]
                         */
                        //take all rights TRUE and put them in ',' separated string
                        //right[text()='TRUE' or text()='true']
                        XmlNodeList entries = _presp.xQueryExt("//right[text()='TRUE' or text()='true']");
                        String rights = "";

                        foreach (XmlNode n in entries)
                        {
                            rights += n.Attributes["cod"].Value + ",";
                        }
                        _params["rights"] = rights;

                        if (_params["cod_uff"].ToString().Length > 0)
                        {
                            _uresp = acl.loadFirst("[struint_coduff]=\"" + _params["cod_uff"] + "\"", false);
                            node = _uresp.xQuery("//struttura_interna/nome");
                            if (node != null)
                            {
                                if (node.LastChild != null)
                                    _params["uff_name"] = node.LastChild.Value;
                                else
                                    _params["uff_name"] = node.Value;
                            }
                        }
                        _session["ACL_USER"] = _params;
                        _session["DOCWAY_USER_NAME"] = locuser+"$"+matricola;
                        _session["DOCWAY_USER_IIDUNIT"] = possibleDocwayIIdUnit;
                    }
                    else
                    {
                        throw new Exception("Persona [" + locuser + "] non trovata in ACL!"); 
                    }
				    
			    }else{
				    _params = (XDocBase.Web.CONTAINER.Map)tmp;
			    }
		    }
	    }
    }
}