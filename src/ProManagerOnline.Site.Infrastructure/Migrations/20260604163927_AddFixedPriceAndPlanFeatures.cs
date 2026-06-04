using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProManagerOnline.Site.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedPriceAndPlanFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FixedPriceAmount",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FixedPriceCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductPlanFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPlanFeatures_ProductPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "ProductPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPlanFeatures_PlanId",
                table: "ProductPlanFeatures",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPlanFeatures");

            migrationBuilder.DropColumn(
                name: "FixedPriceAmount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FixedPriceCurrency",
                table: "Products");
        }
    }
}
