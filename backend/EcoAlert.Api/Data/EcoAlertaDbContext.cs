using Microsoft.EntityFrameworkCore;
using EcoAlerta.Api.Models;

namespace EcoAlerta.Api.Data;

/// <summary>
/// DbContext do Entity Framework Core para persistência de dados.
/// Configurado para suportar connection strings de bancos remotos (MongoDB Atlas, Railway, etc.).
/// </summary>
public class EcoAlertaDbContext : DbContext
{
    public EcoAlertaDbContext(DbContextOptions<EcoAlertaDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet de queimadas - representa a tabela de focos de queimadas.
    /// Pode ser usado para armazenar consultas históricas ou cache de dados da API do INPE.
    /// </summary>
    public DbSet<Queimada> Queimadas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração do modelo de Queimada
        modelBuilder.Entity<Queimada>(entity =>
        {
            entity.HasIndex(e => e.DataHora);
            entity.HasIndex(e => e.Municipio);
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
        });
    }
}

