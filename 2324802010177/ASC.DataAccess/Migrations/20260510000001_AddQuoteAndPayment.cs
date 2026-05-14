using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASC.DataAccess.Migrations
{
    public partial class AddQuoteAndPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedPrice",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineerNotes",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuoteStatus",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EstimatedPrice", table: "ServiceRequests");
            migrationBuilder.DropColumn(name: "EngineerNotes", table: "ServiceRequests");
            migrationBuilder.DropColumn(name: "QuoteStatus", table: "ServiceRequests");
            migrationBuilder.DropColumn(name: "FinalPrice", table: "ServiceRequests");
            migrationBuilder.DropColumn(name: "PaymentStatus", table: "ServiceRequests");
        }
    }
}
