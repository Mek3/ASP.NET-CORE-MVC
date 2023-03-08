using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT Id FROM AspNetRoles WHERE Id ='ca4411b8-8747-4ab6-bf85-e904e74a82aa')
                                    BEGIN
	                                    INSERT AspNetRoles (Id, [Name], [NormalizedName]) 
	                                    VALUES ('ca4411b8-8747-4ab6-bf85-e904e74a82aa', 'admin', 'ADMIN')
                                    END");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE AspNetRoles WHERE Id ='ca4411b8-8747-4ab6-bf85-e904e74a82aa'");
        }
    }
}
