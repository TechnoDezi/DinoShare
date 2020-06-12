using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DinoShare.Migrations
{
    public partial class Folder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    FolderID = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.FolderID);
                });

            migrationBuilder.CreateTable(
                name: "FolderDirectories",
                columns: table => new
                {
                    FolderDirectoryID = table.Column<Guid>(nullable: false),
                    FolderID = table.Column<Guid>(nullable: false),
                    FolderPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderDirectories", x => x.FolderDirectoryID);
                    table.ForeignKey(
                        name: "FK_FolderDirectories_Folders_FolderID",
                        column: x => x.FolderID,
                        principalTable: "Folders",
                        principalColumn: "FolderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderUsers",
                columns: table => new
                {
                    FolderUserID = table.Column<Guid>(nullable: false),
                    FolderID = table.Column<Guid>(nullable: false),
                    UserID = table.Column<Guid>(nullable: false),
                    AllowEdit = table.Column<bool>(nullable: false),
                    AllowDelete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderUsers", x => x.FolderUserID);
                    table.ForeignKey(
                        name: "FK_FolderUsers_Folders_FolderID",
                        column: x => x.FolderID,
                        principalTable: "Folders",
                        principalColumn: "FolderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolderUsers_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderDirectories_FolderID",
                table: "FolderDirectories",
                column: "FolderID");

            migrationBuilder.CreateIndex(
                name: "IX_FolderUsers_FolderID",
                table: "FolderUsers",
                column: "FolderID");

            migrationBuilder.CreateIndex(
                name: "IX_FolderUsers_UserID",
                table: "FolderUsers",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderDirectories");

            migrationBuilder.DropTable(
                name: "FolderUsers");

            migrationBuilder.DropTable(
                name: "Folders");
        }
    }
}
