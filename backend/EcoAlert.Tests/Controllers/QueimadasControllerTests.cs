using System.Net;
using System.Net.Http.Json;
using EcoAlerta.Api.Clients;
using EcoAlerta.Api.DTOs;
using EcoAlerta.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EcoAlerta.Tests.Controllers;

public class QueimadasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public QueimadasControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost")
        });
    }

    [Fact]
    public async Task GetResumoEstatisticas_DeveRetornar200()
    {
        var response = await _client.GetAsync("/api/queimadas/estatisticas/resumo");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ResumoEstatisticasDto>();
        payload.Should().NotBeNull();
        payload!.TotalFocos.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetQueimadas_DeveAplicarFiltroMunicipio()
    {
        var response = await _client.GetAsync("/api/queimadas?municipio=Anapolis");
        response.EnsureSuccessStatusCode();

        var dados = await response.Content.ReadFromJsonAsync<List<QueimadaDto>>();
        dados.Should().NotBeNull();
        dados.Should().OnlyContain(q => q.Municipio.Contains("Anápolis", StringComparison.OrdinalIgnoreCase));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IInpeApiClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IInpeApiClient, FakeInpeApiClient>();
        });
    }
}

internal class FakeInpeApiClient : IInpeApiClient
{
    private readonly List<Queimada> _dados = new()
    {
        new Queimada
        {
            Id = 1,
            Municipio = "Goiânia",
            Estado = "GO",
            DataHora = DateTime.UtcNow.AddHours(-5),
            Latitude = -16.65m,
            Longitude = -49.26m,
            Intensidade = 45,
            FonteSatelite = "AQUA"
        },
        new Queimada
        {
            Id = 2,
            Municipio = "Anápolis",
            Estado = "GO",
            DataHora = DateTime.UtcNow.AddHours(-7),
            Latitude = -16.33m,
            Longitude = -48.95m,
            Intensidade = 33,
            FonteSatelite = "TERRA"
        }
    };

    public Task<List<Queimada>> ObterFocosQueimadasAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
        => Task.FromResult(_dados);
}

