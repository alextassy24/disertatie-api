using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Products_ProductID",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "Locations",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Locations_ProductID",
                table: "Locations",
                newName: "IX_Locations_ProductId");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Locations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductID",
                table: "Locations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Products_ProductId",
                table: "Locations",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Products_ProductId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ProductID",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Locations",
                newName: "ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_Locations_ProductId",
                table: "Locations",
                newName: "IX_Locations_ProductID");

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "Locations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Products_ProductID",
                table: "Locations",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
