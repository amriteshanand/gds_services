using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
namespace gds_services.Utils
{
    public class HTTP
    {
        public bool use_proxy= false;
        public string proxy = null; //"http://216.12.192.201:808";        
        public HTTP()
        { 

        }
        public HTTP(string proxy)
        {
            this.proxy = proxy;
            use_proxy = true;
        }
        public string POST(string url,string request_body) 
        {
            HttpWebRequest http_req = (HttpWebRequest)WebRequest.Create(url);
            http_req.Method = "POST";
            if (this.use_proxy)
            {
                WebProxy myProxy = new WebProxy();
                myProxy.Address = new Uri(proxy);
                http_req.Proxy = myProxy;
            } 
            
            byte[] req_bytes = System.Text.Encoding.ASCII.GetBytes(request_body);
            http_req.ContentLength = req_bytes.Length;
            Stream dataStream = http_req.GetRequestStream();
            dataStream.Write(req_bytes, 0, req_bytes.Length);
            dataStream.Flush();
            dataStream.Close();

            HttpWebResponse http_resp = (HttpWebResponse)http_req.GetResponse();
            if (http_resp.StatusCode==HttpStatusCode.OK){
                long n=http_resp.ContentLength;
                byte[] tempBuffer = new byte[n];
                http_resp.GetResponseStream().Read(tempBuffer, 0, (int)n);
                string res = System.Text.Encoding.UTF8.GetString(tempBuffer);
                return res;
            }else{
                throw new System.Exception("HTTP Error: "+http_resp.StatusDescription);
            }
            
        }
        public string GET(string url)
        {
            HttpWebRequest http_req = (HttpWebRequest)WebRequest.Create(url);
            http_req.Method = "GET";
            if (this.use_proxy)
            {
                WebProxy myProxy = new WebProxy();
                myProxy.Address = new Uri(proxy);
                http_req.Proxy = myProxy;
            }           
            HttpWebResponse http_resp = (HttpWebResponse)http_req.GetResponse();
            if (http_resp.StatusCode == HttpStatusCode.OK)
            {
                string res;
                StringBuilder respBody = new StringBuilder();
                byte[] buf = new byte[8192];
                Stream respStream = http_resp.GetResponseStream();
                int count = 0;
                do
                {
                    count = respStream.Read(buf, 0, buf.Length);
                    if (count != 0)
                        respBody.Append(Encoding.ASCII.GetString(buf, 0, count));
                }
                while (count > 0);
                res = respBody.ToString();
                http_resp.Close();
                return res;
            }
            else
            {
                throw new System.Exception("HTTP Error: " + http_resp.StatusDescription);
            }
        }
        
    }
}