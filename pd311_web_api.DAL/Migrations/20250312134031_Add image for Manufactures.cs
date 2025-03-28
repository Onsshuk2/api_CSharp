﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pd311_web_api.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddimageforManufactures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Manufactures",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Manufactures");
        }
    }
}
