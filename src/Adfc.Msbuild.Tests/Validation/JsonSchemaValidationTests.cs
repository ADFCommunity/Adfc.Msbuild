using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Adfc.Msbuild.Validation;

namespace Adfc.Msbuild.Tests.Validation
{
    [TestClass]
    public class JsonSchemaValidationTests
    {
        [TestMethod]
        public async Task ShouldValidateConfigJsonWithExplicitSchema()
        {
            var jsonFile = new JsonFile("test", "test.json", JObject.Parse(JsonSample.Config), ArtefactCategory.Config);
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
            var jsonFile = new JsonFile("test.json", "test", JObject.Parse("{'$schema':'uri://server/path.json'}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnSchemaUrlNotFound()
        {
            var jsonFile = new JsonFile("test.json", "test", JObject.Parse("{'$schema':'https://server/notfound.json'}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnInvalidSchemaContent()
        {
            var jsonFile = new JsonFile("test.json", "test", JObject.Parse("{'$schema':'https://server/notjson.xml'}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
        }

        [TestMethod]
        public async Task ShouldReportSchemaDownloadErrorOnNetworkError()
        {
            var jsonFile = new JsonFile("test", "test.json", JObject.Parse(JsonSample.Config), ArtefactCategory.Config);
            BuildError actual;
            using (var httpMessageHandler = new MockNoNetworkHttpMessageHandler())
            using (var httpClient = new HttpClient(httpMessageHandler))
            {
                var target = new JsonSchemaValidation(httpClient);
                actual = await target.ValidateAsync(jsonFile);
            }
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0007.Code, actual.Code);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnExplicitSchemaValidationFailure()
        {
            var jsonFile = new JsonFile("test.json", "test", JObject.Parse("{'$schema':'" + JsonSchemaUri.Config + "','artefact':{}}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0005.Code, actual.Code);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnImplicitSchemaValidationFailure()
        {
            var jsonFile = new JsonFile("test.json", "test", JObject.Parse("{'artefact':{}}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0006.Code, actual.Code);
        }

        [TestMethod]
        public async Task ShouldReportBuildErrorOnImplicitSchemaValidationFailureAfterSuccessfulExplicitSchemaValidation()
        {
            var jsonFile = new JsonFile("test.json", "test", JObject.Parse("{'$schema':'https://server/emptyschema.json','artefact':{}}"), ArtefactCategory.Config);
            var actual = await ValidateJsonFile(jsonFile);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ErrorCodes.Adfc0006.Code, actual.Code);
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
