using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class OfflineDelegatingHandlerTests
    {
        [TestMethod]
        public async Task ShouldGetConfigSchemaWhenOffline()
        {
            var innerHandler = new MockNoNetworkHttpMessageHandler();
            var target = new OfflineDelegatingHandler(innerHandler);

            using (var httpClient = new HttpClient(target))
            {
                var text = await httpClient.GetStringAsync(JsonSchemaUri.Config);
                Assert.AreEqual(Resources.Microsoft_DataFactory_Config, text);
            }
        }

        [TestMethod]
        public async Task ShouldGetDatasetSchemaWhenOffline()
        {
            var innerHandler = new MockNoNetworkHttpMessageHandler();
            var target = new OfflineDelegatingHandler(innerHandler);

            using (var httpClient = new HttpClient(target))
            {
                var text = await httpClient.GetStringAsync(JsonSchemaUri.Dataset);
                Assert.AreEqual(Resources.Microsoft_DataFactory_Table, text);
            }
        }

        [TestMethod]
        public async Task ShouldGetLinkedServiceSchemaWhenOffline()
        {
            var innerHandler = new MockNoNetworkHttpMessageHandler();
            var target = new OfflineDelegatingHandler(innerHandler);

            using (var httpClient = new HttpClient(target))
            {
                var text = await httpClient.GetStringAsync(JsonSchemaUri.LinkedService);
                Assert.AreEqual(Resources.Microsoft_DataFactory_LinkedService, text);
            }
        }

        [TestMethod]
        public async Task ShouldGetPipelineSchemaWhenOffline()
        {
            var innerHandler = new MockNoNetworkHttpMessageHandler();
            var target = new OfflineDelegatingHandler(innerHandler);

            using (var httpClient = new HttpClient(target))
            {
                var text = await httpClient.GetStringAsync(JsonSchemaUri.Pipeline);
                Assert.AreEqual(Resources.Microsoft_DataFactory_Pipeline, text);
            }
        }
    }
}
