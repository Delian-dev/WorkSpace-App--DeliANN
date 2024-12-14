using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proiect_DAW_DeliANN.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedModeratorField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "moderator",
                table: "ApplicationUserWorkspaces",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "moderator",
                table: "ApplicationUserWorkspaces");
        }
    }
}
