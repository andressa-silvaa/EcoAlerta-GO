using EcoAlerta.Api.Clients;
using EcoAlerta.Api.DTOs;
using EcoAlerta.Api.Models;
using EcoAlerta.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EcoAlerta.Tests.Services;

public class QueimadaServiceTests
{
    private readonly Mock<IInpeApiClient> _inpeApiClientMock = new();
    private readonly Mock<ILogger<QueimadaService>> _loggerMock = new();

    private QueimadaService CreateService(IEnumerable<Queimada> queimadas)
    {
        _inpeApiClientMock
            .Setup(client => client.ObterFocosQueimadasAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(queimadas.ToList());

        return new QueimadaService(_inpeApiClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObterQueimadasAsync_DeveFiltrarPorMunicipio()
    {
        var service = CreateService(new[]
        {
            CriarQueimada(1, "Goiânia"),
            CriarQueimada(2, "Anápolis"),
            CriarQueimada(3, "Goiânia")
        });

        var resultado = await service.ObterQueimadasAsync(null, null, "goiÂnia");

        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(q => q.Municipio.Equals("Goiânia", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ObterEstatisticasPorMunicipioAsync_DeveAgruparCorretamente()
    {
        var service = CreateService(new[]
        {
            CriarQueimada(1, "Goiânia"),
            CriarQueimada(2, "Goiânia"),
            CriarQueimada(3, "Anápolis"),
            CriarQueimada(4, "Anápolis"),
            CriarQueimada(5, "Anápolis"),
            CriarQueimada(6, "Rio Verde")
        });

        var estatisticas = await service.ObterEstatisticasPorMunicipioAsync(null, null);

        estatisticas.Should().HaveCount(3);
        estatisticas.Should().ContainEquivalentOf(new EstatisticasMunicipiosDto { Municipio = "Anápolis", TotalFocos = 3 });
        estatisticas.First().Municipio.Should().Be("Anápolis");
    }

    [Fact]
    public async Task ObterResumoEstatisticasAsync_DeveCalcularIndicadores()
    {
        var dataBase = new DateTime(2024, 10, 1);
        var service = CreateService(new[]
        {
            CriarQueimada(1, "Goiânia", dataBase),
            CriarQueimada(2, "Goiânia", dataBase.AddDays(1)),
            CriarQueimada(3, "Anápolis", dataBase.AddDays(1)),
            CriarQueimada(4, "Rio Verde", dataBase.AddDays(2))
        });

        var resumo = await service.ObterResumoEstatisticasAsync(dataBase, dataBase.AddDays(2));

        resumo.TotalFocos.Should().Be(4);
        resumo.TotalMunicipiosAfetados.Should().Be(3);
        resumo.DataComMaisFocos.Should().Be(dataBase.AddDays(1).Date);
        resumo.FocosNaDataMaxima.Should().Be(2);
        resumo.MediaFocosPorDia.Should().BeGreaterThan(0);
    }

    private static Queimada CriarQueimada(int id, string municipio, DateTime? data = null)
        => new()
        {
            Id = id,
            Municipio = municipio,
            Estado = "GO",
            DataHora = (data ?? DateTime.UtcNow).AddHours(id),
            Latitude = -16.0m,
            Longitude = -49.0m,
            Intensidade = 30,
            FonteSatelite = "TERRA"
        };
}

