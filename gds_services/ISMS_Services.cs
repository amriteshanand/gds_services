using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace gds_services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISMS_Services" in both code and config file together.
    [ServiceContract]
    public interface ISMS_Services
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        string test();
        /**/

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "send_sms?type={type}&booking_id={booking_id}&mobile_no={mobile_no}")]
        SMS_Response send_sms(string type, int booking_id, string mobile_no);


    }


    /*Service Contract*/

    [DataContract]
    [KnownType(typeof(Op_Response))]
    public class Response
    {
        public class Result
        {
        }

        protected bool bool_status = false;
        protected string str_error = "";
        protected Result obj_result;
        protected virtual object get_default_result()
        {
            return "abc";
        }
        protected virtual bool set_default_result(object obj)
        {
            return false;
        }
        [DataMember]
        public bool status
        {
            get { return this.bool_status; }
            set { this.bool_status = value; }
        }

        [DataMember]
        public string error
        {
            get { return this.str_error; }
            set { this.str_error = value; }
        }
    }

    public class Op_Response : Response
    {
        public class Op_Result
        {
            public string abc = "test";
        }
        public Op_Result result = new Op_Result();
    }
}
