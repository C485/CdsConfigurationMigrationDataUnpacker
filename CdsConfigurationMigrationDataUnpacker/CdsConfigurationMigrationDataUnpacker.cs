using CdsConfigurationMigrationDataUnpacker.Extension;
using CdsConfigurationMigrationDataUnpacker.Helper;
using CdsConfigurationMigrationDataUnpacker.Interface;
using CdsConfigurationMigrationDataUnpacker.Model;
using Dawn;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace CdsConfigurationMigrationDataUnpacker
{
    public class CdsConfigurationMigrationDataUnpacker : IOLayer, IExecute
    {
        private PortalUnpackerArgs PortalExtractorArgs { get; }
        private Config Config { get; }

        public CdsConfigurationMigrationDataUnpacker(PortalUnpackerArgs portalExtractorArgs)
        {
            PortalExtractorArgs = Guard
                .Argument(portalExtractorArgs, nameof(portalExtractorArgs))
                .NotNull()
                .Member(p => p.ConfigFile, u => u.NotNull().NotEmpty().FileExists())
                .Member(p => p.SourceFile, u => u.NotNull().NotEmpty().FileExists())
                .Member(p => p.TargetDirectory, u => u.NotNull().NotEmpty().DirectoryExists());
            Config config = ConfigHelper
                .GetConfig<Config>(PortalExtractorArgs.ConfigFile);
            Config = Guard
                .Argument(config, nameof(config))
                .NotNull()
                .Member(p => p.ConfigItems, u => u.NotNull().MinCount(1));
        }

        public void Execute()
        {
            string xmlContext = GetDataXmlContextFromZip(PortalExtractorArgs.SourceFile);

            XmlDocument doc = new()
            {
                PreserveWhitespace = true
            };
            doc.LoadXml(xmlContext);
            XmlNode entitiesNode = doc.SelectSingleNode("entities");

            foreach (ConfigItem configItem in Config.ConfigItems)
            {
                XmlNode entityNode = entitiesNode.SelectSingleNode($"entity[@name='{configItem.EntityName}']");
                Guard
                    .Argument(entityNode, nameof(entityNode))
                    .NotNull()
                    .Member(p => p.Attributes, u => u.NotNull().Contains("displayname"));
                string displayName = entityNode.Attributes["displayname"].Value;
                string mainFolderName = FormatHelper
                    .ReplaceAllTags(configItem.FolderNameFormat,
                        ("[DisplayName]", displayName),
                        ("[LogicalName]", configItem.EntityName)
                    );
                string mainFolderPath = Directory.CreateDirectory(Path.Combine(PortalExtractorArgs.TargetDirectory, mainFolderName)).FullName;

                foreach (XmlNode item in entityNode.SelectNodes("records/record"))
                {
                    Guard
                        .Argument(item, nameof(item))
                        .NotNull()
                        .Member(p => p.Attributes, u => u.Contains("id"));
                    string recordId = item.Attributes["id"].Value;
                    string name = GetXmlNodeAttributeFromFieldByNames(item, configItem.RecordNameFields, false, false, configItem.RecordNameCombineMode);

                    string fileName = FormatHelper
                        .ReplaceAllTags(configItem.FolderNameFormat,
                            ("[Name]", ClearFileName(name)),
                            ("[Id]", recordId)
                        );
                    string finalFolderPath = mainFolderPath;
                    if (configItem.UnpackToFolder)
                        finalFolderPath = Directory.CreateDirectory(Path.Combine(finalFolderPath, fileName)).FullName;
                    string xmlFilePath = Path.Combine(finalFolderPath, configItem.UnpackToFolder ? "data.xml" : $"{fileName}.xml");

                    DecodeHtmlFieldsValue(configItem, item);
                    SaveAllFiles(configItem, item, recordId, finalFolderPath);

                    File.WriteAllText(xmlFilePath, item.OuterXml);
                }
            }
        }

        private static void DecodeHtmlFieldsValue(ConfigItem configItem, XmlNode item)
        {
            if (configItem.HtmlDecodeFields.Length == 0)
                return;
            foreach (XmlNode singleField in item.SelectNodes("field"))
            {
                string singleFieldsName = singleField.Attributes["name"].Value;
                if (configItem.HtmlDecodeFields.Any(p => p == singleFieldsName))
                    singleField.Attributes["value"].Value = HttpUtility.HtmlDecode(singleField.Attributes["value"].Value);
            }
        }

        private static void SaveAllFiles(ConfigItem configItem, XmlNode item, string recordId, string finalFolderPath)
        {
            if (configItem.FileFields.Length <= 0)
                return;

            uint fileNameCounter = 0;
            foreach (XmlNode singleField in item.SelectNodes("field"))
            {
                string singleFieldsName = singleField.Attributes["name"].Value;
                FileField configFile = Array.Find(configItem.FileFields, p => p.FieldName == singleFieldsName);
                if (configFile == null)
                    continue;
                ++fileNameCounter;
                string content = singleField.Attributes["value"].Value;
                string fieldFileName = string.IsNullOrEmpty(configFile.FileNameFieldName)
                    ? $"{recordId}_{fileNameCounter}"
                    : GetXmlNodeAttributeFromFieldByNames(singleField, new string[] { configFile.FileNameFieldName }, true, false);

                if (string.IsNullOrEmpty(configFile.OverrideFileExtension))
                {
                    if (Path.HasExtension(fieldFileName))
                    {
                        fieldFileName = Path.GetFileNameWithoutExtension(fieldFileName) + configFile.OverrideFileExtension;
                    }
                    else
                    {
                        fieldFileName += configFile.OverrideFileExtension;
                    }
                }
                else if (Path.HasExtension(fieldFileName))
                {
                    fieldFileName += ".data";
                }

                if (!string.IsNullOrEmpty(content))
                {
                    string filePath = Path.Combine(finalFolderPath, fieldFileName);
                    if (configFile.IsBase64 == true)
                    {
                        byte[] data = Convert.FromBase64String(content);
                        File.WriteAllBytes(filePath, data);
                    }
                    else
                    {
                        File.WriteAllText(filePath, content);
                    }
                }
            }
        }

        private static string GetXmlNodeAttributeFromFieldByNames(XmlNode node, string[] fieldNames, bool throwException = false, bool removeItemFromNode = false, NameCombineMode? nameCombineMode = null)
        {
            StringBuilder sb = nameCombineMode == NameCombineMode.CombineAll ? new(512) : null;
            foreach (string nodeName in fieldNames)
            {
                XmlNode fieldNode = node.SelectSingleNode($"field[@name='{nodeName}']");
                if (fieldNode == null)
                    continue;
                if (removeItemFromNode)
                    node.RemoveChild(fieldNode);
                string value = HttpUtility.HtmlDecode(fieldNode.Attributes["value"].Value);
                if (nameCombineMode == NameCombineMode.CombineAll)
                {
                    sb.Append(value);
                    continue;
                }
                return value;
            }
            if (nameCombineMode == NameCombineMode.CombineAll)
                return sb.ToString();
            return throwException ? throw new ArgumentNullException($"Field node with name [{string.Join(',', fieldNames)}] wasn't found.") : string.Empty;
        }
    }
}