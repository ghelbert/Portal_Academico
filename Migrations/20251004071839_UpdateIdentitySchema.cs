using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal_Academico.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "474c2488-04cc-49d5-a1a4-2153f7afd1d7");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "474c2488-04cc-49d5-a1a4-2153f7afd1d7", 0, "4ac3875b-f635-4da0-8f3b-c0f50a98ad4d", "gustavo@hotmail.com", false, false, null, null, null, "AQAAAAIAAYagAAAAEI1iLNqYRKIucp2iFANzzKZB1hZiJnP+QFOYZKf7B4JXoYHw7D0/rt7gugrKvG1foQ==", null, false, "35fe78b2-4c3a-490a-9462-c5998c8cfe42", false, "Gustavo Reinoso" });
        }
    }
}
