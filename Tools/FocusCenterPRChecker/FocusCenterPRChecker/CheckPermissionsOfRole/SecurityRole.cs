// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.using System;

using FocusCenterPRChecker.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FocusCenterPRChecker.CheckPermissionsOfRole.Models;
using FocusCenterPRChecker.DuplicatesComponentTool;
using FocusCenterPRChecker.Helpers;

namespace FocusCenterPRChecker.CheckPermissionsOfRole
{
    public class SecurityRole
    {
        private static string _roleName;

        // Parse roles XML and segregate role into entity and  other privileges
        public static string ProcessRole(string pathToRole)
        {
            if (!File.Exists(pathToRole))
            {
                Console.WriteLine($"\n\tFile {pathToRole} doesn't exist");
                return "";
            }

            XElement roleFile = XElement.Load(pathToRole);

            _roleName = roleFile.Attribute("name")?.Value;

            Console.WriteLine($"\n\tProcessing {_roleName} security role");

            Console.WriteLine("\tParsing xml file to get permissions by entity");

            var rolePrivileges = ParseSecurityRoleFile(roleFile);

            var privilegesByEntity = GetPrivilegesByEntity(rolePrivileges.Where(entity => !string.IsNullOrEmpty(entity.EntityName))).ToList();

            var othersPrivileges = privilegesByEntity.FirstOrDefault(entity => entity.EnityName == "Others")?.Permissions.ToList();

            var excludedEntities = ExcludeEntities(privilegesByEntity);

            var entityTables = GetEntityTables(excludedEntities).ToList();

            Console.WriteLine("\tGetting rows for table");

            var rows = GetRowsForEntityTable(entityTables, othersPrivileges);

            Console.WriteLine("\tGetting result table");

            return GetTableForRole(rows);
        }

        // get role privilege name, level permission, entity name and permission type for each role
        private static List<RolePrivilegeModel> ParseSecurityRoleFile(XElement roleFile)
        {
            var privileges = roleFile.Descendants("RolePrivilege").ToList();

            List<RolePrivilegeModel> rolePrivilegeModels = new List<RolePrivilegeModel>();

            privileges.ForEach(role => rolePrivilegeModels.Add(ParseRoleRow(role)));

            return rolePrivilegeModels;
        }

        // Exclude entities mentioned in Exclusion list path
        private static IEnumerable<EntitiesPermissions> ExcludeEntities(List<EntitiesPermissions> entitiesPermissions)
        {
            ExclusionList exclusionList = new ExclusionList(ConfigManager.ExclusionListPath);

            var excludedSpecificEntitie = entitiesPermissions
                .Where(entity => entity.EnityName != "Others")
                .Where(entityPermission => !exclusionList.ContainsEntity(entityPermission.EnityName))
                .Where(pr => !(pr.Permissions.Count() == 1 && pr.Permissions.First().Permission == "Read"));

            return excludedSpecificEntitie;
        }

        // Extract the entity name and permission type from Roles xml
        private static RolePrivilegeModel ParseRoleRow(XElement roleXElement)
        {
            Regex regex = new Regex(@"^prv(?<permission>Create|Read|Write|Delete|Assign|AppendTo|Append|Print|Share)(?<entity>.*)$");

            var rolePrivilege = new RolePrivilegeModel() { RolePrivilegeName = roleXElement.Attribute("name")?.Value, LevelPermission = roleXElement.Attribute("level")?.Value };

            if (regex.IsMatch(rolePrivilege.RolePrivilegeName ?? string.Empty))
            {
                if (IsNeedPermission(regex.Match(rolePrivilege.RolePrivilegeName).Groups["permission"].Value))
                {
                    rolePrivilege.EntityName = regex.Match(rolePrivilege.RolePrivilegeName).Groups["entity"].Value;

                    rolePrivilege.PermissionType = regex.Match(rolePrivilege.RolePrivilegeName).Groups["permission"].Value;
                }
            }
            else
            {
                if (rolePrivilege.RolePrivilegeName != null)
                {
                    rolePrivilege.PermissionType = rolePrivilege.RolePrivilegeName.Replace("prv", "");
                    rolePrivilege.EntityName = "Others";
                }
            }

            return rolePrivilege;

        }

        private static IEnumerable<EntitiesPermissions> GetPrivilegesByEntity(IEnumerable<RolePrivilegeModel> rolePrivilegeModels)
        {
            var entitiesPrivileges = rolePrivilegeModels
                .GroupBy(role => role.EntityName)
                .Select(group => new EntitiesPermissions
                {
                    EnityName = group.Key,
                    Permissions = group.Select(x => new PermissionLevel
                    {
                        Permission = x.PermissionType,
                        Level = x.LevelPermission
                    }).ToList()
                });

            return entitiesPrivileges;
        }

        private static bool IsNeedPermission(string permission)
        {
            if (permission == "Create" || permission == "Read" || permission == "Write" || permission == "Delete")
                return true;

            return false;
        }

        // Add permission level for each entity and its respective privilege
        private static IEnumerable<EntityTableModel> GetEntityTables(IEnumerable<EntitiesPermissions> entitiesPermissions)
        {
            return entitiesPermissions.Select(entity => new EntityTableModel()
            {
                EnityName = entity.EnityName,
                Create = entity.Permissions.FirstOrDefault(x => x.Permission == "Create")?.Level ?? string.Empty,
                Read = entity.Permissions.FirstOrDefault(x => x.Permission == "Read")?.Level ?? string.Empty,
                Update = entity.Permissions.FirstOrDefault(x => x.Permission == "Write")?.Level ?? string.Empty,
                Delete = entity.Permissions.FirstOrDefault(x => x.Permission == "Delete")?.Level ?? string.Empty
            }).OrderBy(entity => entity.EnityName).OrderBy(entity => string.IsNullOrEmpty(entity.Delete));
        }

        // add rows with entity name, level against each permission and other privilege as permission-level
        private static List<Row> GetRowsForEntityTable(List<EntityTableModel> entityTableModels, List<PermissionLevel> otherPrivileges)
        {
            var maxLength = Math.Max(entityTableModels?.Count ?? 0, otherPrivileges?.Count ?? 0);

            List<Row> rows = new List<Row>();

            for(int i=0; i < maxLength; i++)
            {
                Row row = new Row();

                if (i < entityTableModels?.Count)
                {
                    row.Cells.AddRange(new List<Cell>()
                    {
                    new Cell(entityTableModels[i].EnityName),
                    new Cell(entityTableModels[i].Create),
                    new Cell(entityTableModels[i].Read),
                    new Cell(entityTableModels[i].Update),
                    new Cell(entityTableModels[i].Delete)
                    });
                }
                else
                {
                    row.Cells.AddRange(new List<Cell>()
                    {
                    new Cell(),
                    new Cell(),
                    new Cell(),
                    new Cell(),
                    new Cell()
                    });
                }

                if (i < otherPrivileges?.Count)
                {
                    row.Cells.Add(new Cell($"{otherPrivileges[i].Permission}-{otherPrivileges[i].Level}"));
                }
                else
                {
                    row.Cells.Add(new Cell());
                }

                rows.Add(row);
            }

            return rows;
        }

        // Create HTML table for roles with upto 4000 rows sorted by Delete privileges
        private static string GetTableForRole(List<Row> rows)
        {
            if (rows.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            sb.Append($"<b>{_roleName}</b><br/><table style='width:{TableStyleSettings.TableWidthSetting}'>");
            sb.Append($"<tr>" +
                $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Entity</th>" +
                $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Create</th>" +
                $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Read</th>" +
                $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Update</th>" +
                $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Delete</th>" +
                $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Others</th></tr>");

            var countOfRows = 0;
            for (int i = 0; i < Math.Min(rows.Count, 4000) && sb.Length < 145000; i++)
            {
                sb.Append(rows[i].ToHTML());
                countOfRows++;
            }

            sb.Append("</table>");

            if(rows.Count >= 4000 || sb.Length >= 145000)
            {
                sb.Insert(0, $"- This comment contains first {countOfRows} permissions for this role sorted by Delete privileges. For more permissions please review the xml file.\n");
            }

            return sb.ToString();
        }
    }
}
