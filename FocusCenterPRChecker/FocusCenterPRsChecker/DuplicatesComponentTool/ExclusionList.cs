using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace FocusCenterPRsChecker.DuplicatesComponentTool
{
    public class ExclusionList
    {
        
        private const string FoldersNodesXPath = "/Exclusions/Folders/Folder";
        private const string FileExtensionsNodesXPath = "/Exclusions/FileExtensions/FileExtension";
        private const string ComponentNamesNodesXPath = "/Exclusions/ComponentNames/ComponentName";
        private const string EntitiesNamesNodesXPath = "/Exclusions/Entities/Entity";
 
        private readonly HashSet<string> folders = new HashSet<string>();
        private readonly HashSet<string> fileExtensions = new HashSet<string>();
        private readonly HashSet<string> componentNames = new HashSet<string>();
        private readonly HashSet<string> entities = new HashSet<string>();

        public ExclusionList(string exclusionListFilePath)
        {
            if (!File.Exists(exclusionListFilePath))
            {
                throw new FileNotFoundException("File either not found or it's extension is wrong.", exclusionListFilePath);
            }

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.Load(exclusionListFilePath);

            GetAllFolders(xmlDocument);
            GetAllFileExtensions(xmlDocument);
            GetAllComponentName(xmlDocument);
            GetAllEntities(xmlDocument);
        }

        public bool ContainsFolder(string folderPath)
        {
            var isPathContainsFolder = folders.Select(f => folderPath.ToLower().Contains(f)).ToList();
            return isPathContainsFolder.Contains(true);
        }

        public bool ContainsFileExtension(string fileExtension)
        {
            return fileExtensions.Contains(fileExtension.ToLower());
        }

        public bool ContainsComponentName(string componentName)
        {
            return componentNames.Contains(componentName.ToLower());
        }

        public bool ContainsEntity(string entityName)
        {
            return entities.Contains(entityName.ToLower());
        }

        private void GetAllEntities(XmlDocument xmlDocument)
        {
            foreach (XmlNode folderNode in xmlDocument.SelectNodes(EntitiesNamesNodesXPath))
            {
                XmlAttribute entityNameAttribute = folderNode.Attributes["name"];
                string entityName = entityNameAttribute?.Value.ToLower();

                if (!string.IsNullOrEmpty(entityName) && !entities.Contains(entityName))
                {
                    entities.Add(entityName);
                }
            }
        }

        private void GetAllFolders(XmlDocument xmlDocument)
        {
            foreach (XmlNode folderNode in xmlDocument.SelectNodes(FoldersNodesXPath))
            {
                XmlAttribute folderNameAttribute = folderNode.Attributes["name"];
                string folderName = folderNameAttribute?.Value.ToLower();

                if (!string.IsNullOrEmpty(folderName) && !folders.Contains(folderName))
                {
                    folders.Add(folderName);
                }
            }
        }

        private void GetAllFileExtensions(XmlDocument xmlDocument)
        {
            foreach (XmlNode fileExtensionNode in xmlDocument.SelectNodes(FileExtensionsNodesXPath))
            {
                XmlAttribute fileExtensionAttribute = fileExtensionNode.Attributes["name"];
                string fileExtensionName = fileExtensionAttribute?.Value.ToLower();

                if (!string.IsNullOrEmpty(fileExtensionName) && !fileExtensions.Contains(fileExtensionName))
                {
                    fileExtensions.Add(fileExtensionName);
                }
            }
        }

        private void GetAllComponentName(XmlDocument xmlDocument)
        {
            foreach (XmlNode componentNamesNode in xmlDocument.SelectNodes(ComponentNamesNodesXPath))
            {
                XmlAttribute componentNamesAttribute = componentNamesNode.Attributes["name"];
                string componentName = componentNamesAttribute?.Value.ToLower();

                if (!string.IsNullOrEmpty(componentName) && !componentNames.Contains(componentName))
                {
                    componentNames.Add(componentName);
                }
            }
        }
    }
}
