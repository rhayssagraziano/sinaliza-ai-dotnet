using System.ComponentModel.DataAnnotations;
using CitizenTech.Abrigos.Models;

namespace CitizenTech.Abrigos.Dtos;

/// <summary>Corpo do POST e do PUT de /api/rotas-evacuacao.</summary>
public class RotaEvacuacaoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Informe o id do abrigo (abrigoId).")]
    public int AbrigoId { get; set; }

    [Required(ErrorMessage = "O ponto de partida é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O ponto de partida deve ter entre 3 e 150 caracteres.")]
    public string PontoPartida { get; set; } = string.Empty;

    [Required(ErrorMessage = "O trajeto é obrigatório.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "O trajeto deve ter entre 3 e 500 caracteres.")]
    public string Trajeto { get; set; } = string.Empty;

    [Range(1, 200000, ErrorMessage = "A distância deve ser entre 1 e 200000 metros.")]
    public int DistanciaMetros { get; set; }

    [Range(1, 1440, ErrorMessage = "O tempo estimado deve ser entre 1 e 1440 minutos.")]
    public int TempoEstimadoMin { get; set; }

    public bool Acessivel { get; set; }

    public bool Ativa { get; set; } = true;

    public void CopiarPara(RotaEvacuacao rota)
    {
        rota.AbrigoId = AbrigoId;
        rota.PontoPartida = PontoPartida.Trim();
        rota.Trajeto = Trajeto.Trim();
        rota.DistanciaMetros = DistanciaMetros;
        rota.TempoEstimadoMin = TempoEstimadoMin;
        rota.Acessivel = Acessivel;
        rota.Ativa = Ativa;
    }
}

/// <summary>O que a API devolve sobre uma rota.</summary>
public record RotaEvacuacaoResponse(
    int Id,
    int AbrigoId,
    string? NomeAbrigo,
    string PontoPartida,
    string Trajeto,
    int DistanciaMetros,
    int TempoEstimadoMin,
    bool Acessivel,
    bool Ativa,
    DateTime CriadoEm)
{
    public static RotaEvacuacaoResponse De(RotaEvacuacao r) => new(
        r.Id, r.AbrigoId, r.Abrigo?.Nome, r.PontoPartida, r.Trajeto,
        r.DistanciaMetros, r.TempoEstimadoMin, r.Acessivel, r.Ativa, r.CriadoEm);
}
