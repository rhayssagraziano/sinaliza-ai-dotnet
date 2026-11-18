namespace CitizenTech.Abrigos.Erros;

/// <summary>
/// Formato único de erro da API, ex.:
/// { "status": 404, "erro": "Abrigo 99 não encontrado.", "detalhes": null }
/// </summary>
public record ErroResponse(
    int Status,
    string Erro,
    IDictionary<string, string[]>? Detalhes = null);
