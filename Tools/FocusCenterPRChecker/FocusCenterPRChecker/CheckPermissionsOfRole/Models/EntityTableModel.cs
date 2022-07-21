// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.using System;

namespace FocusCenterPRChecker.CheckPermissionsOfRole.Models
{
    public class EntityTableModel
    {
        public string EnityName { get; set; }

        public string Create { get; set; }
        public string Read { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
    }
}
