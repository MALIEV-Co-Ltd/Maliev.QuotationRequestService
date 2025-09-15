using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Maliev.QuotationRequestService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuotationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Requirements = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Industry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProjectTimeline = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EstimatedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PreferredContactMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    AssignedToTeamMember = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    QuotedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CustomerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuotationRequestComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationRequestId = table.Column<int>(type: "integer", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthorEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CommentType = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationRequestComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationRequestComments_QuotationRequests_QuotationRequest~",
                        column: x => x.QuotationRequestId,
                        principalTable: "QuotationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationRequestFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationRequestId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ObjectName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UploadServiceFileId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationRequestFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationRequestFiles_QuotationRequests_QuotationRequestId",
                        column: x => x.QuotationRequestId,
                        principalTable: "QuotationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationRequestStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationRequestId = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ChangedByTeamMember = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationRequestStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationRequestStatusHistories_QuotationRequests_Quotation~",
                        column: x => x.QuotationRequestId,
                        principalTable: "QuotationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestComments_CommentType",
                table: "QuotationRequestComments",
                column: "CommentType");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestComments_QuotationRequestId",
                table: "QuotationRequestComments",
                column: "QuotationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestFiles_QuotationRequestId",
                table: "QuotationRequestFiles",
                column: "QuotationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_AssignedToTeamMember",
                table: "QuotationRequests",
                column: "AssignedToTeamMember");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_CreatedAt",
                table: "QuotationRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_CustomerEmail",
                table: "QuotationRequests",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_CustomerId",
                table: "QuotationRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_Priority",
                table: "QuotationRequests",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_Status",
                table: "QuotationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestStatusHistories_CreatedAt",
                table: "QuotationRequestStatusHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestStatusHistories_QuotationRequestId",
                table: "QuotationRequestStatusHistories",
                column: "QuotationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestStatusHistories_ToStatus",
                table: "QuotationRequestStatusHistories",
                column: "ToStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationRequestComments");

            migrationBuilder.DropTable(
                name: "QuotationRequestFiles");

            migrationBuilder.DropTable(
                name: "QuotationRequestStatusHistories");

            migrationBuilder.DropTable(
                name: "QuotationRequests");
        }
    }
}
