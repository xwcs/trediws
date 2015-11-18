using System;
using XDocBase.Web.CFG;
using XDocBase.WS;
using System.Web;
using System.IO;

namespace XDocBase.Web.UI
{
    class XDocNewDocProc : XDocAjaxRequestHandler
    {
        
        //dd/mm/yyyy -> yyyymmdd
        protected String formatDateToXW(String date)
        {
            string [] vs = date.Split(new Char[] {'/'});
            String tmp1 = "00" + vs[1];
            String tmp0 = "00" + vs[0];
            return vs[2] + tmp1.Substring(tmp1.Length - 2) + tmp0.Substring(tmp0.Length - 2);
        }

        override public void ProcessRequest(HttpContext context)
        {
            init(context, false); //skip content type

            response.ContentType = "text/html";

            response.Write("<html>\r\n");
            response.Write("<head>\r\n");
            response.Write("<script type=\"text/javascript\">\r\n");
            response.Write("//<![CDATA[\r\n");
            response.Write("function execResult(){\r\n");

            try
            {
                String dbid	        = pms["dbid"];
                String repid        = pms["repid"];
		        String groupid	    = pms["groupid"];
                String sgroupid     = pms["sgroupid"];
		        String dataInizioPubb	= formatDateToXW(pms["dataInizioPubb"]);
		        String dataFinePubb 	= formatDateToXW(pms["dataFinePubb"]);
		        String oggetto		= pms["oggetto"];
		        String note 	    = pms["note"];
		        String op			= pms["opertion"];
                
		        HxXWPerson person = new HxXWPerson(session, config.dbCon.getDatabaseByName("acl"));
		        HxXWDatabase xw = config.dbCon.setCurrentDb(dbid);
		
		        HxXWDocwayDocument xdoc = new HxXWDocwayDocument(xw, person);
		        //repertorio
                String repertorio = config.repertories.getRepertorioXML(person, repid);
		        //extra
                String extra = "";
		        extra += "	<extra>";
                extra += "		<tivdata grp=\"" + groupid + "\" subgrp=\"" + sgroupid + "\" pub_from=\"" + dataInizioPubb + "\" pub_to=\"" + dataFinePubb + "\"/>";
		        extra += "	</extra>";
		
		        xdoc.create(oggetto, repertorio, extra);
		        xdoc.save();
		
		        HxXWResponse resp = null;
                //now add postit
		        if(note.Length > 0)
			        resp = xw.postIt(xdoc.idIUnit, note);
		
		        
                HttpContext postedContext = HttpContext.Current;
                //File Collection that was submitted with posted data
                HttpFileCollection Files = postedContext.Request.Files;

                //Make sure a file was posted
                string fileName = (string)postedContext.Request.Form["file"];
                //if (Files.Count == 1 && Files[0].ContentLength > 0 && fileName != null && fileName != ""){
                if (Files.Count == 1 && Files[0].ContentLength > 0)
                {
                    //The byte array we'll use to write the file with
                    byte[] binaryWriteArray = new byte[Files[0].InputStream.Length];
                    //Read in the file from the InputStream
                    Files[0].InputStream.Read(binaryWriteArray, 0, (int)Files[0].InputStream.Length);
                    //Open the file stream
                    /*
                    FileStream objfilestream = new FileStream("c:\\test.txt", FileMode.Create, FileAccess.ReadWrite);
                    objfilestream.Write(binaryWriteArray, 0, binaryWriteArray.Length);
                    objfilestream.Close();
                     */
                    xw.checkInContentFile(xdoc.idIUnit, Files[0].FileName, "", binaryWriteArray);
                }

		        if(op.CompareTo("godoc") == 0){
                    context.Response.Write("parent.loadDocument(" + xdoc.idIUnit + ");\r\n");
		        }else{
			        context.Response.Write("parent.continueNew();\r\n");
		        }
	        }catch(Exception e){
                context.Response.Write("alert(\"" + e.Message + "\");\r\n");
                context.Response.Write("parent.handleError();\r\n");
	        }
            context.Response.Write("}\r\n");
            context.Response.Write("//]]>\r\n");
            context.Response.Write("</script>\r\n");
            context.Response.Write("</head>\r\n");
            context.Response.Write("<body onload=\"execResult()\">\r\n");
            context.Response.Write("</body>\r\n");
            context.Response.Write("</html>\r\n");
        }
    }
}