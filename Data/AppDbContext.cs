using CitizenTech.Abrigos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CitizenTech.Abrigos.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Abrigo> Abrigos => Set<Abrigo>();

    public DbSet<RotaEvacuacao> RotasEvacuacao => Set<RotaEvacuacao>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Datas são gravadas em UTC; ao ler de volta, marca como UTC para o JSON sair com "Z".
        configurationBuilder.Properties<DateTime>().HaveConversion<DataUtcConverter>();
    }

    private sealed class DataUtcConverter : ValueConverter<DateTime, DateTime>
    {
        public DataUtcConverter()
            : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Nomes em MAIÚSCULAS para consultar no Oracle sem aspas: SELECT * FROM ABRIGO;
        modelBuilder.Entity<Abrigo>(e =>
        {
            e.ToTable("ABRIGO");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("ID_ABRIGO").ValueGeneratedOnAdd();
            e.Property(a => a.Nome).HasColumnName("NOME").HasMaxLength(120).IsRequired();
            e.Property(a => a.Endereco).HasColumnName("ENDERECO").HasMaxLength(200).IsRequired();
            e.Property(a => a.Bairro).HasColumnName("BAIRRO").HasMaxLength(80).IsRequired();
            e.Property(a => a.Municipio).HasColumnName("MUNICIPIO").HasMaxLength(80).IsRequired();
            e.Property(a => a.Uf).HasColumnName("UF").HasMaxLength(2).IsFixedLength().IsRequired();
            e.Property(a => a.Capacidade).HasColumnName("CAPACIDADE");
            e.Property(a => a.OcupacaoAtual).HasColumnName("OCUPACAO_ATUAL");
            e.Property(a => a.Acessivel).HasColumnName("ACESSIVEL");
            e.Property(a => a.Ativo).HasColumnName("ATIVO");
            e.Property(a => a.Telefone).HasColumnName("TELEFONE").HasMaxLength(20);
            e.Property(a => a.CriadoEm).HasColumnName("CRIADO_EM");

            // Regras também no banco, não só na API.
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ABRIGO_CAPACIDADE", "CAPACIDADE > 0");
                t.HasCheckConstraint("CK_ABRIGO_OCUPACAO", "OCUPACAO_ATUAL >= 0 AND OCUPACAO_ATUAL <= CAPACIDADE");
            });
        });

        modelBuilder.Entity<RotaEvacuacao>(e =>
        {
            e.ToTable("ROTA_EVACUACAO");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("ID_ROTA").ValueGeneratedOnAdd();
            e.Property(r => r.AbrigoId).HasColumnName("ID_ABRIGO");
            e.Property(r => r.PontoPartida).HasColumnName("PONTO_PARTIDA").HasMaxLength(150).IsRequired();
            e.Property(r => r.Trajeto).HasColumnName("TRAJETO").HasMaxLength(500).IsRequired();
            e.Property(r => r.DistanciaMetros).HasColumnName("DISTANCIA_METROS");
            e.Property(r => r.TempoEstimadoMin).HasColumnName("TEMPO_ESTIMADO_MIN");
            e.Property(r => r.Acessivel).HasColumnName("ACESSIVEL");
            e.Property(r => r.Ativa).HasColumnName("ATIVA");
            e.Property(r => r.CriadoEm).HasColumnName("CRIADO_EM");

            // Relação 1:N: um ABRIGO tem várias ROTA_EVACUACAO.
            // Cascade: apagar o abrigo apaga as rotas dele.
            e.HasOne(r => r.Abrigo)
                .WithMany(a => a.Rotas)
                .HasForeignKey(r => r.AbrigoId)
                .HasConstraintName("FK_ROTA_ABRIGO")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(r => r.AbrigoId).HasDatabaseName("IX_ROTA_ABRIGO");

            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ROTA_DISTANCIA", "DISTANCIA_METROS > 0");
                t.HasCheckConstraint("CK_ROTA_TEMPO", "TEMPO_ESTIMADO_MIN > 0");
            });
        });
    }
}
