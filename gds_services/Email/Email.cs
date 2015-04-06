﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace gds_services.Email
{
    public class Email
    {
        public
            NameValueCollection config = null;
            string type=null;
            string subject = null;
            Dictionary<string, object> content = null;
            Dictionary<string, object> attachments = null;
            string body = null;
            string attachment_file = null;
            List<string> temp_attachments = new List<string>();

        // Constructor for email class.
        public Email(string type, string subject, Dictionary<string, object> content, Dictionary<string, object> attachments)
        {
            this.type = type;
            this.subject = subject;
            this.content = content;
            this.attachments = attachments;
            load_config();
            set_defaults();
            prepare_body(content);
            attach_file(attachments);
        }
        
        // loads configuration based on type.
        private void load_config()
        {
            this.config = (NameValueCollection)ConfigurationManager.GetSection(this.type);
        }

        // Gets default subject based on type.
        private void get_email_subject(string type)
        {
            if (this.subject == null || this.subject.Trim().Length == 0)
            {
                this.subject = this.config["default_subject"].ToString();
            }
            return;
        }
        
        //Sets all Defaults when user does not provide entry.
        private void set_defaults()
        {
            get_email_subject(this.type);
            return;
        }
        
        
        private string get_template(string template_name)
        {
            string template = "";
            switch (template_name)
            {
                case "Booking_Email.html":
                case "Cancellation.html":
                case "Pickup_Mismatch.html":
                    template = new StreamReader(this.config["directory"].ToString() + template_name).ReadToEnd();
                    break;
                case "Blank_Email.html":
                    template = "#CONTENT#";
                    break;
                case "Table_Email.html":
                    template = "table";
                    break;
                default:
                    throw new System.Exception("Invalid Template Type");
            }
            if (template == null || template.Trim().Length == 0)
            {
                throw new System.Exception("Template Not Found!");
            }
            return template;
        }

        //Get all keys capitalletters and _ quoted between #
        private List<string> get_template_keys(string template)
        {
            List<string> _template_keys = new List<string>();
            MatchCollection matchList = Regex.Matches(template, "#[A-Z_]+#");
            _template_keys = matchList.Cast<Match>().Select(match => match.Value).Distinct().ToList();
            return _template_keys;
        }

        // Takes in content and creates a table
        private string convert_to_html(Dictionary<string, object> content)
        { 
            string html = @"<table style=""border:1px solid black"">";
            //add header row
            html += "<tr>";
            for (int i = 0; i < content.Count; i++)
                html += @"<th style=""border:1px solid black"">" + content.Keys + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < content.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < content.Values.Count; j++)
                    html += @"<td style=""border:1px solid black"">" + content.Values.ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        // Takes in content,template and patches them.
        public string prepare(string template, Dictionary<string, object> content)
        {
            string prepared_data = template;
            if (prepared_data == "table")
            {
                prepared_data = convert_to_html(content);
            }
            else
            {
                List<string> template_keys = get_template_keys(template);
                foreach (string tkey in template_keys)
                {
                    string key = tkey.Trim(new Char[] { '#', '=', '$', ']', '[' });
                    if (content.ContainsKey(key))
                    {
                        if (prepared_data != null)
                        {
                            prepared_data = prepared_data.Replace(tkey, content[key].ToString());
                        }
                    }
                    else
                    {
                        throw new System.Exception("Template data " + key + " not found");
                    }
                }
            }
            return prepared_data;
        }
        
        //Prepares Body
        private void prepare_body(Dictionary<string, object> content)
        {
            string body_template_name = this.config["content_template"].ToString();
            string body_template = get_template(body_template_name);
            this.body = prepare(body_template, content);
        }

        // Attach File 
        private void attach_file(Dictionary<string, object> render_data)
        {
            try
            {
                string file_name = this.config["default_attachment"].ToString();
                create_attachment(file_name, render_data);
            }
            catch
            {
            }
        }

        //Creates Attachment
        private string create_attachment(string file_name, Dictionary<string, object> render_data)
        {
            StreamWriter fp;
            string directory = this.config["directory"].ToString();
            string attachment_template_name = this.config["attachment_template"].ToString();
            string attachment_template = get_template(attachment_template_name);
            string attachment_content = prepare(attachment_template, render_data);
            string file_path = directory + file_name;
            if (File.Exists(file_path))
            {
                File.Delete(file_path);
            }
            fp = File.CreateText(file_path);
            fp.WriteLine(attachment_content);
            fp.Flush();
            fp.Close();
            temp_attachments.Add(file_path);
            return file_path;
        }

        //sends the message.
        public bool send_email(int booking_id, string to_email_ids, string cc_email_ids)
        {
            Email_Response response = new Email_Response();
            Utils.clsLogger logger = new Utils.clsLogger();
            MailMessage message = new MailMessage();
            try
            {
                string smtpServer = ConfigurationManager.AppSettings["SMTPServer"].ToString();
                string smtpPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();
                string fromEmailId = this.config["sender_email_id"].ToString();
                string fromEmailPassword = this.config["sender_password"].ToString();
                SmtpClient smtp = new SmtpClient(smtpServer, Convert.ToInt32(smtpPort));
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Timeout = 100000;
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential(fromEmailId, fromEmailPassword);
                message.From = new MailAddress(fromEmailId);
                message.Subject = this.subject;
                string[] to_email_id_list = to_email_ids.Split(',');
                foreach (string to_email_id in to_email_id_list)
                    message.To.Add(new MailAddress(to_email_id));
                if (cc_email_ids == null || cc_email_ids.Trim().Length == 0)
                {
                    cc_email_ids = this.config["default_cc_email"].ToString();
                }
                else
                {
                    cc_email_ids = cc_email_ids + "," + this.config["default_cc_email"].ToString();
                }
                string[] cc_email_id_list = cc_email_ids.Split(',');
                foreach (string cc_email_id in cc_email_id_list)
                    message.To.Add(new MailAddress(cc_email_id));
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Body = this.body;
                message.IsBodyHtml = true;
                foreach (string temp_attachment in temp_attachments)
                    message.Attachments.Add(new Attachment(temp_attachment));
                smtp.Send(message);
                response.status = true;
            }
            catch (Exception ex)
            {
                logger.log("error", new Dictionary<string, object>
                {   {"booking_id",booking_id},
                    {"email_ids",to_email_ids},
                    {"cc_email_ids",cc_email_ids},
                    {"type",type}
                }, ex.ToString());
                response.status = false;
                response.error = ex.ToString();
            }
            finally
            {
                if (message != null)
                    message.Dispose();
            }
            //Email.log_email_into_db(type, booking_id, to_email_ids, cc_email_ids, response.error, Convert.ToInt32(response.status));
            return response.status;
        }
        
        //SP not implemented. Not required feature.
        public static void log_email_into_db(string type, int booking_id, string to_email_ids, string cc_email_ids, string error, int is_sent)
        {
            DB.clsDB db = new DB.clsDB();
            db.AddParameter("TYPE", type, 20);
            db.AddParameter("BOOKING_ID", booking_id);
            db.AddParameter("To_Email_IDS", to_email_ids, 200);
            db.AddParameter("CC_Email_IDS", cc_email_ids, 200);
            db.AddParameter("SEND_TIME", DateTime.Now);
            db.AddParameter("ERROR", error, 20);
            db.AddParameter("IS_SENT", is_sent);
            db.ExecuteDML("spEmailLog_Insert", CommandType.StoredProcedure, 30);
        }
        ~Email()
        {
            for (int i = 0; i < temp_attachments.Count; i++)
            {
                File.Delete(temp_attachments[i]);
            }
        }   
    }
}