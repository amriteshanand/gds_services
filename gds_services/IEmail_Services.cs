using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
namespace gds_services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEmail_Services" in both code and config file together.
    [ServiceContract]
    public interface IEmail_Services
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        string test();
        /**/

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "send_email?type={type}&booking_id={booking_id}&mobile_no={mobile_no}")]
        SMS_Response send_sms(string type, int booking_id, string mobile_no, string key);


    }

}
