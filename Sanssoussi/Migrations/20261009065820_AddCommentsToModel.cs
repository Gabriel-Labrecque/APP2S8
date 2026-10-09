using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanssoussi.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsToModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The table already exists (created by CreateCommentsSchema, without a foreign key).
            // SQLite cannot add a foreign key to an existing table, so the table is rebuilt.
            migrationBuilder.Sql("ALTER TABLE \"Comments\" RENAME TO \"Comments_old\";");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            // Comments whose owner no longer exists cannot satisfy the foreign key and are not kept
            migrationBuilder.Sql(
                "INSERT INTO \"Comments\" (\"CommentId\", \"UserId\", \"Comment\") " +
                "SELECT \"CommentId\", \"UserId\", \"Comment\" FROM \"Comments_old\" " +
                "WHERE \"UserId\" IN (SELECT \"Id\" FROM \"AspNetUsers\");");
            migrationBuilder.Sql("DROP TABLE \"Comments_old\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");
        }
    }
}
