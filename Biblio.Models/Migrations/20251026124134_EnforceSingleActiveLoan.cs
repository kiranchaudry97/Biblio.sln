// Doel: Unieke gefilterde index toevoegt zodat max. één actieve uitlening per boek mogelijk is.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Models.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActiveLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Unieke gefilterde index: maximaal één actieve (niet-ingeleverde) uitlening per boek
            migrationBuilder.CreateIndex(
                name: "IX_Uitleningen_BoekId_Actief",
                table: "Uitleningen",
                column: "BoekId",
                unique: true,
                filter: "([IngeleverdOp] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Uitleningen_BoekId_Actief",
                table: "Uitleningen");
        }
    }
}
