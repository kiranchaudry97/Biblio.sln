// Doel: Synchroniseert model-snapshot via herstel van indexen en filters.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Models.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Uitleningen_BoekId",
                table: "Uitleningen");

            migrationBuilder.CreateIndex(
                name: "IX_Uitleningen_BoekId_Actief",
                table: "Uitleningen",
                column: "BoekId",
                unique: true,
                filter: "([IngeleverdOp] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Leden_Email",
                table: "Leden",
                column: "Email",
                unique: true,
                filter: "([Email] IS NOT NULL AND [Email] <> '')");

            migrationBuilder.CreateIndex(
                name: "IX_Boeken_ISBN",
                table: "Boeken",
                column: "ISBN",
                unique: true,
                filter: "([Isbn] IS NOT NULL AND [Isbn] <> '')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Uitleningen_BoekId_Actief",
                table: "Uitleningen");

            migrationBuilder.DropIndex(
                name: "IX_Leden_Email",
                table: "Leden");

            migrationBuilder.DropIndex(
                name: "IX_Boeken_ISBN",
                table: "Boeken");

            migrationBuilder.CreateIndex(
                name: "IX_Uitleningen_BoekId",
                table: "Uitleningen",
                column: "BoekId");
        }
    }
}
