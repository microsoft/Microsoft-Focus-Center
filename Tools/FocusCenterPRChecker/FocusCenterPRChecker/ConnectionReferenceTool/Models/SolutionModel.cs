// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace FocusCenterPRChecker.ConnectionReferenceTool.Models
{
    public class SolutionModel
    {
        public HashSet<string> Flows { get; set; }

        public bool IsAdded { get; set; } = false;
    }
}
