using Microsoft.EntityFrameworkCore.Migrations;

namespace DinoShare.Migrations
{
    public partial class NetworkCredentials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "FolderDirectories",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCredentials",
                table: "FolderDirectories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "FolderDirectories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "FolderDirectories");

            migrationBuilder.DropColumn(
                name: "RequireCredentials",
                table: "FolderDirectories");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "FolderDirectories");
        }
    }
}
