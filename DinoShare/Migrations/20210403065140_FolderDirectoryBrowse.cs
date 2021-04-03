using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DinoShare.Migrations
{
    public partial class FolderDirectoryBrowse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDirectory",
                table: "FolderDirectoryFiles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentFolderDirectoryFileID",
                table: "FolderDirectoryFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDirectory",
                table: "FolderDirectoryFiles");

            migrationBuilder.DropColumn(
                name: "ParentFolderDirectoryFileID",
                table: "FolderDirectoryFiles");
        }
    }
}
