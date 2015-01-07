using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using log4net;
using log4net.Config;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using System.Configuration;


namespace gds_services.Utils
{
    public class clsLogger
    {
        log4net.ILog logger;
        public long intEventId = -1;
        public clsLogger()
        {
            System.IO.FileInfo fi = new FileInfo(ConfigurationManager.AppSettings["Log4NetLocation"]);
            log4net.Config.XmlConfigurator.Configure(fi);
            logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
        private void log(string fileName, string methodName, string Level, string strEventData)
        {
            string logMessage = "";

            try
            {                
                logMessage = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                logMessage += "\t";
                logMessage += !string.IsNullOrEmpty(Level) ? Level : "INFO";
                logMessage += "\t";
                logMessage += !string.IsNullOrEmpty(fileName) ? fileName : "";
                logMessage += "\t";
                logMessage += !string.IsNullOrEmpty(methodName) ? methodName : "";                
                logMessage += "\t";
                logMessage += !string.IsNullOrEmpty(strEventData) ? Regex.Replace(strEventData,@"\t|\n|\r", "") : "";                                

                switch (Level.ToLower())
                {
                    case "debug":
                        logger.Debug(logMessage);
                        break;
                    case "info":
                        logger.Info(logMessage);
                        break;
                    case "warn":
                        logger.Warn(logMessage);
                        break;
                    case "error":
                        logger.Error(logMessage);
                        break;
                    case "fatal":
                        logger.Fatal(logMessage);
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception)
            {
            }
        }

        public void log(string strLevel, object objEvent)
        {
            StackFrame sf = new StackFrame(1, true);
            string strMethodName = sf.GetMethod().Name;
            string strFname = sf.GetFileName();
            string strFileName = System.IO.Path.GetFileName(strFname);
            string strEvent = JsonConvert.SerializeObject(objEvent);
            this.log(strFileName, strMethodName, strLevel, strEvent);
        }
        public void log(string strLevel, object req ,object res)
        {
            StackFrame sf = new StackFrame(1, true);
            string strMethodName = sf.GetMethod().Name;
            string strFname = sf.GetFileName();
            string strFileName = System.IO.Path.GetFileName(strFname);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["request"] = req;
            dict["response"] = res;
            string strEvent = JsonConvert.SerializeObject(dict);
            this.log(strFileName, strMethodName, strLevel, strEvent);
        }

    }
}