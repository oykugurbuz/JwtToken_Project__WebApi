using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtTokenProject.Migrations
{
    /// <inheritdoc />
    public partial class AppUserInfoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AppUserInfos");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "AppUserInfos");

            migrationBuilder.DropColumn(
                name: "RememberMe",
                table: "AppUserInfos");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "AppUserInfos");

            migrationBuilder.DropColumn(
                name: "UserTypeName",
                table: "AppUserInfos");

            migrationBuilder.DropColumn(
                name: "FailedAttempt",
                table: "AppUserInfos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAttempt",
                table: "AppUserInfos",
                type:"int",
                nullable:false,
                defaultValue: 0
                );

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AppUserInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "AppUserInfos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RememberMe",
                table: "AppUserInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "AppUserInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserTypeName",
                table: "AppUserInfos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
