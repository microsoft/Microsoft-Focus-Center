using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymizationFunctionApp.Models
{
    public class CrmCredentials : IEqualityComparer<CrmCredentials>
    {
        public DateTime CreatedAt { get; set; }

        public CrmCredentials()
        {
            CreatedAt = DateTime.Now;
        }

        public string Host { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool Equals(CrmCredentials x, CrmCredentials y)
        {
            return x?.Host == y?.Host && x?.ClientId == y?.ClientId && x?.ClientSecret == y?.ClientSecret;
        }

        public int GetHashCode(CrmCredentials obj)
        {
            return obj.Host.GetHashCode() ^ obj.ClientId.GetHashCode();
        }
    }
}
