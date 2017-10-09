using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class JsonFileLoaderTests
    {
        private const string DatasetJson = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Table.json'}";
        private const string LinkedServiceJson = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.LinkedService.json'}";
        private const string PipelineJson = "{'name':'test','$schema':'http://datafactories.schema.management.azure.com/schemas/2015-09-01/Microsoft.DataFactory.Pipeline.json'}";
        private const string ConfigJson = "{'$schema':'http://datafactories.schema.management.azure.com/vsschemas/V1/Microsoft.DataFactory.Config.json'}";

        [TestMethod]
        public async Task LoadDatasetFileAsync()
        {
            var target = new JsonFileLoader();
            using (var reader = new StringReader(DatasetJson))
            {
                await target.LoadAsync("file.json", reader);
            }

            var result = target.JsonFile;
            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.Dataset, result.Category);
            Assert.AreEqual("test", result.Name);
            Assert.IsNotNull(result.Json);
            Assert.AreEqual(0, target.Errors.Count);
        }

        [TestMethod]
        public async Task LoadLinkedServiceFileAsync()
        {
            var target = new JsonFileLoader();
            using (var reader = new StringReader(LinkedServiceJson))
            {
                await target.LoadAsync("file.json", reader);
            }

            var result = target.JsonFile;
            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.LinkedService, result.Category);
            Assert.AreEqual("test", result.Name);
            Assert.IsNotNull(result.Json);
            Assert.AreEqual(0, target.Errors.Count);
        }

        [TestMethod]
        public async Task LoadPipelineFile()
        {
            var target = new JsonFileLoader();
            using (var reader = new StringReader(PipelineJson))
            {
                await target.LoadAsync("file.json", reader);
            }

            var result = target.JsonFile;
            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.Pipeline, result.Category);
            Assert.AreEqual("test", result.Name);
            Assert.IsNotNull(result.Json);
            Assert.AreEqual(0, target.Errors.Count);
        }

        [TestMethod]
        public async Task LoadConfigFileAsync()
        {
            var target = new JsonFileLoader();
            using (var reader = new StringReader(ConfigJson))
            {
                await target.LoadAsync("file.json", reader);
            }

            var result = target.JsonFile;
            Assert.IsNotNull(result);
            Assert.AreEqual("file.json", result.Identity);
            Assert.AreEqual(ArtefactCategory.Config, result.Category);
            Assert.AreEqual("file", result.Name);
            Assert.IsNotNull(result.Json);
            Assert.AreEqual(0, target.Errors.Count);
        }

        [TestMethod]
        public async Task LoadInvalidJsonAsync()
        {
            var json = "{";
            var target = new JsonFileLoader();
            using (var reader = new StringReader(json))
            {
                await target.LoadAsync("file.json", reader);
            }

            Assert.IsNull(target.JsonFile);
            Assert.AreEqual(1, target.Errors.Count);
            Assert.AreEqual(ErrorCodes.Adfc0004.Code, target.Errors[0].Code);
        }

        [TestMethod]
        public async Task LoadJsonWithUnknownArtefactTypeAsync()
        {
            var json = "{\"NotADataFactoryProperty\": true}";
            var target = new JsonFileLoader();
            using (var reader = new StringReader(json))
            {
                await target.LoadAsync("file.json", reader);
            }

            Assert.IsNull(target.JsonFile);
            Assert.AreEqual(1, target.Errors.Count);
            Assert.AreEqual(ErrorCodes.Adfc0003.Code, target.Errors[0].Code);
        }

        [TestMethod]
        public void MustLoadBeforeGettingProperties()
        {
            var target = new JsonFileLoader();

            Assert.ThrowsException<InvalidOperationException>(() => target.Errors);
            Assert.ThrowsException<InvalidOperationException>(() => target.JsonFile);
        }

        [TestMethod]
        public async Task CannotLoadTwiceAsync()
        {
            var target = new JsonFileLoader();
            using (var reader = new StringReader(ConfigJson))
            {
                await target.LoadAsync("config.json", reader);
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => target.LoadAsync("config.json", reader));
            }
        }
    }
}
