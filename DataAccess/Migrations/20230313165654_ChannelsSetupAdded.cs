using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class ChannelsSetupAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateMotherChannelId",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "PublicMotherChannelId",
                table: "Servers");

            migrationBuilder.AddColumn<int>(
                name: "ChannelsSetupId",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChannelsSetup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicMotherChannelId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    PrivateMotherChannelId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    PublicChannelDefaultName = table.Column<string>(type: "TEXT", nullable: false),
                    AddToPublicChannelCounter = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrivateChannelDefaultName = table.Column<string>(type: "TEXT", nullable: false),
                    AddToPrivateChannelCounter = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelsSetup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ChannelsSetupId",
                table: "Servers",
                column: "ChannelsSetupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_ChannelsSetup_ChannelsSetupId",
                table: "Servers",
                column: "ChannelsSetupId",
                principalTable: "ChannelsSetup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_ChannelsSetup_ChannelsSetupId",
                table: "Servers");

            migrationBuilder.DropTable(
                name: "ChannelsSetup");

            migrationBuilder.DropIndex(
                name: "IX_Servers_ChannelsSetupId",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "ChannelsSetupId",
                table: "Servers");

            migrationBuilder.AddColumn<ulong>(
                name: "PrivateMotherChannelId",
                table: "Servers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "PublicMotherChannelId",
                table: "Servers",
                type: "INTEGER",
                nullable: true);
        }
    }
}
