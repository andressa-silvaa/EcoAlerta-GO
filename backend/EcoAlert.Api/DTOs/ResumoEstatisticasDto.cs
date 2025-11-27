namespace EcoAlerta.Api.DTOs;

public class ResumoEstatisticasDto
{
    public int TotalFocos { get; set; }

    public int TotalMunicipiosAfetados { get; set; }

    public DateTime? DataComMaisFocos { get; set; }

    public int FocosNaDataMaxima { get; set; }
    
    public double MediaFocosPorDia { get; set; }
}

