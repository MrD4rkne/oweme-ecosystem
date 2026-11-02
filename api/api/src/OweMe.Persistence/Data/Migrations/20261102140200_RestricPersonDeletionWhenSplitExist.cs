using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OweMe.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestricPersonDeletionWhenSplitExist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Splits_Personas_PersonaId",
                table: "Splits");

            migrationBuilder.AddForeignKey(
                name: "FK_Splits_Personas_PersonaId",
                table: "Splits",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Splits_Personas_PersonaId",
                table: "Splits");

            migrationBuilder.AddForeignKey(
                name: "FK_Splits_Personas_PersonaId",
                table: "Splits",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
