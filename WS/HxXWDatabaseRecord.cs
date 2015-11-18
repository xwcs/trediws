using System.Collections;
using System;
using XDocBase.Web;
using XDocBase.Web.CFG;
using XDocBase.Web.CONTAINER;
using System.Collections.Generic;

namespace XDocBase.WS
{
[CLSCompliant(false)]
public class HxXWDatabaseRecord{
	public Session  session = null;
	public String   ID;
	public String   label;
	public String   db_host;
	public String   db_port;
	public String   db_usr;
	public String   db_pwd;
	public String   db_name;
	public String   db_encoding;
	public Int32    titlePageSize;
    public Int32    indexPageSize;
    public Boolean  isDataDb = true;
    public Boolean  bDefault = false;
    public Boolean  isClone = false;
    public String connection = "";

	protected HxXWDatabase objDatabase;

    public HxXWDatabaseRecord(Session session, KeyValuePair aParams)
        : this(session, aParams, false)
	{
    }

    private HxXWDatabaseRecord(Session session, String ID, String connection, Boolean clone)
    {
        this.session = session;
        this.objDatabase = null;
        this.isClone = clone;
        this.connection = connection;

        //take connection string   host:port:db
        string[] v = connection.Split(new Char[] { ':' });
        if (v.Length != 3)
            throw new Exception("Incorrect DB connection string! [" + connection + "]");

        ConfigData cfg = ConfigData.Open();
        this.ID = ID;
        this.label = ID;
        this.db_host = v[0];
        this.db_port = v[1];
        this.db_usr = cfg.application["xDocUser"] ?? "lettore";
        this.db_pwd = cfg.application["xDocPass"] ?? "reader";
        this.db_name = v[2];
        this.db_encoding = cfg.application["xDocEncoding"] ?? "utf-8";
        this.titlePageSize = cfg.application["xDocIndexPageSize"].AsInt32;
        this.indexPageSize = cfg.application["xDocTitlesPageSize"].AsInt32;
        this.bDefault = ID == "MAIN" ? true : false;
    }

    public HxXWDatabaseRecord(Session session, KeyValuePair aParams, Boolean clone)
        : this(session, aParams.Key, aParams.AsString, clone) 
	{		
	}

    public HxXWDatabaseRecord cloneSelf(String newID){
        return new HxXWDatabaseRecord(session, newID, this.connection, true); 
	}

    public HxXWDatabase getDbObj()
	{
		if(this.objDatabase == null)
			this.objDatabase = new HxXWDatabase(this.session, this.ID, this.db_host, this.db_port, this.db_usr, this.db_pwd, this.db_name, this.db_encoding, this.titlePageSize, this.indexPageSize);
		return this.objDatabase;
	}
}}
