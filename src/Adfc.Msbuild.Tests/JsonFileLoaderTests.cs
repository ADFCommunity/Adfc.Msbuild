using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class JsonFileLoaderTests
    {
        [TestMethod]
        public void LoadDatasetFile()
        {
            var contents = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Table.json'}";

            var result= JsonFileLoader.Parse(contents, "file.json");

            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.Dataset, result.Category);
            Assert.AreEqual("test", result.Name);
            Assert.IsNotNull(result.Json);
        }

        [TestMethod]
        public void LoadLinkedServiceFile()
        {
            var contents = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.LinkedService.json'}";

            var result = JsonFileLoader.Parse(contents, "file.json");

            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.LinkedService, result.Category);
            Assert.AreEqual("test", result.Name);
            Assert.IsNotNull(result.Json);
        }

        [TestMethod]
        public void LoadPipelineFile()
        {
            var contents = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Pipeline.json'}";

            var result = JsonFileLoader.Parse(contents, "file.json");

            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.Pipeline, result.Category);
            Assert.AreEqual("test", result.Name);
            Assert.IsNotNull(result.Json);
        }

        [TestMethod]
        public void LoadConfigFile()
        {
            var contents = "{'$schema':'http://datafactories.schema.management.azure.com/vsschemas/V1/Microsoft.DataFactory.Config.json'}";

            var result = JsonFileLoader.Parse(contents, "file.json");

            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.Config, result.Category);
            Assert.AreEqual("file", result.Name);
            Assert.IsNotNull(result.Json);
        }
    }
}
