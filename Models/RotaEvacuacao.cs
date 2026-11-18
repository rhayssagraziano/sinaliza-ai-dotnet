namespace CitizenTech.Abrigos.Models;

/// <summary>
/// Caminho seguro de um ponto de partida (ex.: um cruzamento com semáforo inteligente)
/// até um abrigo. Na "onda verde de evacuação" os semáforos desta rota abrem em sequência.
/// </summary>
public class RotaEvacuacao
{
    public int Id { get; set; }

    /// <summary>Chave estrangeira: cada rota pertence a um abrigo.</summary>
    public int AbrigoId { get; set; }

    public Abrigo? Abrigo { get; set; }

    /// <summary>De onde a rota sai, ex.: "Av. do Estado x Rua Mauá".</summary>
    public string PontoPartida { get; set; } = string.Empty;

    /// <summary>Descrição do trajeto, rua a rua.</summary>
    public string Trajeto { get; set; } = string.Empty;

    public int DistanciaMetros { get; set; }

    public int TempoEstimadoMin { get; set; }

    /// <summary>A rota evita escadas e tem calçada rebaixada (idosos, cadeirantes).</summary>
    public bool Acessivel { get; set; }

    /// <summary>Falsa quando a rota está alagada ou bloqueada.</summary>
    public bool Ativa { get; set; } = true;

    public DateTime CriadoEm { get; set; }
}
