using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using gds_services;
using System.Configuration;
using Newtonsoft.Json;
namespace gds_services
{
    public class SMS_Response : Response
    {
        public class SMS_Result
        {
            public string type;
            public int booking_id;
            public string mobile_no;

        }
        public SMS_Result result = new SMS_Result();
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SMS_Services" in code, svc and config file together.
    public class SMS_Services : ISMS_Services
    {
        public string test()
        {
            return "OK";
        }

        public SMS_Response send_sms(string type, int booking_id, string mobile_no, string content_dict, string key)
        {

            SMS_Response response = new SMS_Response();
            response.result.type = type;
            response.result.booking_id = booking_id;
            response.result.mobile_no = mobile_no;
            Dictionary<string,string> content =  JsonConvert.DeserializeObject<Dictionary<string, string>>(content_dict);

            SMS.SMS_Sender sms_sender;
            string sms_template;
            SMS.SMS_Data sms_data;
            string sms_text;
            string sms_complete_url="";
            string tag = "TYARRI";
            Utils.clsLogger logger = new Utils.clsLogger();
            string default_sms_gateway=ConfigurationManager.AppSettings["DEFAULT_SMS_GATEWAY"].ToString();
            try
            {
                switch (type)
                {
                    //SMS on booking 
                    case "booking_sms_ty":
                    case "booking_sms_gds":
                    case "cancel_sms_ty":
                    case "cancel_sms_gds":
                    case "pickup_mismatch_sms_ty":
                    case "pickup_mismatch_sms_gds":
                    case "pickup_reminder_sms_ty":
                    case "pickup_reminder_sms_gds":
                        if (type.EndsWith("gds"))
                            tag = "IamGDS";
                        sms_sender = new SMS.SMS_Sender(default_sms_gateway,type, key);
                        sms_template = SMS.SMS_Sender.get_sms_template(type);
                        sms_data = new SMS.SMS_Data(content);
                        sms_text = sms_data.prepare_booking_sms(sms_template);
                        sms_complete_url=sms_sender.send_sms(mobile_no, sms_text, tag);
                        response.status = true;
                        break;
                    default:
                        throw new System.Exception("Invalid SMS Type");
                }

            }
            catch (System.Exception ex)
            {
                logger.log("error", new Dictionary<string, object>
                {   {"booking_id",booking_id},
                    {"mobile_no",mobile_no},
                    {"type",type}
                }, ex.ToString());
                response.status = false;
                response.error = ex.Message;
            }
            //Log sms into db for accounting purpose;
            SMS.SMS_Sender.log_sms_into_db(booking_id, mobile_no, sms_complete_url, Convert.ToInt32(response.status));
            return response;
        }
    }
}
