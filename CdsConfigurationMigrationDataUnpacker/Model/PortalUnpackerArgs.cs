using PowerArgs;

namespace CdsConfigurationMigrationDataUnpacker.Model
{
    public class PortalUnpackerArgs
    {
        [ArgRequired(PromptIfMissing = false)]
        [ArgExistingDirectory]
        [ArgDescription("Directory where project will be unpacked in[must exist].")]
        public string TargetDirectory { get; set; }

        [ArgRequired(PromptIfMissing = false)]
        [ArgExistingFile]
        [ArgDescription("Zip file produced by CDS Configuration Migration [must exist].")]
        public string SourceFile { get; set; }

        [ArgExistingFile]
        [ArgDescription("Config file location, must exist when provided.")]
        public string ConfigFile { get; set; }
    }
}