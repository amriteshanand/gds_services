using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
namespace gds_services.Email
{
    public class Email
    {
        public 
        string type=null;
        string subject = null;
        string content = null;
        NameValueCollection config = null;
        List<string> temp_attachments = new List<string>();
        public Email(string type, string subject, string content)
        { 

        }
        public void attach_file(Dictionary<string, string> render_data)
        {

        }
        public void send(string[] to_email_ids, string[] cc_email_ids)
        { 

        }
        private void load_config()
        {
            this.config = (NameValueCollection)ConfigurationManager.GetSection(this.type);
        }
        private string create_attachment(string file_name,Dictionary<string, string> render_data)
        {
            StreamWriter fp;
            string directory = this.config["directory"].ToString();
            string attachment_template = this.config["attachment_template"].ToString();
            //TODO:Create Content
            string attachment_content = "";
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
        ~Email()
        {
            for (int i = 0; i < temp_attachments.Count; i++)
            {
                File.Delete(temp_attachments[i]);
            }
        }
        
        
    }
}