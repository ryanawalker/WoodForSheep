using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WoodForSheep.Data.Migrations
{
    public partial class testmigration4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GameInitID = table.Column<int>(nullable: false),
                    GameReceiveID = table.Column<int>(nullable: false),
                    UserInitID = table.Column<string>(nullable: true),
                    UserInitStatus = table.Column<string>(nullable: true),
                    UserReceiveID = table.Column<string>(nullable: true),
                    UserReceiveStatus = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
