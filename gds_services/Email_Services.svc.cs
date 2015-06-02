using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using gds_services;
using System.Configuration;
using System.Web;
using System.IO;
using System.Web.Script.Services;
using Newtonsoft.Json;


namespace gds_services
{
    public class Email_Response : Response
    {
        public class Email_Result
        {
            public string type;
            public int booking_id;
            public string email_ids;
            public string cc_email_ids;
            public string bcc_email_ids;
            public bool status;
        }
        public Email_Result result = new Email_Result();
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Email_Services" in code, svc and config file together.
    [ScriptService]
    public class Email_Services : IEmail_Services
    {
        public string test()
        {
            return "OK";
        }
        public Email_Response send_email(string type, int booking_id, string email_ids, string cc_email_ids, string bcc_email_ids, string subject, string content_dict, string attachments_dict)
        {
            Email_Response response = new Email_Response();
            Email.Email email;
            Utils.clsLogger logger = new Utils.clsLogger();
            Dictionary<string, object> content = new Dictionary<string, object>();
            Dictionary<string, object> attachments = new Dictionary<string, object>();

            if (type == null || type.Trim().Length == 0)
            {
                type = "blank_email";
            }
            
            content = JsonConvert.DeserializeObject<Dictionary<string, object>>(content_dict);
            
            if (attachments_dict != null)
            {
                attachments = JsonConvert.DeserializeObject<Dictionary<string, object>>(attachments_dict);
            }

            string[] valid_email_types = ConfigurationManager.AppSettings["Valid_Email_Types"].ToString().Split(',');

            try
            {
                response.result.type = type;
                response.result.booking_id = booking_id;
                response.result.email_ids = email_ids;
                response.result.cc_email_ids = cc_email_ids;
                response.result.bcc_email_ids = bcc_email_ids;
                response.status = true;

                if(valid_email_types.Contains(type))
                {
                    email = new Email.Email(type, subject, content,attachments);
                    response.status = email.send_email(booking_id, email_ids, cc_email_ids, bcc_email_ids);
                }
                else
                {
                    throw new System.Exception("Invalid Email Type");
                }
            }
            catch (System.Exception ex)
            {
                logger.log("error", new Dictionary<string, object>
                {   {"booking_id",booking_id},
                    {"email_ids",email_ids},
                    {"cc_email_ids",cc_email_ids},
                    {"bcc_email_ids",bcc_email_ids},
                    {"type",type}
                }, ex.ToString());
                response.status = false;
                response.error = ex.Message;
            }

            //Log email into db;
            //Email.Email.log_email_into_db(type, booking_id, response.result.email_ids, response.result.cc_email_ids, response.result.bcc_email_ids, response.error, Convert.ToInt32(response.status));
            return response;
        }
    }
}
