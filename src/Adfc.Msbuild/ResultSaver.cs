using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Adfc.Msbuild
{
    public class ResultSaver
    {
        public async Task SaveDocuments(IList<JsonFile> documents, JsonFile config, string outputPath)
        {
            var configPath = Path.Combine(outputPath, config.Name);
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            foreach (var document in documents)
            {
                await SaveDocument(document, configPath);
            }
        }

        private Task SaveDocument(JsonFile document, string configPath)
        {
            var categoryPath = Path.Combine(configPath, document.Category.ToString());
            if (!Directory.Exists(categoryPath))
            {
                Directory.CreateDirectory(categoryPath);
            }

            var filePath = Path.Combine(categoryPath, document.Name + ".json");
            using (var writer = new StreamWriter(filePath))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = Formatting.Indented;
                return document.Json.WriteToAsync(jsonWriter);
            }
        }
    }
}
