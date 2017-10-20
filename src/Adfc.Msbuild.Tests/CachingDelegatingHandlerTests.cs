using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests
{
    [TestClass]
    public class CachingDelegatingHandlerTests
    {
        [TestMethod]
        public async Task ShouldCacheSuccessfulResults()
        {
            var innerHandler = new MockHttpMessageHandler();
            var target = new CachingDelegatingHandler(innerHandler);

            using (var httpClient = new HttpClient(target))
            {
                var text = await httpClient.GetStringAsync(JsonSchemaUri.Config);
                text = await httpClient.GetStringAsync(JsonSchemaUri.Config);
            }

            Assert.AreEqual(1, innerHandler.Requests.Count);
        }

        [TestMethod]
        public async Task ShouldCacheNotFoundResults()
        {
            var innerHandler = new MockHttpMessageHandler();
            var target = new CachingDelegatingHandler(innerHandler);

            using (var httpClient = new HttpClient(target))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://server/notfound");
                var response = await httpClient.SendAsync(request);
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

                request = new HttpRequestMessage(HttpMethod.Get, "https://server/notfound");
                response = await httpClient.SendAsync(request);
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }

            Assert.AreEqual(1, innerHandler.Requests.Count);
        }
    }
}
