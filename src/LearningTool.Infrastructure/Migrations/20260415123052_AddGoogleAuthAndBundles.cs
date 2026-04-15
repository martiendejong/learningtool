using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuthAndBundles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingGoogleVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    GoogleId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingGoogleVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BundleSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BundleSkills_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BundleSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationBundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    IsUnlimited = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationBundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationBundles_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationBundles_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBundles_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_Name",
                table: "Bundles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BundleSkills_BundleId_SkillId",
                table: "BundleSkills",
                columns: new[] { "BundleId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BundleSkills_SkillId",
                table: "BundleSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationBundles_BundleId",
                table: "OrganizationBundles",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationBundles_OrganizationId_BundleId",
                table: "OrganizationBundles",
                columns: new[] { "OrganizationId", "BundleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingGoogleVerifications_Email",
                table: "PendingGoogleVerifications",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UserBundles_BundleId",
                table: "UserBundles",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBundles_UserId_BundleId",
                table: "UserBundles",
                columns: new[] { "UserId", "BundleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BundleSkills");

            migrationBuilder.DropTable(
                name: "OrganizationBundles");

            migrationBuilder.DropTable(
                name: "PendingGoogleVerifications");

            migrationBuilder.DropTable(
                name: "UserBundles");

            migrationBuilder.DropTable(
                name: "Bundles");

            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "AspNetUsers");
        }
    }
}
