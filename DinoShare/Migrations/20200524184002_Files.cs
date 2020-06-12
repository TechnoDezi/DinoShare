using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DinoShare.Migrations
{
    public partial class Files : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FolderDirectoryFiles",
                columns: table => new
                {
                    FolderDirectoryFileID = table.Column<Guid>(nullable: false),
                    FolderDirectoryID = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    FileType = table.Column<string>(nullable: true),
                    FileExtention = table.Column<string>(nullable: true),
                    FullPath = table.Column<string>(nullable: true),
                    SizeMB = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderDirectoryFiles", x => x.FolderDirectoryFileID);
                    table.ForeignKey(
                        name: "FK_FolderDirectoryFiles_FolderDirectories_FolderDirectoryID",
                        column: x => x.FolderDirectoryID,
                        principalTable: "FolderDirectories",
                        principalColumn: "FolderDirectoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderDirectoryFiles_FolderDirectoryID",
                table: "FolderDirectoryFiles",
                column: "FolderDirectoryID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderDirectoryFiles");
        }
    }
}
