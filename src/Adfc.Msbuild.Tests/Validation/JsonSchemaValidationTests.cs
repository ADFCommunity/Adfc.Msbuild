using Adfc.Msbuild.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests.Validation
{
    [TestClass]
    public class JsonSchemaValidationTests
    {
        [TestMethod]
        public async Task ShouldValidateConfigJsonWithExplicitSchema()
        {
            var jsonFile = new JsonFile("test", "test.json", JObject.Parse(TestResource.Config), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task ShouldValidateConfigJsonWithImplicitSchema()
        {
            var jsonFile = new JsonFile("test.json", "test", new JObject(), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnInvalidSchemaUrl()
        {
            const string identity = "test.json";
            var jsonFile = new JsonFile("test", identity, JObject.Parse("{'$schema':'uri://server/path.json'}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
            Assert.AreEqual(identity, actual.FileName);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnSchemaUrlNotFound()
        {
            const string identity = "test.json";
            var jsonFile = new JsonFile("test", identity, JObject.Parse("{'$schema':'https://server/notfound.json'}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
            Assert.AreEqual(identity, actual.FileName);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnInvalidSchemaContent()
        {
            const string identity = "test.json";
            var jsonFile = new JsonFile("test", identity, JObject.Parse("{'$schema':'https://server/notjson.xml'}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
            Assert.AreEqual(identity, actual.FileName);
        }

        [TestMethod]
        public async Task ShouldReportSchemaDownloadErrorOnNetworkError()
        {
            const string identity = "test.json";
            var jsonFile = new JsonFile("test", identity, JObject.Parse(TestResource.Config), ArtefactCategory.Config);
            BuildError actual;
            using (var httpMessageHandler = new MockNoNetworkHttpMessageHandler())
            using (var httpClient = new HttpClient(httpMessageHandler))
            {
                var target = new JsonSchemaValidation(httpClient);
                actual = await target.ValidateAsync(jsonFile);
            }
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
            Assert.AreEqual(identity, actual.FileName);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnExplicitSchemaValidationFailure()
        {
            const string identity = "test.json";
            var jsonFile = new JsonFile("test", identity, JObject.Parse("{'$schema':'" + JsonSchemaUri.Config + "','artefact':{}}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0005.Code, actual.Code);
            Assert.AreEqual(identity, actual.FileName);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnImplicitSchemaValidationFailure()
        {
            const string identity = "test.json";
            var jsonFile = new JsonFile("test", identity, JObject.Parse("{'artefact':{}}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0006.Code, actual.Code);
            Assert.AreEqual(identity, actual.FileName);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnImplicitSchemaValidationFailureAfterSuccessfulExplicitSchemaValidation()
        {
            const string identity = "test.json";
            var jsonFile = new JsonFile("test", identity, JObject.Parse("{'$schema':'https://server/emptyschema.json','artefact':{}}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0006.Code, actual.Code);
            Assert.AreEqual(identity, actual.FileName);
        }

        [TestMethod]
        public async Task ShouldValidateLinkedService()
        {
            var jsonFile = await Util.LoadJsonAsync("linkedservice.json", TestResource.LinkedService);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task ShouldValidateDataset()
        {
            var jsonFile = await Util.LoadJsonAsync("dataset.json", TestResource.Dataset);
            var actual = await ValidateJsonFile(jsonFile);
            if (actual != null)
            {
                System.Console.WriteLine(actual.Message);
            }
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task ShouldValidatePipeline()
        {
            var jsonFile = await Util.LoadJsonAsync("pipeline.json", TestResource.Pipeline);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNull(actual);
        }

        private async Task<BuildError> ValidateJsonFile(JsonFile jsonFile)
        {
            BuildError actual;

            using (var httpMessageHandler = new MockHttpMessageHandler())
            using (var httpClient = new HttpClient(httpMessageHandler))
            {
                var target = new JsonSchemaValidation(httpClient);
                actual = await target.ValidateAsync(jsonFile);
            }

            return actual;
        }
    }
}
