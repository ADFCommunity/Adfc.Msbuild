using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Adfc.Msbuild.Tests
{
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        public Dictionary<Uri, HttpResponseMessage> Routes { get; }

        public MockHttpMessageHandler()
        {
            Routes = new Dictionary<Uri, HttpResponseMessage>();

            Routes.Add(new Uri(JsonSchemaUri.Config), CreateHttpResponse(Resources.Microsoft_DataFactory_Config));
            Routes.Add(new Uri(JsonSchemaUri.Dataset), CreateHttpResponse(Resources.Microsoft_DataFactory_Table));
            Routes.Add(new Uri(JsonSchemaUri.LinkedService), CreateHttpResponse(Resources.Microsoft_DataFactory_LinkedService));
            Routes.Add(new Uri(JsonSchemaUri.Pipeline), CreateHttpResponse(Resources.Microsoft_DataFactory_Pipeline));
            Routes.Add(new Uri("https://server/emptyschema.json"), CreateHttpResponse("{}"));
            Routes.Add(new Uri("https://server/notjson.xml"), CreateHttpResponse("<xml />"));
        }

        private HttpResponseMessage CreateHttpResponse(string contents)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(contents, Encoding.UTF8, "application/json");
            return response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            if (Routes.TryGetValue(request.RequestUri, out response))
            {
                return Task.FromResult(response);
            }

            response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);

            return Task.FromResult(response);
        }
    }
}
