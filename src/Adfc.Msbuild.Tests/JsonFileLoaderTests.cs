using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class JsonFileLoaderTests
    {
        [TestMethod]
        public async Task LoadDatasetFileAsync()
        {
            var target = new JsonFileLoader();
            using (var reader = new StringReader(JsonSample.Dataset))
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
            using (var reader = new StringReader(JsonSample.LinkedService))
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
            using (var reader = new StringReader(JsonSample.Pipeline))
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
            using (var reader = new StringReader(JsonSample.Config))
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
            using (var reader = new StringReader(JsonSample.Config))
            {
                await target.LoadAsync("config.json", reader);
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => target.LoadAsync("config.json", reader));
            }
        }
    }
}
