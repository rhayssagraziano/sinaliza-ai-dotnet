using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CitizenTech.Abrigos.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoAbrigoERotaEvacuacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ABRIGO",
                columns: table => new
                {
                    ID_ABRIGO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    ENDERECO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    BAIRRO = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    MUNICIPIO = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    UF = table.Column<string>(type: "NCHAR(2)", fixedLength: true, maxLength: 2, nullable: false),
                    CAPACIDADE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OCUPACAO_ATUAL = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ACESSIVEL = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ABRIGO", x => x.ID_ABRIGO);
                    table.CheckConstraint("CK_ABRIGO_CAPACIDADE", "CAPACIDADE > 0");
                    table.CheckConstraint("CK_ABRIGO_OCUPACAO", "OCUPACAO_ATUAL >= 0 AND OCUPACAO_ATUAL <= CAPACIDADE");
                });

            migrationBuilder.CreateTable(
                name: "ROTA_EVACUACAO",
                columns: table => new
                {
                    ID_ROTA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_ABRIGO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PONTO_PARTIDA = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    TRAJETO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    DISTANCIA_METROS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TEMPO_ESTIMADO_MIN = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ACESSIVEL = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    ATIVA = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROTA_EVACUACAO", x => x.ID_ROTA);
                    table.CheckConstraint("CK_ROTA_DISTANCIA", "DISTANCIA_METROS > 0");
                    table.CheckConstraint("CK_ROTA_TEMPO", "TEMPO_ESTIMADO_MIN > 0");
                    table.ForeignKey(
                        name: "FK_ROTA_ABRIGO",
                        column: x => x.ID_ABRIGO,
                        principalTable: "ABRIGO",
                        principalColumn: "ID_ABRIGO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ROTA_ABRIGO",
                table: "ROTA_EVACUACAO",
                column: "ID_ABRIGO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ROTA_EVACUACAO");

            migrationBuilder.DropTable(
                name: "ABRIGO");
        }
    }
}
