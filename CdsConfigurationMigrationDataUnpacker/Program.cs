using CdsConfigurationMigrationDataUnpacker.Model;
using PowerArgs;
using System;
using System.IO;

namespace CdsConfigurationMigrationDataUnpacker
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var parsed = Args.Parse<PortalUnpackerArgs>(args);
                if (string.IsNullOrEmpty(parsed.ConfigFile))
                    parsed.ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "config", "default.json");

                CdsConfigurationMigrationDataUnpacker logic = new(parsed);
                logic.Execute();
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<PortalUnpackerArgs>());
            }
        }
    }
}