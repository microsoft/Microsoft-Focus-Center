// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.using System;

namespace FocusCenterPRChecker.DuplicatesComponentTool.Models
{
    public class SearchResponseModel
    {
        public int Count { get; set; }
        public FoundComponent[] Results { get; set; }
    }

    public class FoundComponent
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}
