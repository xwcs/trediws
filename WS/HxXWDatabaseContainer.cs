using XDocBase.Web;
using System.Collections;
using XDocBase.Web.CONTAINER;
using System;
using System.Xml;
using XDocBase.Web.CFG;
namespace XDocBase.WS
{
    [CLSCompliant(false)]
    public class HxXWDatabaseContainer
    {
        protected ArrayList aDbRecs = null;
        protected Session session = null;

        protected HxXWDatabaseRecord recDefaultDatabase = null;
        protected HxXWDatabaseRecord recCurrentDatabase = null;

        public HxXWDatabaseContainer(Session session)
        {
            this.session = session;
            recDefaultDatabase = null;
            recCurrentDatabase = null;

            aDbRecs = new ArrayList();
            KeyValueMap databases = ConfigData.Open().db;
            //make all db records
            foreach (KeyValuePair p in databases)
            {
                createDbRec(p);
            }
            if (recDefaultDatabase == null)
            {
                throw new Exception("Missing default xw DB! [Must have ID 'MAIN']");
            }
        }

        protected void createDbRec(KeyValuePair aParams)
        {
            HxXWDatabaseRecord recDatabase = new HxXWDatabaseRecord(session, aParams);
            aDbRecs.Add(recDatabase);
            if (recDatabase.bDefault)
                recDefaultDatabase = recDatabase;
        }

        //clones are necesary for separate db connections in multi thread environment
	    //and multi titles requests
        public void cloneDbConnection(String newId, String oldId)
        {
		    HxXWDatabaseRecord dbRec = getDbRecById(oldId);
		    if(dbRec != null){			
			    aDbRecs.Add(dbRec.cloneSelf(newId));		
		    }
	    }

        public XDocBase.Web.CONTAINER.Map getDbList()
        {
            return getDbList(true);
        }
        public XDocBase.Web.CONTAINER.Map getDbList(bool dataDbOnly)
        {
            XDocBase.Web.CONTAINER.Map tmp = new XDocBase.Web.CONTAINER.Map();
            foreach (HxXWDatabaseRecord recDatabase in aDbRecs)
                if ((dataDbOnly && recDatabase.isDataDb) || (!dataDbOnly))
                    tmp[recDatabase.ID] = recDatabase.label;
            return tmp;
        }

        public HxXWDatabase setCurrentDb()
        {
            return setCurrentDb(null);
        }

        public HxXWDatabase setCurrentDb(String sID)
        {
		    //String curID = "";
		    /*
		     * If sId is null or empty string try to take current db from session
		     * if neither session hold some db take default
		     */
		    if(sID == null || sID == ""){
			    sID = (String)(session["recCurrentDatabase"]);
			    if(sID != null && sID != ""){
				    //we take curDb from session
                    if (recCurrentDatabase == null)
                    {
                        //and change happend
                        recCurrentDatabase = getDbRecById(sID);
                        if (recCurrentDatabase == null)
                        {
                            throw new Exception("Unknown database id [" + sID + "]");
                        }
                    } 
                    if (recCurrentDatabase.ID != sID)
                    {
					    //and change happend
					    recCurrentDatabase = getDbRecById(sID);
					    if(recCurrentDatabase == null){
						    throw new Exception("Unknown database id [" + sID + "]");
					    }
				    }
			    }else{
				    //take default and set it into session too
				    recCurrentDatabase = recDefaultDatabase;
				    session["recCurrentDatabase"] =  recCurrentDatabase.ID;
			    }
		    }else{
			    //test if there is a change
			    if((recCurrentDatabase != null && sID != recCurrentDatabase.ID) ||
			       (recCurrentDatabase == null)){
				    //lets change
				    session["recCurrentDatabase"] = sID;
				    recCurrentDatabase = this.getDbRecById(sID);
				    if(recCurrentDatabase == null){
					    throw new Exception("Unknown database id [" + sID + "]");
				    }
			    }
		    }
		    return recCurrentDatabase.getDbObj();
	    }

        public HxXWDatabase getCurrentDb()
        {
            if (recCurrentDatabase == null)
            {
                return setCurrentDb();
            }
            return recCurrentDatabase.getDbObj();
        }

        public void useDefaultDb()
        {
            setCurrentDb(recDefaultDatabase.ID);
        }

        protected HxXWDatabaseRecord getDbRecByName(String sDbName)
        {
            foreach (HxXWDatabaseRecord recDatabase in aDbRecs)
            {
                //echo("dbname={objDatabase.getDb()}<BR>");
                if (recDatabase.db_name.CompareTo(sDbName) == 0)
                {
                    //echo "reusing DB object <BR>";
                    return recDatabase;
                }
            }
            return null;
        }

        public HxXWDatabase getDatabaseByName(String sDbName)
        {
		    HxXWDatabaseRecord tmp = getDbRecByName(sDbName);
		    if(tmp != null)
			    return tmp.getDbObj();
		    else throw new Exception("Database [" + sDbName + "] not found!");
	    }

        protected HxXWDatabaseRecord getDbRecById(String sID)
        {
            foreach (HxXWDatabaseRecord recDatabase in aDbRecs)
                if (recDatabase.ID.CompareTo(sID) == 0)
                    return recDatabase;
            return null;
        }

        public HxXWDatabase getDatabaseById(String sDbName)
        {
		    HxXWDatabaseRecord tmp = getDbRecById(sDbName);
		    if(tmp != null)
			    return tmp.getDbObj();
		    else throw new Exception("Database [" + sDbName + "] not found!");
	    }

        
    }
}