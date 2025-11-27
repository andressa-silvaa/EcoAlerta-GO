using EcoAlerta.Api.Models;
using EcoAlerta.Api.Configuration;

namespace EcoAlerta.Api.Clients.Inpe;

internal static class MockDataGenerator
{
    private static readonly string[] MunicipiosGoias = new[]
    {
        "Goiânia", "Aparecida de Goiânia", "Anápolis", "Rio Verde", "Luziânia",
        "Águas Lindas de Goiás", "Valparaíso de Goiás", "Trindade", "Formosa", "Novo Gama",
        "Senador Canedo", "Catalão", "Jataí", "Itumbiara", "Santo Antônio do Descoberto"
    };

    private static readonly string[] Satelites = new[] { "AQUA", "TERRA", "NOAA", "SUOMI" };

    public static List<Queimada> Generate(DateTime startDate, DateTime endDate, InpeApiOptions options)
    {
        var random = new Random();
        var queimadas = new List<Queimada>();
        var currentDate = startDate;
        var idCounter = 1;

        while (currentDate <= endDate)
        {
            var focosNoDia = random.Next(0, 16);

            for (var i = 0; i < focosNoDia; i++)
            {
                var queimada = GenerateSingleFoco(currentDate, idCounter++, random, options);
                queimadas.Add(queimada);
            }

            currentDate = currentDate.AddDays(1);
        }

        return queimadas;
    }

    private static Queimada GenerateSingleFoco(
        DateTime date,
        int id,
        Random random,
        InpeApiOptions options)
    {
        var municipio = MunicipiosGoias[random.Next(MunicipiosGoias.Length)];
        var latitude = (decimal)(-16.0 - random.NextDouble() * 2.0);
        var longitude = (decimal)(-48.0 - random.NextDouble() * 3.0);

        return new Queimada
        {
            Id = id,
            DataHora = date.AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60)),
            Municipio = municipio,
            Estado = options.DefaultEstado,
            Latitude = latitude,
            Longitude = longitude,
            Intensidade = (decimal)(random.NextDouble() * 100),
            FonteSatelite = Satelites[random.Next(Satelites.Length)],
            DataCriacao = DateTime.UtcNow
        };
    }
}

