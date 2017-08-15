using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Adfc.Msbuild
{
    public static class JsonFileLoader
    {
        public static JsonFile Parse(string text, string identity)
        {
            var json = JObject.Parse(text);
            var name = GetName(json, identity);
            var category = GetCategory(json);

            return new JsonFile(name, identity, json, category);
        }

        private static string GetName(JObject json, string identity)
        {
            return GetName(json) ?? GetName(identity);
        }

        private static string GetName(JObject json)
        {
            var name = json["name"] as JValue;
            return name?.Value.ToString();
        }

        private static string GetName(string identity)
        {
            int filenameStart = identity.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            int extensionStart = identity.IndexOf(".", filenameStart);
            int length = extensionStart >= 0 ? extensionStart - filenameStart : identity.Length - 1 - filenameStart;
            return identity.Substring(filenameStart, length);
        }

        private static ArtefactCategory GetCategory(JObject json)
        {
            var schema = json["$schema"] as JValue;
            if (schema == null)
            {
                throw new Exception("does not contain $schema property.");
            }

            var value = (string)schema.Value;
            var filename = Path.GetFileNameWithoutExtension(value).ToLowerInvariant();

            switch (filename)
            {
                case "microsoft.datafactory.table":
                    return ArtefactCategory.Dataset;

                case "microsoft.datafactory.linkedservice":
                    return ArtefactCategory.LinkedService;

                case "microsoft.datafactory.pipeline":
                    return ArtefactCategory.Pipeline;

                case "microsoft.datafactory.config":
                    return ArtefactCategory.Config;

                default:
                    throw new Exception("Unsupported schema " + value);
            }

        }
    }
}
