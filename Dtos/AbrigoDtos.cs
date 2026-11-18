using System.ComponentModel.DataAnnotations;
using CitizenTech.Abrigos.Models;

namespace CitizenTech.Abrigos.Dtos;

/// <summary>Corpo do POST e do PUT de /api/abrigos.</summary>
public class AbrigoRequest : IValidatableObject
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório.")]
    [StringLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres.")]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "O bairro é obrigatório.")]
    [StringLength(80, ErrorMessage = "O bairro deve ter no máximo 80 caracteres.")]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "O município é obrigatório.")]
    [StringLength(80, ErrorMessage = "O município deve ter no máximo 80 caracteres.")]
    public string Municipio { get; set; } = string.Empty;

    [Required(ErrorMessage = "A UF é obrigatória.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "A UF deve ter 2 letras, ex.: SP.")]
    public string Uf { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "A capacidade deve ser entre 1 e 100000 pessoas.")]
    public int Capacidade { get; set; }

    [Range(0, 100000, ErrorMessage = "A ocupação atual não pode ser negativa.")]
    public int OcupacaoAtual { get; set; }

    public bool Acessivel { get; set; }

    public bool Ativo { get; set; } = true;

    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    [RegularExpression(@"^[0-9 ()+\-]*$", ErrorMessage = "O telefone só pode ter números, espaço, ( ) + e -.")]
    public string? Telefone { get; set; }

    // Regra que envolve dois campos: não dá para colocar mais gente do que o abrigo comporta.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OcupacaoAtual > Capacidade)
        {
            yield return new ValidationResult(
                "A ocupação atual não pode ser maior que a capacidade.",
                new[] { nameof(OcupacaoAtual) });
        }
    }

    public void CopiarPara(Abrigo abrigo)
    {
        abrigo.Nome = Nome.Trim();
        abrigo.Endereco = Endereco.Trim();
        abrigo.Bairro = Bairro.Trim();
        abrigo.Municipio = Municipio.Trim();
        abrigo.Uf = Uf.ToUpperInvariant();
        abrigo.Capacidade = Capacidade;
        abrigo.OcupacaoAtual = OcupacaoAtual;
        abrigo.Acessivel = Acessivel;
        abrigo.Ativo = Ativo;
        abrigo.Telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone.Trim();
    }
}

/// <summary>O que a API devolve sobre um abrigo.</summary>
public record AbrigoResponse(
    int Id,
    string Nome,
    string Endereco,
    string Bairro,
    string Municipio,
    string Uf,
    int Capacidade,
    int OcupacaoAtual,
    int VagasDisponiveis,
    bool Acessivel,
    bool Ativo,
    string? Telefone,
    DateTime CriadoEm,
    int QuantidadeRotas)
{
    public static AbrigoResponse De(Abrigo a, int quantidadeRotas) => new(
        a.Id, a.Nome, a.Endereco, a.Bairro, a.Municipio, a.Uf,
        a.Capacidade, a.OcupacaoAtual, a.Capacidade - a.OcupacaoAtual,
        a.Acessivel, a.Ativo, a.Telefone, a.CriadoEm, quantidadeRotas);
}

/// <summary>GET /api/abrigos/{id}: o abrigo com as rotas dele (mostra a relação 1:N).</summary>
public record AbrigoDetalheResponse(AbrigoResponse Abrigo, List<RotaEvacuacaoResponse> Rotas);
