using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System;
using gds_services;

namespace gds_services.DB
{
    class clsDB
    {
        SqlCommand cmd;
        SqlCommand cmdFilter;
        string strConnString;
        /*
        string strConnStringSlave;
        string strConnStringCRS;
        string strConnStringBitla;
        string strAbhibusConnString;
        string strKalladaConnString;
        string strNeetaConnString;
        string strConnStringNeeta = "";
        string strTicketGooseConnString;
        string strKPNConnString;
        string strHindusthanConnString;
        string strConnStringMantis2011;
        string strConnStringPaulo = "";
        string strHermesConnString = "";
        string strConnStringPurple = "";
        string strConnStringPatel = "";
        string strDurgambaConnString = "";
        string strConnStringKesineni = "";
        string strConnStringParveen = "";
        string strOgiveConnString = "";
        string strConnStringOlivea = "";
        string strVishalConnString = "";
        string strSriDurgambaConnString = "";
        string strAbhibusOperatorsConnString = "";
        string strConnStringRSRTC = "";
        string strConnStringUPSRTC = "";
        string strConnStringHRTC = "";
        string strConnStringMSRTC = "";
        string strConnStringTicketEngine = "";
         */ 
        Utils.clsLogger logger;
        public clsDB()
        {
            strConnString = System.Configuration.ConfigurationManager.AppSettings["GDSConnString"].ToString();
            /*
            strConnStringSlave = System.Configuration.ConfigurationManager.AppSettings["GDSSlaveConnString"].ToString();
            strConnStringCRS = System.Configuration.ConfigurationManager.AppSettings["CRSConnString"].ToString();
            strConnStringBitla = System.Configuration.ConfigurationManager.AppSettings["BitlaConnString"].ToString();
            strAbhibusConnString = System.Configuration.ConfigurationManager.AppSettings["AbhibusConnectionString"].ToString();
            strKalladaConnString = System.Configuration.ConfigurationManager.AppSettings["KalladaConnectionString"].ToString();
            strTicketGooseConnString = System.Configuration.ConfigurationManager.AppSettings["TicketGooseConnectionString"].ToString();
            strKPNConnString = System.Configuration.ConfigurationManager.AppSettings["KPNConnectionString"].ToString();
            strHindusthanConnString = System.Configuration.ConfigurationManager.AppSettings["HindusthanConnectionString"].ToString();
            strNeetaConnString = System.Configuration.ConfigurationManager.AppSettings["NeetaConnectionString"].ToString();
            strConnStringNeeta = System.Configuration.ConfigurationManager.AppSettings["NeetaConnString"].ToString();
            strConnStringMantis2011 = System.Configuration.ConfigurationManager.AppSettings["MantisConnString"].ToString();
            strHermesConnString = System.Configuration.ConfigurationManager.AppSettings["HermesConnString"].ToString();
            strConnStringPatel = System.Configuration.ConfigurationManager.AppSettings["PatelConnString"].ToString();
            strConnStringPurple = System.Configuration.ConfigurationManager.AppSettings["PurpleConnString"].ToString();
            strDurgambaConnString = System.Configuration.ConfigurationManager.AppSettings["DurgambaConnectionString"].ToString();
            strConnStringPaulo = System.Configuration.ConfigurationManager.AppSettings["PauloConnectionString"].ToString();
            strConnStringParveen = System.Configuration.ConfigurationManager.AppSettings["ParveenConnectionString"].ToString();
            strOgiveConnString = System.Configuration.ConfigurationManager.AppSettings["OgiveConnectionString"].ToString();
            strConnStringKesineni = System.Configuration.ConfigurationManager.AppSettings["KesineniConnectionString"].ToString();
            strConnStringOlivea = System.Configuration.ConfigurationManager.AppSettings["OliveaConnectionString"].ToString();
            strVishalConnString = System.Configuration.ConfigurationManager.AppSettings["VishalConnString"].ToString();
            strSriDurgambaConnString = System.Configuration.ConfigurationManager.AppSettings["SriDurgambaConnectionString"].ToString();
            strAbhibusOperatorsConnString = System.Configuration.ConfigurationManager.AppSettings["AbhibusOperatorsConnectionString"].ToString();
            strConnStringRSRTC = System.Configuration.ConfigurationManager.AppSettings["RSRTCConnString"].ToString();
            strConnStringUPSRTC = System.Configuration.ConfigurationManager.AppSettings["UPSRTCConnString"].ToString();
            strConnStringHRTC = System.Configuration.ConfigurationManager.AppSettings["HRTCConnString"].ToString();
            strConnStringMSRTC = System.Configuration.ConfigurationManager.AppSettings["MSRTCConnString"].ToString();
            // by : sWaRtHi
            strConnStringTicketEngine = System.Configuration.ConfigurationManager.AppSettings["TicketEngineConnString"].ToString();
            */
            logger = new Utils.clsLogger();
            StartNewCommand();
        }       

        public void StartNewCommand()
        {
            cmd = new SqlCommand();
            cmdFilter = new SqlCommand();
        }

        public void AddParameter(string strParamName, string strParamValue, int intParamSize)
        {
            cmd.Parameters.Add("@" + strParamName, SqlDbType.VarChar, intParamSize).Value = strParamValue;
        }
        public void AddParameter(string strParamName, int intParamValue)
        {
            cmd.Parameters.Add("@" + strParamName, SqlDbType.Int).Value = intParamValue;
        }
        public void AddParameter(string strParamName, bool blnParamValue)
        {
            cmd.Parameters.Add("@" + strParamName, SqlDbType.Bit).Value = blnParamValue;
        }
        public void AddParameter(string strParamName, DateTime dtParamValue)
        {
            cmd.Parameters.Add("@" + strParamName, SqlDbType.DateTime).Value = dtParamValue;
        }
        public void AddParameter(string strParamName, decimal dclParamValue)
        {
            cmd.Parameters.Add("@" + strParamName, SqlDbType.Decimal).Value = dclParamValue;
        }
        public void AddParameter(string strParamName, double dblParamValue)
        {
            cmd.Parameters.Add("@" + strParamName, SqlDbType.Decimal).Value = dblParamValue;
        }
        public void AddParameter(string strParamName, DataTable tablePara)
        {
            cmd.Parameters.Add("@" + strParamName, SqlDbType.Structured).Value = tablePara;
        }
        public DataTable FilterDataTable(DataTable dt, string strFilterQuery, string strSortBy)
        {
            DataTable dtTemp;
            DataRow[] dr;
            dr = dt.Select(strFilterQuery, strSortBy);
            dtTemp = dt.Clone();

            foreach (DataRow r in dr)
            {
                dtTemp.ImportRow(r);
            }
            return dtTemp;
        }

        public int ExecuteDML(string strSQL, CommandType cmdType, int intTimeout)
        {
            int status = 0;
            SqlConnection conn = new SqlConnection(strConnString);

            try
            {
                cmd.CommandText = strSQL;
                cmd.CommandType = cmdType;
                cmd.CommandTimeout = intTimeout;
                conn.Open();
                cmd.Connection = conn;
                status = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            catch (System.Exception ex)
            {
                status = -1;
                try
                {
                    logger.log("error", strSQL, ex.Message);
                }
                catch (System.Exception)
                {
                }
            }
            finally
            {
                cmd.Cancel();
                conn.Close();
                conn.Dispose();
            }
            return status;
        }

        public DataSet ExecuteSelect(string strSQL, CommandType cmdType, int intTimeout)
        {
            DataSet ds = new DataSet();
            SqlDataAdapter adp = new SqlDataAdapter();
            SqlConnection conn = new SqlConnection(strConnString);
            try
            {
                cmd.CommandText = strSQL;
                cmd.CommandType = cmdType;
                cmd.CommandTimeout = intTimeout;
                adp.SelectCommand = cmd;
                conn.Open();
                cmd.Connection = conn;
                adp.Fill(ds);
                cmd.Parameters.Clear();
            }
            catch (System.Exception e)
            {
                ds = null;
                try
                {
                    logger.log("error", strSQL, e.Message);
                }
                catch (System.Exception)
                {
                }

                //prateek- line below should be uncommented.
                //throw e;
            }
            finally
            {
                adp.Dispose();
                cmd.Cancel();
                conn.Close();
                conn.Dispose();
            }
            return ds;
        }

    }
}