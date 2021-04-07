using Dawn;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace CdsConfigurationMigrationDataUnpacker
{
    public abstract class IOLayer
    {
        private const string DataXmlFileName = "data.xml";

        protected IOLayer()
        {
        }

        protected static string ClearFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            StringBuilder oryginalString = new(fileName);
            for (int i = 0; i < oryginalString.Length; i++)
            {
                if (invalidChars.Any(p => p == oryginalString[i]))
                    oryginalString[i] = '_';
            }

            return oryginalString.ToString();
        }

        protected static string AddSuffixAndRemoveExtension(string filename, string suffix)
        {
            string fDir = Path.GetDirectoryName(filename);
            string fName = Path.GetFileNameWithoutExtension(filename);
            return Path.Combine(fDir, string.Concat(fName, suffix));
        }

        protected static void WriteBase64File(string filePath, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;
            byte[] data = Convert.FromBase64String(content);
            File.WriteAllBytes(filePath, data);
        }

        protected static string GetDataXmlContextFromZip(string ArchiveFileName)
        {
            using ZipArchive zipeFile = ZipFile.Open(ArchiveFileName, ZipArchiveMode.Read);
            ZipArchiveEntry dataFile = Guard
                .Argument(zipeFile, nameof(zipeFile))
                .NotNull()
                .Value
                .GetEntry(DataXmlFileName);
            using StreamReader sr = new(Guard
                .Argument(dataFile, nameof(dataFile))
                .NotNull()
                .Value
                .Open());
            return sr.ReadToEnd();
        }
    }
}