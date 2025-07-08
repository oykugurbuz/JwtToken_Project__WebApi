using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtTokenProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AppUserInfos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AppUserInfos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserInfos_Email",
                table: "AppUserInfos",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserInfos_IdentityNumber",
                table: "AppUserInfos",
                column: "IdentityNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserInfos_UserName",
                table: "AppUserInfos",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppUserInfos_Email",
                table: "AppUserInfos");

            migrationBuilder.DropIndex(
                name: "IX_AppUserInfos_IdentityNumber",
                table: "AppUserInfos");

            migrationBuilder.DropIndex(
                name: "IX_AppUserInfos_UserName",
                table: "AppUserInfos");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AppUserInfos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AppUserInfos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
