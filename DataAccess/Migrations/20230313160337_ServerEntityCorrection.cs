using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class ServerEntityCorrection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "Servers");

            migrationBuilder.AlterColumn<ulong>(
                name: "PublicMotherChannelId",
                table: "Servers",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<ulong>(
                name: "PrivateMotherChannelId",
                table: "Servers",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Servers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Servers");

            migrationBuilder.AlterColumn<ulong>(
                name: "PublicMotherChannelId",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "PrivateMotherChannelId",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "ServerId",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
