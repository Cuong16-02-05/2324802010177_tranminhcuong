using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASC.DataAccess.Migrations
{
    /// <summary>
    /// Chuẩn hóa kiểu cột QuoteStatus và PaymentStatus trên ServiceRequests.
    /// (Cột Price trên MasterDataValues đã được thêm trong AddPriceAndChat — bỏ qua ở đây)
    /// </summary>
    public partial class AddPriceToMasterDataValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuoteStatus",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuoteStatus",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
