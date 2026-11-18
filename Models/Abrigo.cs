namespace CitizenTech.Abrigos.Models;

/// <summary>
/// Local seguro (escola, ginásio, igreja...) que recebe a população durante uma enchente.
/// Um abrigo tem várias rotas de evacuação (relação 1:N).
/// </summary>
public class Abrigo
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Municipio { get; set; } = string.Empty;

    /// <summary>Sigla do estado, ex.: "SP".</summary>
    public string Uf { get; set; } = string.Empty;

    /// <summary>Quantas pessoas o abrigo comporta.</summary>
    public int Capacidade { get; set; }

    /// <summary>Quantas pessoas estão no abrigo agora. Nunca maior que a capacidade.</summary>
    public int OcupacaoAtual { get; set; }

    /// <summary>Tem rampa, banheiro adaptado e acesso para cadeira de rodas.</summary>
    public bool Acessivel { get; set; }

    public bool Ativo { get; set; } = true;

    public string? Telefone { get; set; }

    public DateTime CriadoEm { get; set; }

    public List<RotaEvacuacao> Rotas { get; set; } = new();
}
