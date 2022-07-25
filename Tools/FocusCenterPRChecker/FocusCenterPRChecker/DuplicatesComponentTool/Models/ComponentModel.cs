// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace FocusCenterPRChecker.DuplicatesComponentTool.Models
{
    public class ComponentModel
    {
        public string ComponentName { get; set; }
        public string SolutionName { get; set; }
        public string Path { get; set; }
        public string ComponentFolder { get; set; }
        public bool IsNewComponent { get; set; }
    }
}
