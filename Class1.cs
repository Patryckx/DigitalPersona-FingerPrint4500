using System;
using System.Collections.Generic;
using System.IO;

namespace DititalPerson4500
{
    internal class Class1
    {
        public class IniFile
        {
            private readonly string filePath;
            private readonly Dictionary<string, Dictionary<string, string>> sections;

            public IniFile(string path)
            {
                filePath = path;
                sections = new Dictionary<string, Dictionary<string, string>>();

                if (File.Exists(filePath))
                {
                    Load();
                }
            }

            private void Load()
            {
                string currentSection = null;
                foreach (var line in File.ReadAllLines(filePath))
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                        continue;

                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Trim('[', ']');
                        if (!sections.ContainsKey(currentSection))
                        {
                            sections[currentSection] = new Dictionary<string, string>();
                        }
                    }
                    else if (currentSection != null)
                    {
                        var index = trimmedLine.IndexOf('=');
                        if (index > 0)
                        {
                            var key = trimmedLine.Substring(0, index).Trim();
                            var value = trimmedLine.Substring(index + 1).Trim();
                            sections[currentSection][key] = value;
                        }
                    }
                }
            }

            public string ReadValue(string section, string key)
            {
                if (sections.ContainsKey(section) && sections[section].ContainsKey(key))
                {
                    return sections[section][key];
                }
                return null;
            }

            public void WriteValue(string section, string key, string value)
            {
                if (!sections.ContainsKey(section))
                {
                    sections[section] = new Dictionary<string, string>();
                }
                sections[section][key] = value;
                Save();
            }

            private void Save()
            {
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var section in sections)
                    {
                        writer.WriteLine($"[{section.Key}]");
                        foreach (var kvp in section.Value)
                        {
                            writer.WriteLine($"{kvp.Key}={kvp.Value}");
                        }
                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
