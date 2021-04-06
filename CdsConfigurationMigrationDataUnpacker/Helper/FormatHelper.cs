namespace CdsConfigurationMigrationDataUnpacker.Helper
{
    public static class FormatHelper
    {
        public static string ReplaceAllTags(string format, params (string tag, string value)[] args)
        {
            string result = format;
            foreach ((string tag, string value) in args)
                result = result.Replace(tag, value);
            return result;
        }
    }
}