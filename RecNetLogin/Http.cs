using System;
using System.Net.Http;

namespace RecNetLogin
{
    internal static class Http
    {
        private static readonly Lazy<HttpClient> HttpClientLazy = new(() =>
        {
            return new HttpClient();
        });

        public static HttpClient Client => HttpClientLazy.Value;
    }
}
