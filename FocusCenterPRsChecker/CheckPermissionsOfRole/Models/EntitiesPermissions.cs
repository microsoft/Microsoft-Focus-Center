using System.Collections.Generic;

namespace FocusCenterPRsChecker.CheckPermissionsOfRole.Models
{
    public class EntitiesPermissions
    {
        public string EnityName { get; set; }

        public IEnumerable<PermissionLevel> Permissions { get; set; }
    }
}
