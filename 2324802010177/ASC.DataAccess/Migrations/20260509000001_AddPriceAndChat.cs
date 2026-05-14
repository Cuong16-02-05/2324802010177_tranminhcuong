using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASC.DataAccess.Migrations
{
    public partial class AddPriceAndChat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "MasterDataValues",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    UniqueId         = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServiceRequestId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FromEmail        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromDisplayName  = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToEmail          = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message          = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentDate         = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead           = table.Column<bool>(type: "bit", nullable: false),
                    SenderRole       = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate      = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate      = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted        = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.UniqueId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ServiceRequestId",
                table: "ChatMessages",
                column: "ServiceRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ChatMessages");
            migrationBuilder.DropColumn(name: "Price", table: "MasterDataValues");
        }
    }
}
