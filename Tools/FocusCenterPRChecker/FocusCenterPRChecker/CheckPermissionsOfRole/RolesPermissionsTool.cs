using FocusCenterPRChecker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FocusCenterPRChecker.DuplicatesComponentTool.Enums;
using FocusCenterPRChecker.DuplicatesComponentTool.Models;
using FocusCenterPRChecker.Managers;

namespace FocusCenterPRChecker.CheckPermissionsOfRole
{
    public static class RolesPermissionsTool
    {
        // Among all changed components, filter component for roles and process the path
        public static async Task CheckPermissionsOfRoles(List<ComponentModel> changedComponent)
        {
            var changedRoles = changedComponent
                .Where(component => component.ComponentFolder == ComponentFolder.Roles.ToString())
                .Select(role => $"{ConfigManager.PathToRepository}{role.Path.Replace("/", "\\")}")
                .ToList();

            if (changedRoles.Count > 0)
            {
                await ProcessChangedRoles(changedRoles);
            }
            else
            {
                Console.WriteLine("\tThere are no changed Roles");
            }
        }

        // for each of changed roles process the role
        private static async Task ProcessChangedRoles(List<string> changedRolesPath)
        {
            foreach (var changedRolePath in changedRolesPath)
            {
                var resultTable = SecurityRole.ProcessRole(changedRolePath);

                Console.WriteLine($"Role table Comment: {resultTable}");

                PullRequest.CommentsToPost.Add(resultTable);
            }
        }
    }
}
