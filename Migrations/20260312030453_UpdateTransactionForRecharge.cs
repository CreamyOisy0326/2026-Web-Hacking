using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoMarket.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionForRecharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Photos_PhotoId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "PhotoId",
                table: "Transactions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Photos_PhotoId",
                table: "Transactions",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Photos_PhotoId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "PhotoId",
                table: "Transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Photos_PhotoId",
                table: "Transactions",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
