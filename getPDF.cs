using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MOCD.CS.Report.Generate.ReportService;
using Microsoft.Crm.Sdk.Messages;
using Newtonsoft.Json;

namespace MOCD.CS.Report.Generate
{
    public class getPDF : CodeActivity
    {
        [Input("E-Mail")]
        [ReferenceTarget("email")]
        public InArgument<EntityReference> Email { get; set; }

        [Input("Report Name")]
        public InArgument<string> RptName { get; set; }
        [Input("ApplicationNum")]
        public InArgument<string> AppNum { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            var tracingService = executionContext.GetExtension<ITracingService>();
            var context = executionContext.GetExtension<IWorkflowContext>();
            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {

                Guid RecordId = context.PrimaryEntityId;
                string _rpName = RptName.Get<string>(executionContext);
                string _appNum = AppNum.Get<string>(executionContext);
                byte[] result = null;

                ReportExecutionService rs = new ReportExecutionService();
                rs.Credentials = new NetworkCredential("<Username>", "<Password>", "<Domain>");
                rs.Url = "<Org_URL>/ReportExecution2005.asmx";
                tracingService.Trace(context.OrganizationName);
                string reportPath = "/"+context.OrganizationName+"_MSCRM/"+_rpName;
                string format = "PDF";
                string historyID = null;
                string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
                string encoding;
                string mimeType;
                string extension;
                Warning[] warnings = null;
                string[] streamIDs = null;
                ParameterValue[] parameters = new ParameterValue[1];
                parameters[0] = new ParameterValue();
                parameters[0].Name = "id";
                parameters[0].Value = RecordId.ToString();
                tracingService.Trace("Id passed: " + RecordId.ToString());
                ExecutionInfo execInfo = new ExecutionInfo();
                ExecutionHeader execHeader = new ExecutionHeader();
                rs.ExecutionHeaderValue = execHeader;
                execInfo = rs.LoadReport(reportPath, historyID);
                rs.SetExecutionParameters(parameters, "en-us");
                String SessionId = rs.ExecutionHeaderValue.ExecutionID;

                result = rs.Render(format, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                Response.Rootobject response = this.stampletter(Convert.ToBase64String(result), Convert.ToInt32(result.Length.ToString()), _appNum, "<Token>", "<BlockChain_URL>");
                string StamppedFile = response.UploadZipHash;
                //Create email activity
                Entity attachment = new Entity("activitymimeattachment");
                attachment["objectid"] = Email.Get<EntityReference>(executionContext);
                attachment["objecttypecode"] = "email";
                attachment["filename"] = "الشهادة.pdf";
                attachment["subject"] = "لمن يهمه الأمر";
                attachment["body"] = System.Convert.ToBase64String(response.StampContent);

                try
                {
                    service.Create(attachment);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating attachment - {0}", ex.Message));
                }

                try
                {
                    service.Execute(new SendEmailRequest()
                    {
                        EmailId = Email.Get<EntityReference>(executionContext).Id,
                        IssueSend = true,
                        TrackingToken = string.Empty
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error sending email - {0}", ex.Message));
                }


            }
            catch (Exception err)
            {
                throw new Exception("Exception" + err);
            }
        }

        private Response.Rootobject stampletter(string filebody,int filesize, string filetag,  string token,  string APoURL)
        {
            string str = JsonConvert.SerializeObject(new Request()
            {
                AccountKey = token,
                Operation = "8",
                Culture = "ar-AE",
                FileName = (filetag + ".pdf"),
                FileType = "application/pdf",
                FileSize = filesize.ToString(),
                Tags = ("FL" + filetag),
                Base64Content = filebody
            });
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(APoURL);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = (long)str.Length;
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(requestStream, Encoding.ASCII))
                    streamWriter.Write(str);
            }
            using (Stream stream = httpWebRequest.GetResponse().GetResponseStream() ?? Stream.Null)
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    Response response = new Response();
                    return JsonConvert.DeserializeObject<Response.Rootobject>(streamReader.ReadToEnd());
                }
            }
        }
    }
}

