using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TOAHawk
{
    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }
    class RestClient
    {

        public bool IsDisposed { get; private set; }
        public HttpClient BaseClient { get; private set; }

        private readonly string jsonMediaType = "application/json";

        public RestClient(string hostName, HttpClient client)
        {
            BaseClient = client;
            BaseClient.BaseAddress = new Uri(hostName);
            BaseClient.DefaultRequestHeaders.Add("X-TOA-Key", "1c10e5d044624a1adfcc4611f2802d2bce19824ffe4c26241c0e391f0a3848fa");
            BaseClient.DefaultRequestHeaders.Add("X-Application-Origin", "TOAHawk");
        }

        public async Task<string> PostAsync(string resource, string postData)
        {
            StringContent strContent = new StringContent(postData, Encoding.UTF8, jsonMediaType);
            HttpResponseMessage responseMessage = await BaseClient.PostAsync(resource, strContent).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> SendAsync(HttpMethod method, string resource, string postData)
        {
            var resourceUri = new Uri(resource, UriKind.Relative);
            var uri = new Uri(BaseClient.BaseAddress, resourceUri);
            HttpRequestMessage request = new HttpRequestMessage(method, uri);
            if (!string.IsNullOrEmpty(postData))
            {
                request.Content = new StringContent(postData, Encoding.UTF8, jsonMediaType);
            }

            HttpResponseMessage response = BaseClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (IsDisposed)
            {
                return;
            }
            if (isDisposing)
            {
                BaseClient.Dispose();
            }
            IsDisposed = true;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RestClient()
        {
            Dispose(false);
        }
    }
}
