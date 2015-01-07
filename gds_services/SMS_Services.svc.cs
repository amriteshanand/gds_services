using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using gds_services;
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
        //public Dictionary<string,object> result = new Dictionary<string,object>();
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SMS_Services" in code, svc and config file together.
    public class SMS_Services : ISMS_Services
    {
        public string test()
        {
            return "OK";
        }

        public SMS_Response send_sms(string type, int booking_id, string mobile_no)
        {

            SMS_Response response = new SMS_Response();
            response.result.type = type;
            response.result.booking_id = booking_id;
            response.result.mobile_no = mobile_no;

            SMS.SMS_Sender sms_sender;
            string sms_template;
            SMS.SMS_Data sms_data;
            string sms_text;
            Utils.clsLogger logger = new Utils.clsLogger();
            try
            {
                switch (type)
                {
                    case "booking_sms":
                        sms_sender = new SMS.SMS_Sender("SMS_GATEWAY_EZEESMS", "test_client_key");
                        sms_template = SMS.SMS_Sender.get_sms_template("booking_sms");
                        sms_data = new SMS.SMS_Data(booking_id);
                        sms_text = sms_data.prepare_booking_sms(sms_template);
                        sms_sender.send_sms(mobile_no, sms_text);
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
                //response.error = ex.Message;
                response.error = ex.ToString();
            }
            return response;
        }
    }
}
