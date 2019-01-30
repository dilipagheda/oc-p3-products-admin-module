using Microsoft.EntityFrameworkCore.Migrations;

namespace P3AddNewFunctionalityDotNetCore.Migrations
{
    public partial class MakeOrderLineForeignKeysCascaseDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineEntity_OrderEntity_OrderEntityId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK__OrderLine__Produ__52593CB8",
                table: "OrderLine");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineEntity_OrderEntity_OrderEntityId",
                table: "OrderLine",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__OrderLine__Produ__52593CB8",
                table: "OrderLine",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineEntity_OrderEntity_OrderEntityId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK__OrderLine__Produ__52593CB8",
                table: "OrderLine");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineEntity_OrderEntity_OrderEntityId",
                table: "OrderLine",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK__OrderLine__Produ__52593CB8",
                table: "OrderLine",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
