using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesAPI.Migrations
{
    /// <inheritdoc />
    public partial class arctiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrixUnitaire",
                table: "Articles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteStock",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrixUnitaire",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "QuantiteStock",
                table: "Articles");
        }
    }
}
