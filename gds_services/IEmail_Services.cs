using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Services;
namespace gds_services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEmail_Services" in both code and config file together.
    [ServiceContract]
    public interface IEmail_Services
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "test")]
        string test();
        /**/
        
        [OperationContract]
        [WebInvoke(Method = "POST",RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "send_email")]
        Email_Response send_email(string type, int booking_id, string email_ids, string cc_email_ids, string bcc_email_ids, string subject, string content_dict, string attachments_dict);
    }
}
