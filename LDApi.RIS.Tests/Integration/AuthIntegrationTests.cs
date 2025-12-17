using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using LDApi.RIS.Data;
using LDApi.RIS.Tests.Infrastructure;

namespace LDApi.RIS.Tests.Integration
{
    public class AuthIntegrationTests
    {

        private readonly HttpClient _client;
        public AuthIntegrationTests()
        {
            var factory = new TestAppFactory(TestMode.AuthOnly);
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed()
        {


            // 1. Register
            var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            });
            registerResponse.EnsureSuccessStatusCode();

            // 2. Login
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new
            {
                Username = "testuser",
                Password = "Password123!"
            });
            loginResponse.EnsureSuccessStatusCode();

            var loginContent = await loginResponse.Content.ReadFromJsonAsync<JwtLoginResponse>();
            Assert.NotNull(loginContent);
            Assert.False(string.IsNullOrWhiteSpace(loginContent.Token));

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent.Token);

            // 3. Access protected endpoint
            var secureResponse = await _client.GetAsync("/api/Secure/secret-data");
            secureResponse.EnsureSuccessStatusCode();

            var secureContent = await secureResponse.Content.ReadFromJsonAsync<dynamic>();
            string u = (string)secureContent.user;
            Assert.Equal("testuser", u);
        }

        private class JwtLoginResponse
        {
            public string? Token { get; set; }
        }
    }

}