using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportSystem.Migrations
{
    /// <inheritdoc />
    public partial class yuu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_AspNetUsers_UserId",
                table: "attendance");

            migrationBuilder.DropTable(
                name: "ApplicationUserProject");

            migrationBuilder.DropIndex(
                name: "IX_attendance_UserId",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "TommorowComment",
                table: "report");

            migrationBuilder.DropColumn(
                name: "Confirm",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "attendance");

            migrationBuilder.AlterColumn<string>(
                name: "TaskName",
                table: "todo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "todo",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "report",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "TomorrowComment",
                table: "report",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpDate",
                table: "report",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "project",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HealthComment",
                table: "attendance",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "attendance",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "attendance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "userproject",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userproject", x => new { x.UserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_userproject_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userproject_project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "project",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_Name",
                table: "project",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_ApplicationUserId",
                table: "attendance",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_ReportId",
                table: "attendance",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_userproject_ProjectId",
                table: "userproject",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_AspNetUsers_ApplicationUserId",
                table: "attendance",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_report_ReportId",
                table: "attendance",
                column: "ReportId",
                principalTable: "report",
                principalColumn: "ReportId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_AspNetUsers_ApplicationUserId",
                table: "attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_attendance_report_ReportId",
                table: "attendance");

            migrationBuilder.DropTable(
                name: "userproject");

            migrationBuilder.DropIndex(
                name: "IX_project_Name",
                table: "project");

            migrationBuilder.DropIndex(
                name: "IX_attendance_ApplicationUserId",
                table: "attendance");

            migrationBuilder.DropIndex(
                name: "IX_attendance_ReportId",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "TomorrowComment",
                table: "report");

            migrationBuilder.DropColumn(
                name: "UpDate",
                table: "report");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "TaskName",
                table: "todo",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "todo",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "report",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TommorowComment",
                table: "report",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "project",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<bool>(
                name: "Confirm",
                table: "feedback",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "HealthComment",
                table: "attendance",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "attendance",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ApplicationUserProject",
                columns: table => new
                {
                    ProjectsProjectId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserProject", x => new { x.ProjectsProjectId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserProject_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserProject_project_ProjectsProjectId",
                        column: x => x.ProjectsProjectId,
                        principalTable: "project",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_UserId",
                table: "attendance",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProject_UsersId",
                table: "ApplicationUserProject",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_AspNetUsers_UserId",
                table: "attendance",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
