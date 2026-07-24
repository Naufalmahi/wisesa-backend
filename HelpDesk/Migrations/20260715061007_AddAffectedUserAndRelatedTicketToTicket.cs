using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddAffectedUserAndRelatedTicketToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketSlas_Users_UserId",
                table: "TicketSlas");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TicketNumber",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TicketSlas",
                newName: "TechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketSlas_UserId",
                table: "TicketSlas",
                newName: "IX_TicketSlas_TechnicianId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TicketSlas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "Tickets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "AffectedUser",
                table: "Tickets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedTicketId",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SlaPolicies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SlaPolicies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SlaPolicies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Attachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567801"),
                column: "CreatedAt",
                value: new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(5021));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567802"),
                column: "CreatedAt",
                value: new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(5027));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567803"),
                column: "CreatedAt",
                value: new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(5029));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567804"),
                column: "CreatedAt",
                value: new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(5032));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-7890-abcd-ef1234567805"),
                column: "CreatedAt",
                value: new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(5033));

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567801"),
                columns: new[] { "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4881), null, "", new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4884) });

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567802"),
                columns: new[] { "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4898), null, "", new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4898) });

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567803"),
                columns: new[] { "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4924), null, "", new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4924) });

            migrationBuilder.UpdateData(
                table: "SlaPolicies",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567804"),
                columns: new[] { "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4926), null, "", new DateTime(2026, 7, 15, 6, 10, 5, 472, DateTimeKind.Utc).AddTicks(4926) });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketNumber",
                table: "Tickets",
                column: "TicketNumber",
                unique: true,
                filter: "[TicketNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketSlas_Users_TechnicianId",
                table: "TicketSlas",
                column: "TechnicianId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketSlas_Users_TechnicianId",
                table: "TicketSlas");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TicketNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TicketSlas");

            migrationBuilder.DropColumn(
                name: "AffectedUser",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RelatedTicketId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "TechnicianId",
                table: "TicketSlas",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketSlas_TechnicianId",
                table: "TicketSlas",
                newName: "IX_TicketSlas_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "Tickets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

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
                name: "IX_Tickets_TicketNumber",
                table: "Tickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketSlas_Users_UserId",
                table: "TicketSlas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
