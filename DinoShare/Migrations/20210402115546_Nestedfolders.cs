using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DinoShare.Migrations
{
    public partial class Nestedfolders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentFolderID",
                table: "Folders",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentFolderID",
                table: "Folders",
                column: "ParentFolderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Folders_ParentFolderID",
                table: "Folders",
                column: "ParentFolderID",
                principalTable: "Folders",
                principalColumn: "FolderID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Folders_ParentFolderID",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_ParentFolderID",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "ParentFolderID",
                table: "Folders");
        }
    }
}
