// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.using System;

using System.Collections.Generic;

namespace FocusCenterPRChecker.CheckPermissionsOfRole.Models
{
    public class EntitiesPermissions
    {
        public string EnityName { get; set; }

        public IEnumerable<PermissionLevel> Permissions { get; set; }
    }
}
