using System.Web;
using System;
using XDocBase.Web.CONTAINER;
using XDocBase.WS;
using System.Collections.Generic;
namespace XDocBase.Web.UI
{
    class XDocNewDoc : XDocAjaxRequestHandler
    {
        override public void ProcessRequest(HttpContext context)
        {
            init(context);

            try
            {
                frm = loadFormTemplate("/" + layouts + "/xdoc/forms/newDocForm.htx");        

                
                //make replaces
                frm = frm.Replace("<?=$layouts?>",   layouts);
                frm = frm.Replace("<?=$target?>",    target);
                frm = frm.Replace("<?=$handler?>",   handler);
                frm = frm.Replace("<?=$skin?>",      skin);

                //repertories
                HxXWPerson personObject = new HxXWPerson(session, config.dbCon.getDatabaseById("ACL1"));

                Boolean hasRepertories = config.repertories.hasRepertories(personObject, true);
                //has repertories dependant peaces
                String hr_part1 = "";
                String hr_part2 = "";
                String hr_part3 = "";
                String hr_part4 = "";
                String hr_part5 = "";
                String hr_part6 = "";
                String repid = "";
                String groupid = "";
                String sgroupid = "";
                if (hasRepertories)
                {
                    hr_part1 = "<script type=\"text/javascript\" src=\"" + layouts + "/xdoc/js/repertoryCmbSupport.js\"></script>";

                    hr_part2 = @"
				        <div class=""formRow"">
					        <div class=""formCellFirst""><span>Repertorio: </span></div>
					        <div class=""formCellLast""><select name=""repid"" id=""repid"" class=""asInput"" onchange=""callChangeGroup(this.value)""></select></div>
					        <p class=""clearL""/>
				        </div>
				        <div class=""formRow"">
					        <div class=""formCellFirst"">Classificazione:</div>
					        <div class=""formCell""><span id=""grpLabel"">Tipo pubbl.:</span><br /><select name=""groupid"" id=""groupid"" class=""asInput""  onchange=""callChangeSGroup(document.getElementById('repid').value, this.value)""></select></div>
					        <div class=""formCellLast""><span id=""sgrpLabel"">Classificazione:</span><br /><select name=""sgroupid"" id=""sgroupid"" class=""asInput""></select></div>
					        <p class=""clearL""/>
				        </div>";

                    hr_part3 = config.repertories.dumpRepertories(personObject, true);
                    hr_part4 = "changeRepertory(reps, '-1');";
                    hr_part5 = "changeRepertory(reps, '" + repid + "');";
                    if (groupid != "")
                    {
                        hr_part5 += "changeGroup(reps, '" + repid + "', '" + groupid + "');";
                        if (sgroupid != "")
                        {
                            hr_part5 += "changeSGroup(reps, '" + repid + "', '" + groupid + "', '" + sgroupid + "');";
                        }
                    }
                    hr_part6 = @"
				        this.callChangeGroup = function(repid){
					        changeGroup(reps, repid);
				        };
				        this.callChangeSGroup = function(repid, groupid){
					        changeSGroup(reps, repid, groupid);
				        };";
                }

                frm = frm.Replace("<?=$hr_part1?>", hr_part1);
                frm = frm.Replace("<?=$hr_part2?>", hr_part2);
                frm = frm.Replace("<?=$hr_part3?>", hr_part3);
                frm = frm.Replace("<?=$hr_part4?>", hr_part4);
                frm = frm.Replace("<?=$hr_part5?>", hr_part5);
                frm = frm.Replace("<?=$hr_part6?>", hr_part6);

                //dblist
                XDocBase.Web.CONTAINER.Map dbList = config.dbCon.getDbList();
                String outDbList1 = "";
                String outDbListMore = "";
                if (dbList.Count <= 1)
                {
                    foreach (KeyValuePair<String, Object> p in dbList)
                    {
                        outDbList1 += "<input type=\"hidden\" name=\"dbid\" id=\"dbid\" value=\"" + p.Key + "\"/>";
                    }
                }
                else
                {
                    outDbListMore += @"<div  class=""formRow"">
			        	                <div class=""formCellFirst"">Archivio:</div>
			                        	<div class=""formCellLast""><select name=""dbid"" id=""dbid"" class=""asInput"">";
                    foreach (KeyValuePair<String, Object> p in dbList)
                    {
                        outDbListMore += "<option id=\"" + p.Key + "\" value=\"" + p.Key + "\">" + ((String)p.Value) + "</option>";
                    }
                    outDbListMore += @"		</select></div><p class=""clearL""/>
			                            </div>";
                }
                frm = frm.Replace("<?=$outDbList1?>", outDbList1);
                frm = frm.Replace("<?=$outDbListMore?>", outDbListMore);



                frm = frm.Replace("<?=$today?>", DateTime.Now.ToString("d"));
                frm = frm.Replace("<?=$today1M?>", DateTime.Now.AddMonths(1).ToString("d"));
                frm = frm.Replace("<?=$service?>", appSettings["xDocServicesPath"] + "/newdocproc." + appSettings["xDocServiceExt"]); 

                //render output
                context.Response.Write(frm);
            }
            catch (Exception ex)
            {
                context.Response.Write(new AjaxServiceError(ex, target != "" ? target : "xDocDesktopHolderID"));
            }
        }
    }
}