using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalSoftDeleteFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boeken_Categories_CategoryId",
                table: "Boeken");

            migrationBuilder.DropIndex(
                name: "IX_Leden_Email",
                table: "Leden");

            migrationBuilder.DropIndex(
                name: "IX_Boeken_ISBN",
                table: "Boeken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Categorieen");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categorieen",
                newName: "Naam");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categorieen",
                newName: "CategorieId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categorieen",
                table: "Categorieen",
                column: "CategorieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Boeken_Categorieen_CategoryId",
                table: "Boeken",
                column: "CategoryId",
                principalTable: "Categorieen",
                principalColumn: "CategorieId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boeken_Categorieen_CategoryId",
                table: "Boeken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categorieen",
                table: "Categorieen");

            migrationBuilder.RenameTable(
                name: "Categorieen",
                newName: "Categories");

            migrationBuilder.RenameColumn(
                name: "Naam",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CategorieId",
                table: "Categories",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Boeken_Categories_CategoryId",
                table: "Boeken",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
