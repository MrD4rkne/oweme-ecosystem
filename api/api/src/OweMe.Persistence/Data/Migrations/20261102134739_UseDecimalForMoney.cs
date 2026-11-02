using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OweMe.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseDecimalForMoney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Splits",
                type: "numeric(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Splits",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,4)",
                oldPrecision: 19,
                oldScale: 4);
        }
    }
}
