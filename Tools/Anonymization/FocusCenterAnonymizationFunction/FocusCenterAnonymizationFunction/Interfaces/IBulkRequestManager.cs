using AnonymizationFunctionApp.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnonymizationFunctionApp.Interfaces
{
    public interface IBulkRequestManager : IDisposable
    {
        ICrmConnectionManager ConnectionManager { get; }
        string EntityName { get; set; }
        Task SendRequests(IEnumerable<OrganizationRequest> requests);
    }
}
