// Doel: Hernoemt tabellen en kolommen naar Nederlandse namen en herstelt unieke indexen.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Models.Migrations
{
    /// <inheritdoc />
    public partial class MapDutchNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Categories_CategoryId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Books_BookId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Members_MemberId",
                table: "Loans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Members",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_Email",
                table: "Members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Loans",
                table: "Loans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Books",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_Isbn",
                table: "Books");

            migrationBuilder.RenameTable(
                name: "Members",
                newName: "Leden");

            migrationBuilder.RenameTable(
                name: "Loans",
                newName: "Uitleningen");

            migrationBuilder.RenameTable(
                name: "Books",
                newName: "Boeken");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Leden",
                newName: "Tel");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Leden",
                newName: "Naam");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Leden",
                newName: "Voornaam");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Leden",
                newName: "Adres");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Leden",
                newName: "LidId");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Uitleningen",
                newName: "StartDatum");

            migrationBuilder.RenameColumn(
                name: "ReturnedAt",
                table: "Uitleningen",
                newName: "IngeleverdOp");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Uitleningen",
                newName: "LidId");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "Uitleningen",
                newName: "EindDatum");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "Uitleningen",
                newName: "BoekId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Uitleningen",
                newName: "UitleningId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_MemberId",
                table: "Uitleningen",
                newName: "IX_Uitleningen_LidId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_BookId",
                table: "Uitleningen",
                newName: "IX_Uitleningen_BoekId");

            migrationBuilder.RenameColumn(
                name: "Isbn",
                table: "Boeken",
                newName: "ISBN");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Boeken",
                newName: "Titel");

            migrationBuilder.RenameColumn(
                name: "Author",
                table: "Boeken",
                newName: "Auteur");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Boeken",
                newName: "BoekId");

            migrationBuilder.RenameIndex(
                name: "IX_Books_CategoryId",
                table: "Boeken",
                newName: "IX_Boeken_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Leden",
                table: "Leden",
                column: "LidId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Uitleningen",
                table: "Uitleningen",
                column: "UitleningId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Boeken",
                table: "Boeken",
                column: "BoekId");

            migrationBuilder.AddForeignKey(
                name: "FK_Boeken_Categories_CategoryId",
                table: "Boeken",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Uitleningen_Boeken_BoekId",
                table: "Uitleningen",
                column: "BoekId",
                principalTable: "Boeken",
                principalColumn: "BoekId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Uitleningen_Leden_LidId",
                table: "Uitleningen",
                column: "LidId",
                principalTable: "Leden",
                principalColumn: "LidId",
                onDelete: ReferentialAction.Cascade);

            // Hermaak unieke indexen met NL-namen en kolommen
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
                filter: "([ISBN] IS NOT NULL AND [ISBN] <> '')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Verwijder NL-indexen vóór terugdraaien
            migrationBuilder.DropIndex(
                name: "IX_Leden_Email",
                table: "Leden");

            migrationBuilder.DropIndex(
                name: "IX_Boeken_ISBN",
                table: "Boeken");

            migrationBuilder.DropForeignKey(
                name: "FK_Boeken_Categories_CategoryId",
                table: "Boeken");

            migrationBuilder.DropForeignKey(
                name: "FK_Uitleningen_Boeken_BoekId",
                table: "Uitleningen");

            migrationBuilder.DropForeignKey(
                name: "FK_Uitleningen_Leden_LidId",
                table: "Uitleningen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Uitleningen",
                table: "Uitleningen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Leden",
                table: "Leden");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Boeken",
                table: "Boeken");

            migrationBuilder.RenameTable(
                name: "Uitleningen",
                newName: "Loans");

            migrationBuilder.RenameTable(
                name: "Leden",
                newName: "Members");

            migrationBuilder.RenameTable(
                name: "Boeken",
                newName: "Books");

            migrationBuilder.RenameColumn(
                name: "StartDatum",
                table: "Loans",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "LidId",
                table: "Loans",
                newName: "MemberId");

            migrationBuilder.RenameColumn(
                name: "IngeleverdOp",
                table: "Loans",
                newName: "ReturnedAt");

            migrationBuilder.RenameColumn(
                name: "EindDatum",
                table: "Loans",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "BoekId",
                table: "Loans",
                newName: "BookId");

            migrationBuilder.RenameColumn(
                name: "UitleningId",
                table: "Loans",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Uitleningen_LidId",
                table: "Loans",
                newName: "IX_Loans_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Uitleningen_BoekId",
                table: "Loans",
                newName: "IX_Loans_BookId");

            migrationBuilder.RenameColumn(
                name: "Voornaam",
                table: "Members",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Tel",
                table: "Members",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "Naam",
                table: "Members",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Adres",
                table: "Members",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "LidId",
                table: "Members",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ISBN",
                table: "Books",
                newName: "Isbn");

            migrationBuilder.RenameColumn(
                name: "Titel",
                table: "Books",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Auteur",
                table: "Books",
                newName: "Author");

            migrationBuilder.RenameColumn(
                name: "BoekId",
                table: "Books",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Boeken_CategoryId",
                table: "Books",
                newName: "IX_Books_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Loans",
                table: "Loans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members",
                table: "Members",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Books",
                table: "Books",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true,
                filter: "([Email] IS NOT NULL AND [Email] <> '')");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Isbn",
                table: "Books",
                column: "Isbn",
                unique: true,
                filter: "([Isbn] IS NOT NULL AND [Isbn] <> '')");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Categories_CategoryId",
                table: "Books",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Books_BookId",
                table: "Loans",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Members_MemberId",
                table: "Loans",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
