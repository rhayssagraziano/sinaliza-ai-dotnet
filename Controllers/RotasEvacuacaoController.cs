using CitizenTech.Abrigos.Data;
using CitizenTech.Abrigos.Dtos;
using CitizenTech.Abrigos.Erros;
using CitizenTech.Abrigos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenTech.Abrigos.Controllers;

[ApiController]
[Route("api/rotas-evacuacao")]
[Tags("Rotas de evacuação")]
[Produces("application/json")]
public class RotasEvacuacaoController : ControllerBase
{
    private readonly AppDbContext _db;

    public RotasEvacuacaoController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lista as rotas. Filtros opcionais: abrigo e só as rotas acessíveis.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RotaEvacuacaoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RotaEvacuacaoResponse>>> Listar(
        [FromQuery] int? abrigoId,
        [FromQuery] bool apenasAcessiveis = false)
    {
        var consulta = _db.RotasEvacuacao.AsNoTracking().Include(r => r.Abrigo).AsQueryable();

        if (abrigoId is not null)
        {
            consulta = consulta.Where(r => r.AbrigoId == abrigoId);
        }

        if (apenasAcessiveis)
        {
            consulta = consulta.Where(r => r.Acessivel && r.Ativa);
        }

        var rotas = await consulta.OrderBy(r => r.Id).ToListAsync();

        return Ok(rotas.Select(RotaEvacuacaoResponse.De).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RotaEvacuacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RotaEvacuacaoResponse>> BuscarPorId(int id)
    {
        var rota = await _db.RotasEvacuacao
            .AsNoTracking()
            .Include(r => r.Abrigo)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rota is null)
        {
            return NaoEncontrada(id);
        }

        return Ok(RotaEvacuacaoResponse.De(rota));
    }

    /// <summary>Cadastra uma rota para um abrigo que já existe.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RotaEvacuacaoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RotaEvacuacaoResponse>> Criar([FromBody] RotaEvacuacaoRequest request)
    {
        var abrigo = await _db.Abrigos.FindAsync(request.AbrigoId);
        if (abrigo is null)
        {
            return AbrigoInexistente(request.AbrigoId);
        }

        var rota = new RotaEvacuacao { CriadoEm = DateTime.UtcNow };
        request.CopiarPara(rota);

        _db.RotasEvacuacao.Add(rota);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(BuscarPorId), new { id = rota.Id }, RotaEvacuacaoResponse.De(rota));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] RotaEvacuacaoRequest request)
    {
        var rota = await _db.RotasEvacuacao.FindAsync(id);
        if (rota is null)
        {
            return NaoEncontrada(id);
        }

        if (!await _db.Abrigos.AnyAsync(a => a.Id == request.AbrigoId))
        {
            return AbrigoInexistente(request.AbrigoId);
        }

        request.CopiarPara(rota);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        var rota = await _db.RotasEvacuacao.FindAsync(id);
        if (rota is null)
        {
            return NaoEncontrada(id);
        }

        _db.RotasEvacuacao.Remove(rota);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private NotFoundObjectResult NaoEncontrada(int id) =>
        NotFound(new ErroResponse(StatusCodes.Status404NotFound, $"Rota de evacuação {id} não encontrada."));

    // O abrigo informado no corpo não existe: é dado inválido, então 400 (e não 404).
    private BadRequestObjectResult AbrigoInexistente(int abrigoId) =>
        BadRequest(new ErroResponse(
            StatusCodes.Status400BadRequest,
            "Dados inválidos.",
            new Dictionary<string, string[]>
            {
                ["abrigoId"] = new[] { $"O abrigo {abrigoId} não existe." },
            }));
}
