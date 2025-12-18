using System.Net;
using System.Net.Http.Json;
using LDApi.RIS.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using LDApi.RIS.Tests.Infrastructure;

namespace LDApi.RIS.Tests.Integration
{
    public class HL7BatchIntegrationTests
    {

        private readonly HttpClient _client;

        public HL7BatchIntegrationTests()
        {
            var factory = new TestAppFactory(TestMode.FullMockServices);

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SendBatch_ReturnsAcksForAllReports()
        {
            // Arrange
            var dto = new HL7SendDto
            {
                Client = "clientTest",
                ClientApp = "clientAppTest"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/HL7/send-batch", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var results = System.Text.Json.JsonSerializer.Deserialize<List<HL7ResponseDto>>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(results);
            Assert.Equal(2, results!.Count);

            var first = results.Single(r => r.Ack == StatusAck.AA);
            var second = results.Single(r => r.Ack == StatusAck.AE);

            Assert.Equal(StatusAck.AA, first.Ack);
            Assert.Equal(StatusAck.AE, second.Ack);
        }
    }
}
