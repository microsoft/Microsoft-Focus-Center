using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FocusCenterPRChecker.ConnectionReferenceTool
{
    public class ConnectionReferences
    {
        private Dictionary<string, ConnectionReference> _connectionReferenceModels; //key - connectionReferenceName

        public ConnectionReferences()
        {
            _connectionReferenceModels = new Dictionary<string, ConnectionReference>();
        }

        // if connection name is present then add into its usage else add the connection name
        public void AddUsage(string connectionName, string flowName, string solutionName)
        {
            if (_connectionReferenceModels.ContainsKey(connectionName))
            {
                _connectionReferenceModels[connectionName].AddUsage(flowName, solutionName);
            }
            else
            {
                _connectionReferenceModels.Add(connectionName, new ConnectionReference(connectionName, flowName, solutionName));
            }
        }
        
        // set connection refernce IsAdded to true if its present in solution
        public void CheckConnectionReferencesInSolution(string solutionName, List<XElement> connectionReferencesInSolution)
        {
            foreach (var connectionReference in _connectionReferenceModels)
            {
                bool isPresentInSolution = connectionReferencesInSolution.FirstOrDefault(c => c.Attribute("connectionreferencelogicalname")?.Value == connectionReference.Key) != null;

                if (isPresentInSolution)
                {
                    if (connectionReference.Value.Solutions.ContainsKey(solutionName))
                    {
                        connectionReference.Value.Solutions[solutionName].IsAdded = true;
                    }
                }
            }
        }

        public Dictionary<string, ConnectionReference> GetConnectionRefrences()
        {
            return _connectionReferenceModels;
        }

        // get conenction references which have conatins same solution name
        public HashSet<KeyValuePair<string, ConnectionReference>> GetDuplicateConnectionReferences(List<string> solutionsOfChanges)
        {
            HashSet<KeyValuePair<string, ConnectionReference>> result = new HashSet<KeyValuePair<string, ConnectionReference>>();

            foreach (var solution in solutionsOfChanges)
            {
                var duplicatesList = _connectionReferenceModels.Where(c => c.Value.IsDuplicateSolutionUsage() && c.Value.Solutions.ContainsKey(solution)).ToList();

                result.UnionWith(duplicatesList);
            }

            return result;
        }

        // Add missing connection by checking if IsAdded is false and solutionsOfChanges contains solution 
        public List<KeyValuePair<string, string>> GetMissingConnectionReferences(List<string> solutionsOfChanges)
        {
            List<KeyValuePair<string, string>> missingConnectionReferences = new List<KeyValuePair<string, string>>();

            foreach (var connectionReference in _connectionReferenceModels)
            {
                foreach (var solution in connectionReference.Value.Solutions)
                {
                    if (!solution.Value.IsAdded && solutionsOfChanges.Contains(solution.Key))
                    {
                        missingConnectionReferences.Add(new KeyValuePair<string, string>(connectionReference.Key, string.Join("<br/><br/>", solution.Value.Flows)));
                    }
                }
            }

            return missingConnectionReferences;
        }
    }
}
