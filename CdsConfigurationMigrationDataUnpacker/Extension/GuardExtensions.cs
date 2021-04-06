using Dawn;
using System;
using System.IO;

namespace CdsConfigurationMigrationDataUnpacker.Extension
{
    public static class GuardExtensions
    {
        public static ref readonly Guard.ArgumentInfo<string> FileExists(in this Guard.ArgumentInfo<string> argument)
        {
            if (string.IsNullOrEmpty(argument.Value))
            {
                throw Guard.Fail(new ArgumentException(
                    $"{argument.Name} is not initialized.",
                    argument.Name));
            }
            if (!File.Exists(argument.Value))
            {
                throw Guard.Fail(new FileNotFoundException(
                    $"{argument.Name} not found[{argument.Value}].",
                    argument.Name));
            }

            return ref argument;
        }

        public static ref readonly Guard.ArgumentInfo<string> DirectoryExists(in this Guard.ArgumentInfo<string> argument)
        {
            if (string.IsNullOrEmpty(argument.Value))
            {
                throw Guard.Fail(new ArgumentException(
                    $"{argument.Name} is not initialized.",
                    argument.Name));
            }
            if (!Directory.Exists(argument.Value))
            {
                throw Guard.Fail(new FileNotFoundException(
                    $"{argument.Name} not found[{argument.Value}].",
                    argument.Name));
            }

            return ref argument;
        }
    }
}