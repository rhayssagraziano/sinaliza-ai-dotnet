using CitizenTech.Abrigos.Data;
using CitizenTech.Abrigos.Dtos;
using CitizenTech.Abrigos.Erros;
using CitizenTech.Abrigos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenTech.Abrigos.Controllers;

[ApiController]
[Route("api/abrigos")]
[Tags("Abrigos")]
[Produces("application/json")]
public class AbrigosController : ControllerBase
{
    private readonly AppDbContext _db;

    public AbrigosController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lista os abrigos. Filtros opcionais: município e só os que ainda têm vaga.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AbrigoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AbrigoResponse>>> Listar(
        [FromQuery] string? municipio,
        [FromQuery] bool apenasComVagas = false)
    {
        var consulta = _db.Abrigos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var termo = municipio.Trim().ToUpper();
            consulta = consulta.Where(a => a.Municipio.ToUpper() == termo);
        }

        if (apenasComVagas)
        {
            consulta = consulta.Where(a => a.Ativo && a.OcupacaoAtual < a.Capacidade);
        }

        var linhas = await consulta
            .OrderBy(a => a.Nome)
            .Select(a => new { Abrigo = a, QuantidadeRotas = a.Rotas.Count })
            .ToListAsync();

        return Ok(linhas.Select(l => AbrigoResponse.De(l.Abrigo, l.QuantidadeRotas)).ToList());
    }

    /// <summary>Busca um abrigo pelo id, junto com as rotas de evacuação dele.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AbrigoDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AbrigoDetalheResponse>> BuscarPorId(int id)
    {
        var abrigo = await _db.Abrigos
            .AsNoTracking()
            .Include(a => a.Rotas)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (abrigo is null)
        {
            return NaoEncontrado(id);
        }

        var rotas = abrigo.Rotas
            .OrderBy(r => r.TempoEstimadoMin)
            .Select(RotaEvacuacaoResponse.De)
            .ToList();

        return Ok(new AbrigoDetalheResponse(AbrigoResponse.De(abrigo, rotas.Count), rotas));
    }

    /// <summary>Lista só as rotas de um abrigo (a parte "N" da relação 1:N).</summary>
    [HttpGet("{id:int}/rotas")]
    [ProducesResponseType(typeof(List<RotaEvacuacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<RotaEvacuacaoResponse>>> ListarRotas(int id)
    {
        if (!await _db.Abrigos.AnyAsync(a => a.Id == id))
        {
            return NaoEncontrado(id);
        }

        var rotas = await _db.RotasEvacuacao
            .AsNoTracking()
            .Include(r => r.Abrigo)
            .Where(r => r.AbrigoId == id)
            .OrderBy(r => r.TempoEstimadoMin)
            .ToListAsync();

        return Ok(rotas.Select(RotaEvacuacaoResponse.De).ToList());
    }

    /// <summary>Cadastra um abrigo. Devolve 201 com o endereço do novo recurso.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AbrigoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AbrigoResponse>> Criar([FromBody] AbrigoRequest request)
    {
        var abrigo = new Abrigo { CriadoEm = DateTime.UtcNow };
        request.CopiarPara(abrigo);

        _db.Abrigos.Add(abrigo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(BuscarPorId), new { id = abrigo.Id }, AbrigoResponse.De(abrigo, 0));
    }

    /// <summary>Atualiza todos os dados de um abrigo. Devolve 204 sem corpo.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AbrigoRequest request)
    {
        var abrigo = await _db.Abrigos.FindAsync(id);
        if (abrigo is null)
        {
            return NaoEncontrado(id);
        }

        request.CopiarPara(abrigo);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Remove um abrigo. As rotas dele são removidas junto (cascade).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        var abrigo = await _db.Abrigos.FindAsync(id);
        if (abrigo is null)
        {
            return NaoEncontrado(id);
        }

        _db.Abrigos.Remove(abrigo);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private NotFoundObjectResult NaoEncontrado(int id) =>
        NotFound(new ErroResponse(StatusCodes.Status404NotFound, $"Abrigo {id} não encontrado."));
}
