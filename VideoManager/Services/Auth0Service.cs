using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IAuth0Service
    {
        Task<string> GetAuth0UserId(string userId);
    }

    public class Auth0Service : IAuth0Service
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public Auth0Service(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        public async Task<string> GetAuth0UserId(string token)
        {
            if (_memoryCache.TryGetValue(token, out string userId)) return userId;

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://dev-fuzknswu.auth0.com/userinfo");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            string json = await httpResponseMessage.Content.ReadAsStringAsync();

            Auth0User auth0User = JsonSerializer.Deserialize<Auth0User>(json);
            userId = auth0User?.sub;

            if (!string.IsNullOrEmpty(userId)) _memoryCache.Set(token, userId, TimeSpan.FromSeconds(86400));

            return userId;
        }
    }
}
