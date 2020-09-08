using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IAuthService
    {
        Task<string> GetUserId(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public AuthService(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        public async Task<string> GetUserId(string token)
        {
            if (_memoryCache.TryGetValue(token, out string userId)) return userId;

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://dev-fuzknswu.auth0.com/userinfo");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            if (!httpResponseMessage.IsSuccessStatusCode) return null;

            Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync();

            AuthUser auth0User = await JsonSerializer.DeserializeAsync<AuthUser>(stream);
            userId = auth0User?.sub;

            if (!string.IsNullOrEmpty(userId)) _memoryCache.Set(token, userId, TimeSpan.FromSeconds(86400));

            return userId;
        }
    }
}
