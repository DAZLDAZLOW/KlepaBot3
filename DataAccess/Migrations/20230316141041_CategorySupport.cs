using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class CategorySupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "PrivateChannelsCategoryId",
                table: "ChannelsSetup",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "PublicChannelsCategoryId",
                table: "ChannelsSetup",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateChannelsCategoryId",
                table: "ChannelsSetup");

            migrationBuilder.DropColumn(
                name: "PublicChannelsCategoryId",
                table: "ChannelsSetup");
        }
    }
}
