using XDocBase.WS;
using System.Collections;
using System.Web;
using System;

namespace XDocBase.Web.UI
{
    class XDocAttach : XDocAjaxRequestHandler
    {
        override public void ProcessRequest(HttpContext context)
        {
            init(context, false);

            Hashtable extensions = new Hashtable();

            extensions.Add("tgz", "application/x-gtar");
            extensions.Add("tar.gz", "application/x-gtar");
            extensions.Add("tar", "application/x-tar");
            extensions.Add("zip", "application/zip");
            extensions.Add("gif", "image/gif");
            extensions.Add("jpeg", "image/jpeg");
            extensions.Add("jpg", "image/jpeg");
            extensions.Add("jpe", "image/jpeg");
            extensions.Add("png", "image/png");
            extensions.Add("tiff", "image/tiff");
            extensions.Add("tif", "image/tiff");
            extensions.Add("kdc", "image/x-kdc");
            extensions.Add("mpeg", "video/mpeg");
            extensions.Add("mpg", "video/mpeg");
            extensions.Add("mpe", "video/mpeg");
            extensions.Add("mng", "video/x-mng");
            extensions.Add("doc", "application/msword");
            extensions.Add("xls", "application/msword");
            extensions.Add("ppt", "application/msword");
            extensions.Add("xml", "text/xml");
            extensions.Add("xsl", "text/xml");
            extensions.Add("xslt", "text/xml");
            extensions.Add("rss", "text/xml");
            extensions.Add("txt", "text/plain");
            extensions.Add("csv", "text/csv");
            extensions.Add("rtf", "text/rtf");
            extensions.Add("pdf", "application/pdf");
            extensions.Add("odt", "application/odt");
       
            

            //datasource
            HxXWDatabase ds = config.dbCon.getCurrentDb();

            try
            {
                String fileTitle = pms["fileTitle"];
                String attId = pms["file"];
                if (attId != null && attId != "")
                {
                    String ext = attId.Substring(attId.IndexOf('.') + 1);
                    String mime =  (String) extensions[ext];

                    if (mime != null && mime != "")
                    {
                        response.ContentType = mime;
                    }
                    else
                    {
                        response.ContentType = "application/octet-stream";
                    }

                    response.AddHeader("Content-Transfer-Encoding", "binary");

                    if(fileTitle != null && fileTitle != ""){
                        response.AddHeader("Content-Disposition", "attachment; filename=" + fileTitle);
                    }               

                    byte[] att = ds.getAttachment(attId);
                    if (att != null)
                    {
                        response.BinaryWrite(att);
                    }
                    else
                    {
                        response.ContentType = "text/plain";
                        response.Write("Attachment not found!");
                    }
                }else{
                    response.ContentType = "text/plain";
                    response.Write("Missing attachment id!");
                }
            }
            catch (Exception ex)
            {
                response.ContentType = "text/plain";
                response.Write(ex.Message);
            }
        }
    }
}