using System;

namespace CdsConfigurationMigrationDataUnpacker.Model
{
    public class ConfigItem
    {
        public string EntityName { get; set; }
        public FileField[] FileFields { get; set; } = Array.Empty<FileField>();
        public bool UnpackToFolder { get; set; }
        public string[] RecordNameFields { get; set; }
        public NameCombineMode RecordNameCombineMode { get; set; }
        public string RecordNameFormat { get; set; } = "[Name]_[Id]";
        public string FolderNameFormat { get; set; } = "[DisplayName]_[LogicalName]";
        public string[] HtmlDecodeFields { get; set; } = Array.Empty<string>();
    }

    public enum NameCombineMode
    {
        FirstNotNull,
        CombineAll
    }
}