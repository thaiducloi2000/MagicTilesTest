using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Score
{
    public static class ConfigHelper
    {
        public static void GenerateEnum(this string enumName, List<string> entries, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(enumName))
                throw new ArgumentException("Enum name is required.");
            if (entries == null || entries.Count == 0)
                throw new ArgumentException("Enum entries are required.");

            var validNames = entries
                .Select(e => ToValidEnumName(e))
                .Distinct()
                .ToList();

            var lines = new List<string>
            {
                "// Auto-generated enum. DO NOT modify manually.",
                "",
                $"public enum {enumName}",
                "{"
            };

            lines.AddRange(validNames.Select(name => $"    {name},"));
            lines.Add("}");

            var dir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllLines(outputPath, lines);
        }

        public static string ToValidEnumName(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Unknown";

            string clean = new string(input.Select(c =>
                char.IsLetterOrDigit(c) || c == '_' ? c : '_'
            ).ToArray());

            if (char.IsDigit(clean[0]))
                clean = "_" + clean;

            return clean;
        }
    }
}