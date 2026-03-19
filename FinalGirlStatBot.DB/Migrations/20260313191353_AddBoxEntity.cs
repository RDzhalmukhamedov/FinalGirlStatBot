using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinalGirlStatBot.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddBoxEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoxId",
                table: "Locations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoxId",
                table: "Killers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoxId",
                table: "FinalGirls",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Boxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Season = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    KillerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boxes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_BoxId",
                table: "Locations",
                column: "BoxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Killers_BoxId",
                table: "Killers",
                column: "BoxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalGirls_BoxId",
                table: "FinalGirls",
                column: "BoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalGirls_Boxes_BoxId",
                table: "FinalGirls",
                column: "BoxId",
                principalTable: "Boxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Killers_Boxes_BoxId",
                table: "Killers",
                column: "BoxId",
                principalTable: "Boxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Boxes_BoxId",
                table: "Locations",
                column: "BoxId",
                principalTable: "Boxes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalGirls_Boxes_BoxId",
                table: "FinalGirls");

            migrationBuilder.DropForeignKey(
                name: "FK_Killers_Boxes_BoxId",
                table: "Killers");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Boxes_BoxId",
                table: "Locations");

            migrationBuilder.DropTable(
                name: "Boxes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_BoxId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Killers_BoxId",
                table: "Killers");

            migrationBuilder.DropIndex(
                name: "IX_FinalGirls_BoxId",
                table: "FinalGirls");

            migrationBuilder.DropColumn(
                name: "BoxId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "BoxId",
                table: "Killers");

            migrationBuilder.DropColumn(
                name: "BoxId",
                table: "FinalGirls");
        }
    }
}
