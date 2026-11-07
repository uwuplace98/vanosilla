using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class CoravelPro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "characters",
                table: "time_space_records",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "website",
                table: "shopitems",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "website",
                table: "shopitems",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "website",
                table: "shopitems",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Expires",
                schema: "accounts",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "bazaar",
                table: "items",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                schema: "bazaar",
                table: "items",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "bazaar",
                table: "items",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "bazaar",
                table: "items",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_warehouses_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_warehouses_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_warehouses_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_warehouses",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_warehouses",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_warehouses",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_memberships",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastOnlineDate",
                schema: "families",
                table: "families_memberships",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                schema: "families",
                table: "families_memberships",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_memberships",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_memberships",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                schema: "families",
                table: "families_logs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "characters",
                table: "characters",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "characters",
                table: "characters",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "characters",
                table: "characters",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "website",
                table: "category",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "website",
                table: "category",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "website",
                table: "category",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "accounts",
                table: "accounts_warehouse",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "accounts",
                table: "accounts_warehouse",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "accounts",
                table: "accounts_warehouse",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                schema: "accounts",
                table: "accounts_penalties",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                schema: "accounts",
                table: "accounts_bans",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                schema: "accounts",
                table: "accounts_bans",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "accounts",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "accounts",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "accounts",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.CreateTable(
                name: "Coravel_JobHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TypeFullPath = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Failed = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coravel_JobHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coravel_ScheduledJobHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TypeFullPath = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Failed = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coravel_ScheduledJobHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coravel_ScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvocableFullPath = table.Column<string>(type: "text", nullable: true),
                    CronExpression = table.Column<string>(type: "text", nullable: true),
                    Frequency = table.Column<string>(type: "text", nullable: true),
                    Days = table.Column<string>(type: "text", nullable: true),
                    PreventOverlapping = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    TimeZoneInfo = table.Column<string>(type: "text", nullable: true),
                    RunOnDedicatedThread = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coravel_ScheduledJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coravel_JobHistory_EndedAt",
                table: "Coravel_JobHistory",
                column: "EndedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Coravel_ScheduledJobHistory_EndedAt",
                table: "Coravel_ScheduledJobHistory",
                column: "EndedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Coravel_ScheduledJobs_Active",
                table: "Coravel_ScheduledJobs",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Coravel_ScheduledJobs_InvocableFullPath",
                table: "Coravel_ScheduledJobs",
                column: "InvocableFullPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coravel_JobHistory");

            migrationBuilder.DropTable(
                name: "Coravel_ScheduledJobHistory");

            migrationBuilder.DropTable(
                name: "Coravel_ScheduledJobs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "characters",
                table: "time_space_records",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "website",
                table: "shopitems",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "website",
                table: "shopitems",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "website",
                table: "shopitems",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Expires",
                schema: "accounts",
                table: "refresh_tokens",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "bazaar",
                table: "items",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                schema: "bazaar",
                table: "items",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "bazaar",
                table: "items",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "bazaar",
                table: "items",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_warehouses_logs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_warehouses_logs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_warehouses_logs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_warehouses",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_warehouses",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_warehouses",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_memberships",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastOnlineDate",
                schema: "families",
                table: "families_memberships",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                schema: "families",
                table: "families_memberships",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_memberships",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_memberships",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families_logs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                schema: "families",
                table: "families_logs",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families_logs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families_logs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "families",
                table: "families",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "families",
                table: "families",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "families",
                table: "families",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "mails",
                table: "characters_notes",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "mails",
                table: "characters_mails",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "characters",
                table: "characters",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "characters",
                table: "characters",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "characters",
                table: "characters",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "website",
                table: "category",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "website",
                table: "category",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "website",
                table: "category",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "accounts",
                table: "accounts_warehouse",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "accounts",
                table: "accounts_warehouse",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "accounts",
                table: "accounts_warehouse",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                schema: "accounts",
                table: "accounts_penalties",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                schema: "accounts",
                table: "accounts_bans",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                schema: "accounts",
                table: "accounts_bans",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "accounts",
                table: "accounts",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 51);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "accounts",
                table: "accounts",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 52);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "accounts",
                table: "accounts",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 50);
        }
    }
}
