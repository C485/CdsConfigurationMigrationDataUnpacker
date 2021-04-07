namespace CdsConfigurationMigrationDataUnpacker.Model
{
    public class FileField
    {
        public string FieldName { get; set; }
        public bool? IsBase64 { get; set; }
        public string FileNameFieldName { get; set; }
        public string FileNameFormat { get; set; } = "[RootId]_[RootName]_[Name]_[Extension]";
        public string OverrideFileExtension { get; set; }
    }
}