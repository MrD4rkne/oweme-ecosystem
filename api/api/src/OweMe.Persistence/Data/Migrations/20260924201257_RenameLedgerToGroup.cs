using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OweMe.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameLedgerToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Ledgers",
                table: "Ledgers");

            migrationBuilder.RenameTable(
                name: "Ledgers",
                newName: "Groups");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Ledgers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ledgers",
                table: "Ledgers",
                column: "Id");
        }
    }
}
