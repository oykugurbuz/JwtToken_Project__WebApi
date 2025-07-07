using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtTokenProject.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IdentityNumber",
                table: "AppUserInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "AppUserInfos");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "AppUserInfos");
        }
    }
}
