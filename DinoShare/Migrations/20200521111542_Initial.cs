using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DinoShare.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    EmailTemplateID = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    EventCode = table.Column<string>(nullable: true),
                    TemplateBody = table.Column<string>(nullable: true),
                    TemplateBodyTags = table.Column<string>(nullable: true),
                    CreatedUserID = table.Column<Guid>(nullable: true),
                    EditUserID = table.Column<Guid>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: true),
                    EditDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.EmailTemplateID);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfiguration",
                columns: table => new
                {
                    SystemConfigurationID = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    EventCode = table.Column<string>(nullable: true),
                    ConfigValue = table.Column<string>(nullable: true),
                    CreatedUserID = table.Column<Guid>(nullable: true),
                    EditUserID = table.Column<Guid>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: true),
                    EditDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfiguration", x => x.SystemConfigurationID);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryTokensType",
                columns: table => new
                {
                    TemporaryTokensTypeID = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    EventCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryTokensType", x => x.TemporaryTokensTypeID);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleID = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    EventCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    LoginTries = table.Column<int>(nullable: false),
                    IsSuspended = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    CreatedUserID = table.Column<Guid>(nullable: false),
                    EditUserID = table.Column<Guid>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: true),
                    EditDateTime = table.Column<DateTime>(nullable: true),
                    Timezone = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    Surname = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLog",
                columns: table => new
                {
                    ApplicationLogID = table.Column<Guid>(nullable: false),
                    LogDate = table.Column<DateTime>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    LogOriginator = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    UserID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLog", x => x.ApplicationLogID);
                    table.ForeignKey(
                        name: "FK_ApplicationLog_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LinkUserRole",
                columns: table => new
                {
                    LinkUserRoleID = table.Column<Guid>(nullable: false),
                    UserRoleID = table.Column<Guid>(nullable: false),
                    UserID = table.Column<Guid>(nullable: false),
                    CreatedUserID = table.Column<Guid>(nullable: false),
                    EditUserID = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkUserRole", x => x.LinkUserRoleID);
                    table.ForeignKey(
                        name: "FK_LinkUserRole_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LinkUserRole_UserRoles_UserRoleID",
                        column: x => x.UserRoleID,
                        principalTable: "UserRoles",
                        principalColumn: "UserRoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTemporaryToken",
                columns: table => new
                {
                    UserTemporaryTokenID = table.Column<Guid>(nullable: false),
                    TemporaryTokensTypeID = table.Column<Guid>(nullable: false),
                    UserID = table.Column<Guid>(nullable: false),
                    TokenExpiryDate = table.Column<DateTime>(nullable: false),
                    CreatedUserID = table.Column<Guid>(nullable: true),
                    EditUserID = table.Column<Guid>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: true),
                    EditDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTemporaryToken", x => x.UserTemporaryTokenID);
                    table.ForeignKey(
                        name: "FK_UserTemporaryToken_TemporaryTokensType_TemporaryTokensTypeID",
                        column: x => x.TemporaryTokensTypeID,
                        principalTable: "TemporaryTokensType",
                        principalColumn: "TemporaryTokensTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTemporaryToken_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLog_UserID",
                table: "ApplicationLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LinkUserRole_UserID",
                table: "LinkUserRole",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LinkUserRole_UserRoleID",
                table: "LinkUserRole",
                column: "UserRoleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTemporaryToken_TemporaryTokensTypeID",
                table: "UserTemporaryToken",
                column: "TemporaryTokensTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTemporaryToken_UserID",
                table: "UserTemporaryToken",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLog");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "LinkUserRole");

            migrationBuilder.DropTable(
                name: "SystemConfiguration");

            migrationBuilder.DropTable(
                name: "UserTemporaryToken");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "TemporaryTokensType");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
