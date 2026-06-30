using System;
using System.Collections.Generic;
using System.IO;

namespace PCL_CE.Neo.Core.Configuration;

public class ConfigMigration
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public Action<string, string>? OnMigration { get; set; }

    public static bool Migrate(string target, IEnumerable<ConfigMigration> migrations)
    {
        foreach (var migration in migrations)
        {
            if (File.Exists(migration.From))
            {
                var targetDir = Path.GetDirectoryName(migration.To);
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                migration.OnMigration?.Invoke(migration.From, migration.To);
                return true;
            }
        }
        return false;
    }
}