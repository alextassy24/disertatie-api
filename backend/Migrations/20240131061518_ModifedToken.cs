using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ModifedToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Tokens",
                newName: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_UserID",
                table: "Tokens",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_AspNetUsers_UserID",
                table: "Tokens",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_AspNetUsers_UserID",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_UserID",
                table: "Tokens");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Tokens",
                newName: "Email");
        }
    }
}
