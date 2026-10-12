using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Denarius.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPortugueseCollation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:pt_br_icu", "pt-BR,pt-BR,icu,True")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:CollationDefinition:pt_br_icu", "pt-BR,pt-BR,icu,True")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
