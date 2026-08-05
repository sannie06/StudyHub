using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetOtpToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailOtpCode",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "NguoiDung",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAt",
                table: "NguoiDung",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetOtp",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetOtpExpiresAt",
                table: "NguoiDung",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "NguoiDung",
                keyColumn: "MaNguoiDung",
                keyValue: 1,
                columns: new[] { "EmailConfirmationToken", "EmailOtpCode", "IsEmailConfirmed", "OtpExpiresAt", "PasswordResetOtp", "ResetOtpExpiresAt" },
                values: new object[] { null, null, false, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "EmailOtpCode",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAt",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtp",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "ResetOtpExpiresAt",
                table: "NguoiDung");
        }
    }
}
