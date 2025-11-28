using LDApi.RIS.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LDApi.RIS;
using LDApi.RIS.Utils;
using Xunit;

namespace LDApi.RIS.Tests.Integration
{
    public class HL7messageIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public HL7messageIntegrationTest(WebApplicationFactory<Program> factory)
        {
            // Crée un client HTTP pour la "vraie" application ASP en mémoire
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Send_ReturnOk_WithExpectedJson()
        {
            var dto = new HL7SendDto
            {
                    Id = 1,
                    ClientApp = "LDApiRIS",
                    Client = "Genourob"
            };
            // Act : POST vers la route
            var response = await _client.PostAsJsonAsync("/api/HL7/send", dto);

            // Assert : Vérifie que la réponse est OK
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<HL7ResponseDto>();
 
            Assert.NotNull(responseData);
        }

    }
}
