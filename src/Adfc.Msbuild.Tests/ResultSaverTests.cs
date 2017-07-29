using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class ResultSaverTests
    {
        [TestMethod]
        public async Task ShouldSaveDocumentsToConfigurationDirectory()
        {
            string contents = "{'name':'file'}";
            var file = new JsonFile("MyDataset",
                "file.json",
                JObject.Parse(contents),
                ArtefactCategory.Dataset);
            var documents = new List<JsonFile>() { file };

            var config = new JsonFile("MyConfig",
                "config.json",
                JObject.Parse(contents),
                ArtefactCategory.Config);

            var outputPath = Path.Combine(nameof(ResultSaverTests), nameof(ShouldSaveDocumentsToConfigurationDirectory));
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            Directory.CreateDirectory(outputPath);

            var target = new ResultSaver();
            await target.SaveDocuments(documents, config, outputPath);

            var expectedPath = Path.Combine(outputPath, config.Name, file.Category.ToString(), file.Name + ".json");
            Assert.IsTrue(File.Exists(expectedPath), "File doesn't exist: " + expectedPath);
        }
    }
}
