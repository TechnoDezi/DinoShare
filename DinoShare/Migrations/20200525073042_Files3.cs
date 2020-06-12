using Microsoft.EntityFrameworkCore.Migrations;

namespace DinoShare.Migrations
{
    public partial class Files3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUploadDirectory",
                table: "FolderDirectories",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUploadDirectory",
                table: "FolderDirectories");
        }
    }
}
