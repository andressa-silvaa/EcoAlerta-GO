using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoAlerta.Api.Models;

/// <summary>
/// Modelo que representa um foco de queimada detectado.
/// Este é o modelo principal do sistema de monitoramento ambiental.
/// </summary>
[Table("Queimadas")]
public class Queimada
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Data e hora da detecção do foco de queimada.
    /// Importante para análise temporal dos dados.
    /// </summary>
    [Required]
    public DateTime DataHora { get; set; }

    /// <summary>
    /// Nome do município onde foi detectado o foco.
    /// Permite análise geográfica e filtros por localidade.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Municipio { get; set; } = string.Empty;

    /// <summary>
    /// Estado onde foi detectado o foco (sempre "GO" neste sistema).
    /// Filtro principal para garantir que apenas dados de Goiás sejam processados.
    /// </summary>
    [Required]
    [MaxLength(2)]
    public string Estado { get; set; } = "GO";

    /// <summary>
    /// Latitude do ponto de queimada.
    /// Essencial para plotagem em mapas geográficos.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    /// <summary>
    /// Longitude do ponto de queimada.
    /// Essencial para plotagem em mapas geográficos.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    /// <summary>
    /// Intensidade do foco (se disponível na API do INPE).
    /// Pode indicar a magnitude da queimada detectada.
    /// </summary>
    public decimal? Intensidade { get; set; }

    /// <summary>
    /// Satélite ou fonte que detectou o foco (ex: AQUA, TERRA, NOAA).
    /// Importante para rastreabilidade dos dados.
    /// </summary>
    [MaxLength(50)]
    public string? FonteSatelite { get; set; }

    /// <summary>
    /// Data de criação do registro no banco de dados.
    /// Útil para auditoria e controle de sincronização.
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}

