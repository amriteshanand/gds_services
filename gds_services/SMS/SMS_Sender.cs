using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using gds_services;
using System.Data;

namespace gds_services.SMS
{
    public class SMS_Sender
    {
        public string gateway_url = null;
        public string client_key = null;
        
        public int max_retries;
        private Utils.clsLogger logger;
        /*
         * gateway: pass gateway name to be used and gateway configuration must be added in configuration
         * client_key: to authorise client
         */
        public SMS_Sender(string gateway,string client_key)
        {
            this.gateway_url = SMS_Sender.get_sms_gateway(gateway);
            this.validate_client_key(client_key);
            this.client_key = client_key;
            this.init_config();
            this.logger = new Utils.clsLogger();
        }

        private void init_config(){
            max_retries = 1;
            try
            {
                max_retries = Convert.ToInt32(ConfigurationManager.AppSettings["SMS_MAX_RETRIES"]);
            }
            catch (System.Exception ex)
            { 

            }
        }

        private void validate_client_key(string client_key)
        { 
            //TODO:
        }

        /*
         * This method can be used to send sms
         */
        public void send_sms(string mobile_no,string text)
        {
            int curr_tries = 0;
            bool success = false;
            string sms_gateway_response="";
            while (curr_tries < this.max_retries)
            {
                try
                {
                    curr_tries++;
                    string fetch_url = this.gateway_url;
                    fetch_url = fetch_url.Replace("@@mobile_no", mobile_no);
                    fetch_url = fetch_url.Replace("@@sms_text", text);
                    Utils.HTTP sms_http = new Utils.HTTP();
                    sms_gateway_response = sms_http.GET(fetch_url);
                    if (sms_gateway_response.ToUpper().Contains("SENT"))
                    {
                        success = true;
                    }
                    else 
                    {
                        success = false;

                    }                    
                    break;
                }
                catch (System.Exception ex)
                {
                    logger.log("error", ex);
                    continue;
                }
            }
            //TODO:Log sms and its status;
            if (!success) {
                this.logger.log("fatal", sms_gateway_response);
                throw new System.Exception("Failed to send sms");                
            }            
        }
        
        /*
         * Static Methods
         */
        private static string get_sms_gateway(string gateway_name)
        {
            string gateway_url=ConfigurationManager.AppSettings[gateway_name];
            if(gateway_url!=null && gateway_url.Trim().Length>0)
            {
                return gateway_url;
            }
            else
            {
                throw new System.Exception("Gateway Not Found !");
            }            
        }
        public static string get_sms_template(string template_name)
        {
            string sms_template = null;
            DB.clsDB db = new DB.clsDB();
            db.AddParameter("TEMPLATE_NAME",template_name,30);
            DataSet ds= db.ExecuteSelect("GET_GDS_SMS_TEMPLATE",System.Data.CommandType.StoredProcedure,30);
            if (ds!=null && ds.Tables.Count>0 && ds.Tables[0].Rows.Count>0){
                sms_template = ds.Tables[0].Rows[0]["TEMPLATE"].ToString();
            }            
            if (sms_template != null && sms_template.Trim().Length > 0)
            {
                return sms_template;
            }
            else
            {
                throw new System.Exception("Template Not Found!");
            }                        
        }
    }
    
    public class SMS_Data
    {
        public SMS_Data(int booking_id)
        { 

        }
        public string prepare_booking_sms(string sms_template)
        {
            return sms_template;
        }
    }
}