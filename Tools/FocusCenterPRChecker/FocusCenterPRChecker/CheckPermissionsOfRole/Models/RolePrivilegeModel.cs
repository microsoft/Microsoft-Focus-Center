// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace FocusCenterPRChecker.CheckPermissionsOfRole.Models
{
    public class RolePrivilegeModel
    {
        public string RolePrivilegeName { get; set; }

        public string LevelPermission { get; set; }

        public string EntityName { get; set; }

        public string PermissionType { get; set; }
    }
}
