using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASC.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNegotiationCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NegotiationCount",
                table: "ServiceRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NegotiationCount",
                table: "ServiceRequests");
        }
    }
}
