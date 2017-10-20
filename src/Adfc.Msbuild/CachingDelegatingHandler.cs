using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Adfc.Msbuild
{
    public class CachingDelegatingHandler : DelegatingHandler
    {
        private Dictionary<Uri, HttpResponseMessage> _cache;

        public CachingDelegatingHandler()
            : base()
        {
            _cache = new Dictionary<Uri, HttpResponseMessage>();
        }

        public CachingDelegatingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _cache = new Dictionary<Uri, HttpResponseMessage>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get)
            {
                HttpResponseMessage response;
                if (!_cache.TryGetValue(request.RequestUri, out response))
                {
                    Console.WriteLine("Downloading " + request.RequestUri);
                    response = await base.SendAsync(request, cancellationToken);
                    _cache.Add(request.RequestUri, response);
                }

                return response;
            }
            else
            {
                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}
