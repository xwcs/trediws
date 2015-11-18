using XDocBase.Web;
using System.Configuration;
using System;
using System.Net;
using System.Xml;
using XDocBase.Web.CFG;
using XDocBase.Web.UI;
namespace XDocBase.WS
{
     [CLSCompliant(false)]
    public class HxXWDatabase
    {
        protected Session session;
        protected eXtraWayService ewws;
        protected string ID = "DOC1";
        protected bool _isOpen = false;
        protected XDocConfig config = null;
        protected KeyValueMap appSettings = null;

       

        protected string _host = "localhost";
        protected string _port = "4859";
        protected string _user = "admin";
        protected string _password = "reader";
        protected string _db = "xdocwaydoc";
        protected string _encoding = "UTF-8";
        protected int    _indexPageSize = 20;
        protected int    _titlesPageSize = 20;
        protected string _wsdl_url = "";
        protected string _orderby = "";
        protected Boolean _isDataDB = true;

        protected int xDocUnSucessfullTryesCount = 3;

        protected Xsl _xsl = null;

        protected int _lastResultSize = 0;

        public int lastResultSize
        {
            get { return _lastResultSize; }
            set { _lastResultSize = value; }
        }
        
        public string orderby
        {
            get { return _orderby; }
            set { _orderby = value; }
        }

        public string host
        {
            get { return _host; }
            set { _host = value; }
        }

        public string port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string user
        {
            get { return _user; }
            set { _user = value; }
        }

        public string password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string db
        {
            get { return _db; } 
            set { _db = value; }
        }

        public string encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public int indexPageSize
        {
            get { return _indexPageSize; }
            set { _indexPageSize = value; }
        }

        public int titlesPageSize
        {
            get { return _titlesPageSize; }
            set { _titlesPageSize = value; }
        }

        public string wsdl_url
        {
            get { return _wsdl_url; }
            set { _wsdl_url = value; }
        }

        public Xsl xsl
        {
            get { return _xsl; }
            set { _xsl = value; }
        }

        protected void initConfig(){
            config = new XDocConfig(session);
            appSettings = config.app;
        }

        public HxXWDatabase(Session session, String _ID, String _dbHost, String _dbPort, String _dbUsr, String _dbPwd, String _dbName, String _dbEncoding, int _titlesPageSize, int _indexPageSize)
        {
            this.session = session;
            initConfig();
            
            this.ID         = _ID;
            this.host       = _dbHost;
            this.port       = _dbPort;
            this.user       = _dbUsr;
            this.password   = _dbPwd;
            this.db         = _dbName;
            this.encoding   = _dbEncoding;
            this.titlesPageSize = _titlesPageSize;
            this.indexPageSize  = _indexPageSize;
            this.xDocUnSucessfullTryesCount = Convert.ToInt32(config.app["xDocUnSucessfullTryesCount"] ?? "3");
            open();
        }

        protected void init()
        {
            
            //take user form configured params
            String locuser;
            String matricola = "";

            locuser = session.AsString("DOCWAY_USER_NAME");

            if (locuser == "")
            {
                //take login for ACL cause we go retrieve users, not documents
                locuser = config.app.AsString("xDocAclReaderLogin") ?? "";
                if (locuser == "") throw new Exception("Missing ACL reader account in web.config! [xDocAclReaderLogin]");
            }
            else
            {
                //take actual docway user
                string[] v = locuser.Split(new Char[] { '$' });
                if (v.Length == 2)
                {
                    locuser = v[0];
                    matricola = v[1]; //[/persona_interna/@matricola/]
                }
                else
                {
                    locuser = v[0];
                    matricola = "";
                }
            }
            if (locuser == "")
            {
                //there is somthing wrong so take user from config [user can be in config in form @@appSettingsKey]
                locuser = config.adjustSubstValue(this.user);
            }

            if (matricola != "")
            {
                ewws.init(host, port, locuser, password, matricola, db, "UTF-8", titlesPageSize, indexPageSize);
            }
            else
            {
                ewws.init(host, port, locuser, password, db, "UTF-8", titlesPageSize, indexPageSize);
            }
           
            getCookies();
            
        }

        private void getCookies(){
                //take JSESSIONID
            CookieCollection cookies = ewws.CookieContainer.GetCookies(new Uri(ewws.Url));
            session[ID] = (String)(cookies["JSESSIONID"].Value);
            System.Windows.Forms.Application.DoEvents();
        }

        public void open()
        {
            //if is open just return
            if (_isOpen) return;

            CookieContainer cc = null;
            wsdl_url = appSettings.AsString("xDocWSGate") ?? "";
            if (wsdl_url == "")
            {
                throw new Exception("Missing xDocWSGate url in web.config!");
            }
            //create service if not exists
            //ewws = new eXtraWayService(_wsdl_url);
            ewws = new eXtraWayService(wsdl_url);
            //do this for sure!!
            //ewws.Url = wsdl_url;
            //authenticate 
            ewws.Credentials = new System.Net.NetworkCredential(appSettings.AsString("httpUser") ?? "admin", appSettings.AsString("httpPwd") ?? "admin");
            ewws.PreAuthenticate = true;

            //some work with cookies
            if (cc == null) cc = new System.Net.CookieContainer();
            ewws.CookieContainer = cc;
            
            if (ewws == null)
            {
                throw new Exception("Can't create webservice!");
            }

            /*
             * 1: test existance of 'xwssid' in session 
             *      a: if exists means we have connection open
             *      b: not exists we have to call init
             */
            String xwssid = (String)session[ID];
            if (xwssid != null)
            {
                //make cookie in webservice so it will do eventual call for correct connection
                cc = new CookieContainer(1);
                Uri uri = new Uri(ewws.Url);
                cc.Add(new Cookie("JSESSIONID", xwssid, "/", uri.Host));
                ewws.CookieContainer = cc;
            }
            else
            {
                //call init
                init();
                
            }
            _isOpen = true;
        }

        protected HxXML getWSDesktop(String rule, Boolean custom)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0){
                try{
                    //mark try
                    --tries;
                    //get desktop
                    if (custom){
                        s = ewws.getCustomDesktop(rule);
                    }else{
                        s = ewws.getDesktop(rule);
                    }                    
                    done = true;
                }catch (Exception e){
                    if (e.Message.Contains("Connessione inattiva!")){
                        init();
                    }else{
                        throw e;
                    }                    
                }
            }
            
            if(!done || s == ""){
                throw new Exception("Can't load desktop!");
            }
            return new HxXML(s);            
        }

        public string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        public HxXML getDesktop(bool custom, String desktopRule, String followDsk)
        {
            return getDesktop(custom, desktopRule, followDsk, false);
        }
        
        // desktopRule have to be HxXML !!!
	    public HxXML getDesktop(bool custom, String desktopRule, String followDsk, bool forceLoad)
        {
            HxXML d = null;
            
		    try {
			    String dskid = GetMD5Hash("CustomDsk" + this.ID + followDsk + desktopRule);
			    if(!forceLoad){
				    //first look for cached
				    String cdsk = (String)(this.session["dskid"]);
				    if(cdsk == null){
                        HxXML src = getWSDesktop(desktopRule, custom);
                        XmlNode srcNode = src.xQuery("/Scrivania");
					    d = new HxXML("<dsk dbid=\"" + this.ID + "\" follow=\"" + followDsk + "\"/>");
					    srcNode = d.dom.ImportNode(srcNode, true);
					    d.dom.DocumentElement.AppendChild(srcNode);
				    }else{
					    d = new HxXML(cdsk);
				    }
			    }else{
                    HxXML src = getWSDesktop(desktopRule, custom);
				    XmlNode srcNode = src.xQuery("/Scrivania");
				    d = new HxXML("<dsk dbid=\"" + this.ID + "\" follow=\"" + followDsk + "\"/>");
                    d.dom.DocumentElement.AppendChild(d.dom.ImportNode(srcNode, true));
				    //srcNode = d.dom.ImportNode(srcNode, true);
				    //d.dom.DocumentElement.AppendChild(srcNode);
				    //cache desktop
				    this.session["dskid"] =  d.xml;
			    }
			    //finish
			    return d;
		    }catch (Exception e) {
                throw new Exception("HxXWDatabase exception : " + e.Message);
		    }
	    }


        protected String getTitleRule(String tr)
        {
            String t;
            if (tr != null && tr != "") return tr;
            else
            {
                //try to take config rule
                t = appSettings["xDocTitleRule"];
                return t != null ? t : "";
            }
        }

        protected void setWSCurrentSet(String selid)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;

            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    ewws.setCurrentSet(selid, titlesPageSize);
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done)
            {
                throw new Exception("Can't set current set!");
            }
        }

        public void setCurrentSet(String selid)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");

            //set currentset and set tittles page size
            setWSCurrentSet(selid);
        }

        /*
        public void saveCurrentPageNumber(String resp)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(resp);
            XmlElement root = xml.DocumentElement;
            String page = root.Attributes["pageIndex"].Value;
            //save page to session
            session["currentPage"] = page;
        }*/

        /*
         * What:
         * 0-first
         * 1-prev
         * 2-next
         * 3-last
         * 4-dirrect page
         */
        protected HxXWResponse getWSTitlePage(String titleRule, int What)
        {
            return getWSTitlePage(titleRule, What, -1);
        }
        protected HxXWResponse getWSTitlePage(String titleRule, int What, int currentPage)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    //get title page
                    switch (What)
                    {
                        case 0:
                            s = ewws.firstTitlePage(getTitleRule(titleRule));
                            break;
                        case 1:
                            s = ewws.prevTitlePage(getTitleRule(titleRule));
                            break;
                        case 2:
                            s = ewws.nextTitlePage(getTitleRule(titleRule));
                            break;
                        case 3:
                            s = ewws.lastTitlePage(getTitleRule(titleRule));
                            break;
                        case 4:
                        default:
                            s = ewws.titlePage(currentPage, getTitleRule(titleRule));
                            break;

                    }
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't load titles!");
            }
            HxXWResponse ret = new HxXWResponse(s); 
            session["currentPage"] = ret.pageIndex;
            return ret;
        }

        public HxXWResponse getFirstTitlePage(String titleRule)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return getWSTitlePage(titleRule, 0);
        }

        public HxXWResponse getPrevTitlePage(String titleRule)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return getWSTitlePage(titleRule, 1);
        }

        public HxXWResponse getNextTitlePage(String titleRule)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return getWSTitlePage(titleRule, 2);
        }

        public HxXWResponse getLastTitlePage(String titleRule)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return getWSTitlePage(titleRule, 3);
        }

        public HxXWResponse getTitlePage(int currentPage, String titleRule)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return getWSTitlePage(titleRule, 4, currentPage);
        }

        protected String sortWSCurrentSet(String cmd)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.sortCurrentSet(cmd, false);
                    getCookies();
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't execute query titles!");
            }
            return s;
        }

        protected String executeWSQuery(String cmd, String __orderBy)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.executeQuery(cmd, "", __orderBy, false);
                    getCookies();
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't execute query titles!");
            }
            return s;
        }

        public HxXWSelection sortCurrentSet(String cmd)
        {
            String resp = "";

            try
            {
                resp = sortWSCurrentSet(cmd);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Esito della ricerca nullo!"))
                {
                    throw e;
                }
            }
            HxXWSelection ret = new HxXWSelection(resp);
            _lastResultSize = ret.size;
            return ret;
        }

        public HxXWSelection executeQuery(String cmd, String __orderBy)
        {
            //String ret = "<Response />";
            String resp = "";

            try
            {
                resp = executeWSQuery(cmd, __orderBy);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Esito della ricerca nullo!"))
                {
                    throw e;
                }
            }
            HxXWSelection ret = new HxXWSelection(resp);
            _lastResultSize = ret.size;
            return ret;
        }


        public HxXWSelection executeQuery(String cmd)
        {
            return executeQuery(cmd, "");
        }

        public HxXWResponse queryExt(String cmd, String titleRule, String orderBy, String followDsk)
        {
            String ret = "<Response />";
            HxXWSelection resp = null;

            try
            {
                resp = executeQuery(cmd, orderBy);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Esito della ricerca nullo!"))
                {
                    throw e;
                }
            }

            if (resp.id.Length > 0) //(resp.Length > 0)
            {
                if (resp.size > 0)
                {
                    HxXWResponse r = getFirstTitlePage(titleRule);
                    r.addNavigationParams(resp.id, followDsk, this.ID);
                    return r;
                }
            }

            return new HxXWResponse(ret);
        }

        public HxXWResponse query(String cmd, String titleRule, String orderBy)
        {
            String ret = "<Response />";
            HxXWSelection resp = null;

            try
            {
                resp = executeQuery(cmd, orderBy);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Esito della ricerca nullo!"))
                {
                    throw e;
                }
            }

            if(resp.id.Length > 0) //(resp.Length > 0)
            {
                if (resp.size > 0)
                {
                    //setActiveSelectionId(resp.id); not necessary after query we have current set
                    return(getFirstTitlePage(titleRule));
                }
            }

            return new HxXWResponse(ret);
        }

        public HxXWResponse query(String cmd, String titleRule)
        {
            return query(cmd, titleRule, "");
        }

        //using in acl
        public HxXWResponse loadFirst(String cmd, bool highlight)
        {
            String ret = "<Response />";
            HxXWSelection resp = null;

            try
            {
                resp = executeQuery(cmd);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Esito della ricerca nullo!"))
                {
                    throw e;
                }
            }

            if (resp.xml.Length > 0)
            {
                //parse result and look for one marked like active
                /*
				  <ArchiveSelection name="xdocwaydoc" size="8026">
					  <Selection active="true" id="3se96035caae325d01" name="A" size="0">
					    <Query>[xml,/doc/@tipo]="non protocollato"</Query>
					  </Selection>
					</ArchiveSelection>
				 */

                if (resp.size > 0)
                {
                    //setActiveSelectionId(resp.id); not neccessary
                    return(loadFirstDocument(highlight));
                }
            }

            return new HxXWResponse(ret);
        }
        
        //load document handling

        protected String loadWSDocument(int id, bool bLock, bool outofset, bool highlight)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.loadDocument(id, bLock, outofset, false, highlight);
                    getCookies();
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't load document!");
            }
            return s;
        }
        public HxXWResponse loadDocument(int id, bool bLock, bool outofset,  bool highlight)
        {
            if (!_isOpen) throw new Exception("Can't take document on closed WS");
            return new HxXWResponse(loadWSDocument(id, bLock, outofset, highlight));
        }

        /*
         * What:
         * 0-first
         * 1-prev
         * 2-next
         * 3-last
         */
        protected String loadWSDocument(bool highlight, int What)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    //get title page
                    switch (What)
                    {
                        case 0:
                            s = ewws.loadFirstDocument(false, highlight, false);
                            break;
                        case 1:
                            s = ewws.loadPrevDocument(false, highlight, false);
                            break;
                        case 2:
                            s = ewws.loadNextDocument(false, highlight, false);
                            break;
                        case 3:
                        default:
                            s = ewws.loadLastDocument(false, highlight, false);
                            break;
                    }
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't load document!");
            }
            return s;
        }

        public HxXWResponse loadFirstDocument(bool highlight)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return new HxXWResponse(loadWSDocument(highlight, 0));
        }

        public HxXWResponse loadPrevDocument(bool highlight)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return new HxXWResponse(loadWSDocument(highlight, 1));
        }

        public HxXWResponse loadNextDocument(bool highlight)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return new HxXWResponse(loadWSDocument(highlight, 2));
        }

        public HxXWResponse loadLastDocument(bool highlight)
        {
            if (!_isOpen) throw new Exception("Can't take titles on closed WS");
            return new HxXWResponse(loadWSDocument(highlight, 3));
        }

        public byte[] checkOutContentFile(int idIUnit, String fileId, bool bLock)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            byte[] s = null;
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                     s = ewws.checkOutContentFile(idIUnit, fileId, bLock);
                    getCookies();
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == null))
            {
                throw new Exception("Can't check out document!");
            }
            return s;
        }

        public HxXWResponse checkInContentFile(int idIUnit, String fileName, String fileId, byte[] fileContent)
        {
            return checkInContentFile(idIUnit, fileName, fileId, fileContent, false, false);
        }

        public HxXWResponse checkInContentFile(int idIUnit, String fileName, String fileId, byte[] fileContent, bool pdfConversion)
        {
            return checkInContentFile(idIUnit, fileName, fileId, fileContent, pdfConversion, false);
        }

        public HxXWResponse checkInContentFile(int idIUnit, String fileName, String fileId, byte[] fileContent, bool pdfConversion, bool sendEMail)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.checkInContentFile(idIUnit, fileName, fileId, fileContent, pdfConversion, sendEMail);
                    getCookies();
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't check in document!");
            }
            return new HxXWResponse(s);
        }

        public HxXWResponse cancelDocument(int idIUnit)
        {
            return cancelDocument(idIUnit, "auto dismiss from tiv");
        }


	    public HxXWResponse cancelDocument(int idIUnit, String reason)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.cancelDocument(idIUnit, reason);
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Cancel exception!");
            }
            return new HxXWResponse(s);
	    }

        public HxXWResponse grantRight(int idIUnit, String rightType, String userName, String officeName, String centerCode, bool sendEmail)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.grantRight(idIUnit, rightType, userName, officeName, centerCode, sendEmail);
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't grant rights!");
            }
            return new HxXWResponse(s);
	    }

	    public HxXWResponse postIt(int idIUnit, String postitContent)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.postIt(idIUnit, postitContent);
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't post it!");
            }
            return new HxXWResponse(s);
	    }

        public void unlockDocument(int idIUnit)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    ewws.unlockDocument(idIUnit);
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done)
            {
                throw new Exception("Can't unlock!");
            }
	    }

	    public void deleteDocument(int idIUnit)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    ewws.deleteDocument(idIUnit);
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done)
            {
                throw new Exception("Can't delete!");
            }            
	    }

        public HxXWResponse saveDocument(String xml)
        {
            return saveDocument(xml, false, -1);   
        }

        public HxXWResponse saveDocument(String xml, bool modify)
        {
            return saveDocument(xml, modify, -1);   
        }

        public HxXWResponse saveDocument(String xml, bool modify, int idIUnit)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            String s = "";
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    s = ewws.saveDocument(xml, modify, idIUnit);
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || (s == ""))
            {
                throw new Exception("Can't save document!");
            }
            return new HxXWResponse(s);
        }
        
        public byte[] getAttachment(String id)
        {
            bool done = false;
            int tries = xDocUnSucessfullTryesCount;
            byte[] ret = null;
            while (!done && tries > 0)
            {
                try
                {
                    //mark try
                    --tries;
                    ret = ewws.getAttachment(id);
                    getCookies();
                    done = true;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Connessione inattiva!"))
                    {
                        init();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            if (!done || ret == null)
            {
                throw new Exception("Can't load attachment!");
            }
            return ret;
        }

        public HxXWResponse raccoglitore(String nrec, String titleRule)
	    {
            HxXWSelection resp = null;
            HxXWResponse resp1 = null;
            String ret = "<Response />";
		    String queryStr = "[XML,/raccoglitore/@nrecord/]=" + nrec;
		    String subTtitleRule = "xml,/raccoglitore/rif_contenuto/oggetto/@codice";

            try
            {
                resp = executeQuery(queryStr);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Esito della ricerca nullo!"))
                {
                    throw e;
                }
            }

            if (resp.id.Length > 0)
            {
                if (resp.size > 0)
                {
                    //setActiveSelectionId(resp.id);
                    resp1 = getFirstTitlePage(subTtitleRule);
                }
                
            }

            if (resp1.pageIndex.Length > 0)
            {
                /*xml = new XmlDocument();
                xml.LoadXml(resp1);
                root = xml.DocumentElement;
                node = root.SelectSingleNode("Item");*/
                XmlNode node = resp1.xQuery("//Response/Item");
                
                String resp2 = node != null ? node.Attributes["value"].Value : "";
                if (resp2.Length > 0)
                {
                    //take all doc nrecords and replace ';' => ','
                    queryStr = "[XML,/doc/@nrecord/]={" + resp2.Replace(";", ",") + ",00000000}";
                    return query(queryStr, titleRule);
                }
                
            }

            return new HxXWResponse(ret);
	    }
    }
}