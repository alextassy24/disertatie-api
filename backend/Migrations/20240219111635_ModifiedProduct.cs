using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WearerName",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "WearerAge",
                table: "Products",
                newName: "WearerID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_WearerID",
                table: "Products",
                column: "WearerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Wearers_WearerID",
                table: "Products",
                column: "WearerID",
                principalTable: "Wearers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Wearers_WearerID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_WearerID",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "WearerID",
                table: "Products",
                newName: "WearerAge");

            migrationBuilder.AddColumn<string>(
                name: "WearerName",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
