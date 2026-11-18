using System.Text.Json;
using CitizenTech.Abrigos.Data;
using CitizenTech.Abrigos.Erros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// A senha do Oracle NUNCA fica no repositório. Ela vem de:
//  - dotnet user-secrets (máquina de desenvolvimento), ou
//  - variável de ambiente ConnectionStrings__Oracle (container / VM).
var connectionString = builder.Configuration.GetConnectionString("Oracle");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Oracle' não configurada. Veja a seção 'Como executar' do README " +
        "(dotnet user-secrets ou variável de ambiente ConnectionStrings__Oracle).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString, oracle =>
        // O Oracle da FIAP é 19c: gera SQL compatível com essa versão.
        oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Dado inválido (Data Annotations) -> 400 no mesmo formato JSON dos outros erros.
        options.InvalidModelStateResponseFactory = context =>
        {
            // Corpo que nem é JSON (ex.: falta uma aspa): uma mensagem só, em português.
            if (context.ModelState.Keys.Any(campo => campo.StartsWith('$')))
            {
                return new BadRequestObjectResult(new ErroResponse(
                    StatusCodes.Status400BadRequest,
                    "Dados inválidos.",
                    new Dictionary<string, string[]>
                    {
                        ["corpo"] = new[] { "O corpo da requisição não é um JSON válido." },
                    }));
            }

            var detalhes = context.ModelState
                .Where(campo => campo.Value?.Errors.Count > 0)
                .ToDictionary(
                    // "Uf" -> "uf": mesmo nome do campo no JSON.
                    campo => JsonNamingPolicy.CamelCase.ConvertName(campo.Key),
                    campo => campo.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return new BadRequestObjectResult(
                new ErroResponse(StatusCodes.Status400BadRequest, "Dados inválidos.", detalhes));
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Qualquer exceção não tratada vira JSON (nunca uma página HTML com stack trace).
app.UseExceptionHandler(erroApp => erroApp.Run(async context =>
{
    var excecao = context.Features
        .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    var (status, mensagem) = excecao switch
    {
        DbUpdateException => (StatusCodes.Status409Conflict,
            "A operação viola uma regra do banco de dados."),
        _ => (StatusCodes.Status500InternalServerError,
            "Erro inesperado no servidor."),
    };

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new ErroResponse(status, mensagem));
}));

// Swagger fica ligado em todos os ambientes para facilitar o teste (inclusive no container).
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
