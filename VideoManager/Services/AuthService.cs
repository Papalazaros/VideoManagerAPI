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
        Task<AuthUser?> GetUser(string token);
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

        public async Task<AuthUser?> GetUser(string token)
        {
            if (_memoryCache.TryGetValue(token, out AuthUser? user))
            {
                return user;
            }

            using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "https://dev-fuzknswu.auth0.com/userinfo");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            using Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            user = await JsonSerializer.DeserializeAsync<AuthUser>(stream);

            if (user != null)
            {
                _memoryCache.Set(token, user, TimeSpan.FromSeconds(86400));
            }

            return user;
        }
    }
}
