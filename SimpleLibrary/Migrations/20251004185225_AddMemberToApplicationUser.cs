using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Members",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ApplicationUserId",
                table: "Members",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_ApplicationUserId",
                table: "Members",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_ApplicationUserId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_ApplicationUserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "AspNetUsers");
        }
    }
}
