using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusCenterPRsChecker.CheckFieldInFlows.Models
{
    public class FlowJsonModel
    {
        public string FlowName { get; set; }

        public List<KeyValuePair<string, ActionModel>> Actions { get; set; }
    }
}
