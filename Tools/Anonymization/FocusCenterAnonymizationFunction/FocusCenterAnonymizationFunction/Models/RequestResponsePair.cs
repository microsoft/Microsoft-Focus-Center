using Microsoft.Xrm.Sdk;

namespace AnonymizationFunctionApp.Models
{
    public class RequestResponsePair
    {
        public OrganizationRequest Request { get; set; }
        public ExecuteMultipleResponseItem Response { get; set; }
    }
}
