using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusCenterPRChecker.CheckFieldInFlows.Models
{
    public class ActionModel
    {
        public Inputs Inputs { get; set; }
    }

    public class Inputs
    {
        public Host Host { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class Host
    {
        public string ApiId { get; set; }
        public string ConnectionName { get; set; }
        public string OperationId { get; set; }
    }
}
