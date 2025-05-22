using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SimpleApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PremierLeagueTeams");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FavoriteTeam",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FavoriteTeam",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "PremierLeagueTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Founded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stadium = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremierLeagueTeams", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PremierLeagueTeams",
                columns: new[] { "Id", "Founded", "LogoUrl", "Name", "Rating", "ShortName", "Stadium" },
                values: new object[,]
                {
                    { 1, new DateTime(1880, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://resources.premierleague.com/premierleague/badges/t43.png", "Manchester City", 88, "MCI", "Etihad Stadium" },
                    { 2, new DateTime(1886, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://resources.premierleague.com/premierleague/badges/t3.png", "Arsenal", 85, "ARS", "Emirates Stadium" },
                    { 3, new DateTime(1892, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://resources.premierleague.com/premierleague/badges/t14.png", "Liverpool", 86, "LIV", "Anfield" },
                    { 4, new DateTime(1878, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://resources.premierleague.com/premierleague/badges/t1.png", "Manchester United", 83, "MUN", "Old Trafford" }
                });
        }
    }
}
