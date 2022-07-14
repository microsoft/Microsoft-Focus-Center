using System.Collections.Generic;

namespace FocusCenterPRsChecker.ConnectionReferenceTool.Models
{
    public class SolutionModel
    {
        public HashSet<string> Flows { get; set; }

        public bool IsAdded { get; set; } = false;
    }
}
