﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using gds_services;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace gds_services.SMS
{
    public class SMS_Sender
    {
        public string gateway_url = null;
        public string type = null;
        private Utils.clsLogger logger;

        public SMS_Sender(string gateway,string type, string client_key)
        {
            this.gateway_url = SMS_Sender.get_sms_gateway(gateway,type);
            this.validate_client_key(type, client_key);
            this.logger = new Utils.clsLogger();
            this.type = type;
        }
        private void validate_client_key(string type,string client_key)
        { 
            string salt=ConfigurationManager.AppSettings["API_KEY_SALT"].ToString();
            string salted_type=type+"|"+salt;
            string generated_key = Utils.Key_Generator.get_MD5_hash(salted_type);
            if (generated_key != client_key) 
            {
                throw new System.Exception("Invalid key !");
            }
        }
        public string send_sms(string mobile_no,string text, string tag)
        {
            bool success = false;
            string sms_gateway_response="";
            string fetch_url ="";

            string[] mobile_nos = mobile_no.Split(',');
            foreach (string mobile in mobile_nos)
            {
                try
                {
                    success = false;
                    fetch_url = this.gateway_url;
                    fetch_url = fetch_url.Replace("@@mobile_no", mobile);
                    fetch_url = fetch_url.Replace("@@sms_text", text);
                    fetch_url = fetch_url.Replace("@@senderid", tag);
                    Utils.HTTP sms_http = new Utils.HTTP();
                    sms_gateway_response = sms_http.GET(fetch_url);
                    success = sms_gateway_response.ToUpper().Contains("3001");
                    if (!success)
                        success = sms_gateway_response.ToUpper().Contains("SENT");

                }
                catch (System.Exception ex)
                {
                    logger.log("error", ex);
                }
                if (!success)
                {
                    this.logger.log("fatal", sms_gateway_response);
                    throw new System.Exception("Failed to send sms");
                }
            }
            return fetch_url;
        }
        private static string get_sms_gateway(string gateway_name, string type)
        {
            string gateway_url=ConfigurationManager.AppSettings[gateway_name];
            NameValueCollection gateways = (NameValueCollection)ConfigurationManager.GetSection("sms_gateway");
            try { gateway_url = gateways[type]; }
            catch { }
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
        public static void log_sms_into_db(int booking_id, string mobile_no, string url, int is_sent)
        {
            int sms_id=0;
            string str_error="";
            DB.clsDB db = new DB.clsDB();

            db.AddParameter("SMS_ID", sms_id);
            db.AddParameter("BOOKING_ID", booking_id);
            db.AddParameter("MOBILE_NO", mobile_no,20);
            db.AddParameter("URL", url,500);
            db.AddParameter("URL_DEPART", "",20);
            db.AddParameter("SEND_TIME", DateTime.Now);
            db.AddParameter("ERR_MSG", str_error, 20);
            db.AddParameter("IS_SENT", is_sent);
            db.ExecuteDML("spSMSLog_Insert", CommandType.StoredProcedure, 30);
        }
    }
    
    public class SMS_Data
    {
        public int booking_id;
        public string text=null;
        List<Dictionary<string, object>> details_list;
        Dictionary<string, string> content;
        public SMS_Data(int booking_id,string method)
        {
            DB.clsDB db = new DB.clsDB();
            content = new Dictionary<string,string>();
            db.AddParameter("BOOKING_ID", booking_id);
            DataSet ds = db.ExecuteSelect(method, System.Data.CommandType.StoredProcedure, 30);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                details_list = ds.Tables[0].AsEnumerable().Select(dr => ds.Tables[0].Columns.Cast<DataColumn>().ToDictionary(dc => dc.ColumnName, dc => dr[dc])).ToList();
                foreach (Dictionary<string, object> elements in details_list)
                {
                    foreach (KeyValuePair<string, object> entry in elements)
                    {
                        content[entry.Key] = entry.Value.ToString();
                    }
                }
            }
            else
            {
                throw new System.Exception("Booking ID Not Found!");
            }
        }
        public SMS_Data(Dictionary<string,string> content)
        {
            this.content = content;
        }
        public SMS_Data(string text)
        {
            this.text = text;
        }
        private List<string> get_template_keys(string template)
        {
            List<string> _template_keys = new List<string>();
            MatchCollection matchList;
            matchList = Regex.Matches(template, "[=][$][a-zA-Z_]+");
            _template_keys.AddRange(matchList.Cast<Match>().Select(match => match.Value).Distinct().ToList());
            matchList = Regex.Matches(template, "[[][a-zA-Z_]+[]]");
            _template_keys.AddRange(matchList.Cast<Match>().Select(match => match.Value).Distinct().ToList());
            return _template_keys;
        }
        public string prepare_booking_sms(string sms_template)
        {
            text = sms_template;
            List<string> template_keys = get_template_keys(text);
            foreach (string tkey in template_keys)
            {
                string key = tkey.Trim( new Char[] { '#', '=', '$', ']', '[' } );
                if (content.ContainsKey(key))
                {
                    if (text != null)
                    {
                        text = text.Replace(tkey, content[key] as string);
                    }
                }
                else
                {
                    throw new System.Exception("Template data " + key + " not found");
                }
            }
            return text;
        }
    }
}