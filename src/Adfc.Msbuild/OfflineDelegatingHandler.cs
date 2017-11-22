using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Adfc.Msbuild
{
    public class OfflineDelegatingHandler : DelegatingHandler
    {
        public OfflineDelegatingHandler()
        {
        }

        public OfflineDelegatingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri.ToString();
            switch (uri)
            {
                case JsonSchemaUri.Config:
                    return MakeResponse(Resources.Microsoft_DataFactory_Config);

                case JsonSchemaUri.Dataset:
                    return MakeResponse(Resources.Microsoft_DataFactory_Table);

                case JsonSchemaUri.LinkedService:
                    return MakeResponse(Resources.Microsoft_DataFactory_LinkedService);

                case JsonSchemaUri.Pipeline:
                    return MakeResponse(Resources.Microsoft_DataFactory_Pipeline);

                default:
                    return await base.SendAsync(request, cancellationToken);
            }
        }

        private HttpResponseMessage MakeResponse(string content)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(content);
            return response;
        }
    }
}
