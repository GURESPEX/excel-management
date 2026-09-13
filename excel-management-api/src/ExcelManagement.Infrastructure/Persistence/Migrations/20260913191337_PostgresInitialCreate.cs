using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExcelManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PostgresInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration only retypes columns to Postgres-native types
            // (character varying/boolean/date/timestamptz/identity) — Sqlite
            // already has the right shape from InitialCreate/AddUsers, and its
            // migrations history is shared with Postgres's, so it must no-op
            // when Sqlite is the active provider (both providers share one
            // Migrations folder/history table by design in this project).
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                return;
            }

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Postgres has no implicit TEXT->timestamptz/date cast, and no int->boolean
            // cast at all (unlike SQLite, which stores both as loosely-typed columns) —
            // the scaffolded AlterColumn calls for these four columns fail with
            // "column ... cannot be cast automatically to type ...", so they're
            // replaced with raw SQL carrying an explicit USING clause.
            migrationBuilder.Sql(
                "ALTER TABLE \"Employees\" ALTER COLUMN \"UpdatedAt\" TYPE timestamp with time zone USING \"UpdatedAt\"::timestamp with time zone;");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.Sql(
                "ALTER TABLE \"Employees\" ALTER COLUMN \"JoinDate\" TYPE date USING \"JoinDate\"::date;");

            migrationBuilder.Sql(
                "ALTER TABLE \"Employees\" ALTER COLUMN \"IsActive\" TYPE boolean USING (\"IsActive\" <> 0);");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Employees",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Employees",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Departments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.Sql(
                "ALTER TABLE \"Departments\" ALTER COLUMN \"IsActive\" TYPE boolean USING (\"IsActive\" <> 0);");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Departments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                return;
            }

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql(
                "ALTER TABLE \"Employees\" ALTER COLUMN \"UpdatedAt\" TYPE TEXT USING \"UpdatedAt\"::text;");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Employees",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.Sql(
                "ALTER TABLE \"Employees\" ALTER COLUMN \"JoinDate\" TYPE TEXT USING \"JoinDate\"::text;");

            migrationBuilder.Sql(
                "ALTER TABLE \"Employees\" ALTER COLUMN \"IsActive\" TYPE integer USING (CASE WHEN \"IsActive\" THEN 1 ELSE 0 END);");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Employees",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Employees",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Departments",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.Sql(
                "ALTER TABLE \"Departments\" ALTER COLUMN \"IsActive\" TYPE integer USING (CASE WHEN \"IsActive\" THEN 1 ELSE 0 END);");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Departments",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
