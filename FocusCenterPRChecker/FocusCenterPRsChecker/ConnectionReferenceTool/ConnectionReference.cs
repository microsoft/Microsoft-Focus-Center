using System.Collections.Generic;
using FocusCenterPRsChecker.ConnectionReferenceTool.Models;

namespace FocusCenterPRsChecker.ConnectionReferenceTool
{
    public class ConnectionReference
    {
        public string ConnectionReferenceName { get; set; }

        public Dictionary<string, SolutionModel> Solutions { get; set; } // key - SolutionName, value - Model: HashSet<string> flows, bool isAdded   

        public ConnectionReference (string connectionReferencename, string flowName, string solutionName)
        {
            ConnectionReferenceName = connectionReferencename;

            Solutions = new Dictionary<string, SolutionModel>() { [solutionName] = new SolutionModel { Flows = new HashSet<string> {flowName } } };

        }

        //Add flowName and solution to the existing connection reference
        public void AddUsage(string flowName, string solutionName)
        {
            if (Solutions.ContainsKey(solutionName))
            {
                Solutions[solutionName].Flows.Add(flowName);
            }
            else
            {
                Solutions.Add(solutionName, new SolutionModel { Flows = new HashSet<string> { flowName } });
            }
        }

        //Get flows from solutions
        public HashSet<string> GetFlowsFromSolutions()
        {
            HashSet<string> flows = new HashSet<string>();

            foreach (var solution in Solutions)
            {
                flows.UnionWith(solution.Value.Flows);
            }

            return flows;
        }

        //check if connection Reference is used in several solutions
        public bool IsDuplicateSolutionUsage()
        {
            return Solutions.Count > 1;
        }
    }
}
