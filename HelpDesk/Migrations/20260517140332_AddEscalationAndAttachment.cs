using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddEscalationAndAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Escalations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FromLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ToLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escalations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Escalations_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Escalations_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Escalations_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567801"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4929));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567802"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4934));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567803"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4936));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567804"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4939));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567805"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4941));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567801"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4518));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567802"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4522));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567803"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4523));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567804"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 14, 3, 31, 872, DateTimeKind.Utc).AddTicks(4550));

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TicketId",
                table: "Attachments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UploadedByUserId",
                table: "Attachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_FromUserId",
                table: "Escalations",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_TicketId",
                table: "Escalations",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_ToUserId",
                table: "Escalations",
                column: "ToUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Escalations");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567801"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4443));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567802"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4450));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567803"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4454));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567804"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4456));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567805"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4462));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567801"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4050));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567802"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4056));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567803"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4058));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567804"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 16, 17, 26, 39, 680, DateTimeKind.Utc).AddTicks(4060));
        }
    }
}
