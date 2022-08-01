using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnonymizationFunctionApp.Interfaces
{
    public interface ICrmConnectionManager : IDisposable 
    {
        ValueTask<ServiceClient> GetClient(TimeSpan timeout, string host = "");
        void Invalidate();
        void Renew();
        string EntityName { get; set; }
    }
}
