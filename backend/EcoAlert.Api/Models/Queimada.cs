using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoAlerta.Api.Models;

[Table("Queimadas")]
public class Queimada
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime DataHora { get; set; }

    [Required]
    [MaxLength(200)]
    public string Municipio { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string Estado { get; set; } = "GO";

    [Required]
    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Required]
    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    public decimal? Intensidade { get; set; }

    [MaxLength(50)]
    public string? FonteSatelite { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}

