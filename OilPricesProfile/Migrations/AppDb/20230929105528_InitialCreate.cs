using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OilPricesProfile.Migrations.AppDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OilDepots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilDepots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PetroleumProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetroleumProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OilDepotId = table.Column<int>(type: "int", nullable: false),
                    PetroleumProductId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MinPricePerLiterInclVat = table.Column<double>(type: "float", nullable: true),
                    MaxPricePerLiterInclVat = table.Column<double>(type: "float", nullable: true),
                    WeightedAveragePricePerLiterInclVat = table.Column<double>(type: "float", nullable: true),
                    WeightedAverageIndexPerLiterInclVat = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prices_OilDepots_OilDepotId",
                        column: x => x.OilDepotId,
                        principalTable: "OilDepots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prices_PetroleumProducts_PetroleumProductId",
                        column: x => x.PetroleumProductId,
                        principalTable: "PetroleumProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OilDepots_Name",
                table: "OilDepots",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PetroleumProducts_Name",
                table: "PetroleumProducts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_OilDepotId",
                table: "Prices",
                column: "OilDepotId");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_PetroleumProductId",
                table: "Prices",
                column: "PetroleumProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prices");

            migrationBuilder.DropTable(
                name: "OilDepots");

            migrationBuilder.DropTable(
                name: "PetroleumProducts");
        }
    }
}
