using System;
using System.Collections.Generic;
using WDE.Module.Attributes;

namespace WDE.Common.Services
{
    public interface IApplicationReleaseConfiguration
    {
        string? GetString(string key);
        bool? GetBool(string key);
        int? GetInt(string key);
    }
    
    public abstract class BaseApplicationReleaseConfiguration : IApplicationReleaseConfiguration
    {
        private readonly IFileSystem fileSystem;
        private readonly Dictionary<string, string> config = new();

        public BaseApplicationReleaseConfiguration(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;

            ReadConfig("app.ini");
        }

        private void ReadConfig(string configurationPath)
        {
            if (!fileSystem.Exists(configurationPath))
                return;

            var contents = fileSystem.ReadAllText(configurationPath);

            if (contents == null)
                return;

            var lines = contents.Split('\n');

            foreach (var line in lines)
            {
                if (!TrySplitLine(line.Trim(), out var key, out var value))
                    continue;

                config[key!] = value!;
            }
        }

        private bool TrySplitLine(string line, out string? key, out string? value)
        {
            int equalsSign = line.IndexOf("=", StringComparison.Ordinal);
            if (equalsSign == -1)
            {
                key = null;
                value = null;
                return false;
            }

            key = line.Substring(0, equalsSign).Trim();
            value = line.Substring(equalsSign + 1).Trim();
            return true;
        }

        public string? GetString(string key)
        {
            if (config.TryGetValue(key, out var value))
                return value;
            return null;
        }

        public bool? GetBool(string key)
        {
            var str = GetString(key);
            if (str == null)
                return null;
            return str.Trim().ToLower() == "true" || str.Trim() == "1";
        }

        public int? GetInt(string key)
        {
            if (config.TryGetValue(key, out var value) && int.TryParse(value, out var intValue))
                return intValue;
            return null;
        }
    }
}