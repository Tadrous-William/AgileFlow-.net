using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgileTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskStartDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingActionItems_AspNetUsers_CreatedByUserId",
                table: "MeetingActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingNote_AspNetUsers_AuthorId",
                table: "MeetingNote");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingNote_Meetings_MeetingId",
                table: "MeetingNote");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingNote_Tenants_TenantId",
                table: "MeetingNote");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_CreatedByUserId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_FacilitatorId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CreatedByUserId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_FacilitatorId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_MeetingActionItems_CreatedByUserId",
                table: "MeetingActionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingNote",
                table: "MeetingNote");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "FacilitatorId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeetingActionItems");

            migrationBuilder.RenameTable(
                name: "MeetingNote",
                newName: "MeetingNotes");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingNote_TenantId",
                table: "MeetingNotes",
                newName: "IX_MeetingNotes_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingNote_MeetingId",
                table: "MeetingNotes",
                newName: "IX_MeetingNotes_MeetingId");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingNote_AuthorId",
                table: "MeetingNotes",
                newName: "IX_MeetingNotes_AuthorId");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "MeetingNotes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "MeetingNotes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingNotes",
                table: "MeetingNotes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CreatedBy",
                table: "Meetings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_FacilitatedBy",
                table: "Meetings",
                column: "FacilitatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActionItems_CreatedBy",
                table: "MeetingActionItems",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingActionItems_AspNetUsers_CreatedBy",
                table: "MeetingActionItems",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingNotes_AspNetUsers_AuthorId",
                table: "MeetingNotes",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingNotes_Meetings_MeetingId",
                table: "MeetingNotes",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingNotes_Tenants_TenantId",
                table: "MeetingNotes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_CreatedBy",
                table: "Meetings",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_FacilitatedBy",
                table: "Meetings",
                column: "FacilitatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingActionItems_AspNetUsers_CreatedBy",
                table: "MeetingActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingNotes_AspNetUsers_AuthorId",
                table: "MeetingNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingNotes_Meetings_MeetingId",
                table: "MeetingNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingNotes_Tenants_TenantId",
                table: "MeetingNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_CreatedBy",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_FacilitatedBy",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CreatedBy",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_FacilitatedBy",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_MeetingActionItems_CreatedBy",
                table: "MeetingActionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingNotes",
                table: "MeetingNotes");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Tasks");

            migrationBuilder.RenameTable(
                name: "MeetingNotes",
                newName: "MeetingNote");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingNotes_TenantId",
                table: "MeetingNote",
                newName: "IX_MeetingNote_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingNotes_MeetingId",
                table: "MeetingNote",
                newName: "IX_MeetingNote_MeetingId");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingNotes_AuthorId",
                table: "MeetingNote",
                newName: "IX_MeetingNote_AuthorId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacilitatorId",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "MeetingActionItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "MeetingNote",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "MeetingNote",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingNote",
                table: "MeetingNote",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CreatedByUserId",
                table: "Meetings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_FacilitatorId",
                table: "Meetings",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActionItems_CreatedByUserId",
                table: "MeetingActionItems",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingActionItems_AspNetUsers_CreatedByUserId",
                table: "MeetingActionItems",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingNote_AspNetUsers_AuthorId",
                table: "MeetingNote",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingNote_Meetings_MeetingId",
                table: "MeetingNote",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingNote_Tenants_TenantId",
                table: "MeetingNote",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_CreatedByUserId",
                table: "Meetings",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_FacilitatorId",
                table: "Meetings",
                column: "FacilitatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
