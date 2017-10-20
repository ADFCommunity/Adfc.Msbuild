using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests
{
    internal class MockNoNetworkHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Mock no internet");
        }
    }
}
