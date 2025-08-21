using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarWars.API.Migrations
{
    /// <inheritdoc />
    public partial class Init_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movie",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Poster = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MovieRating",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    MovieID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Released = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Runtime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Director = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metascore = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieRating", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MovieRating_Movie_MovieID",
                        column: x => x.MovieID,
                        principalTable: "Movie",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieRating_MovieID",
                table: "MovieRating",
                column: "MovieID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieRating");

            migrationBuilder.DropTable(
                name: "Movie");
        }
    }
}
