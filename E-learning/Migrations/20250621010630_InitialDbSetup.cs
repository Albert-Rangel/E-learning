using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_learning.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- INICIO DE CORRECCIONES ---

            // Comentar la creación de la tabla 'User' porque ya existe.
            // Los campos de perfil se añadirán con AddColumn más abajo.
            /*
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NationalIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });
            */

            // Comentar la creación de la tabla 'Course' porque ya existe.
            /*
            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_Course_User_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });
            */

            // Comentar la creación de la tabla 'Grade' porque ya existe.
            // Los campos de notas LapsoTotal, FinalGrade se añadirán con AddColumn más abajo.
            // Los tipos decimales de Lapso1, 2, 3 se ajustarán con AlterColumn.
            /*
            migrationBuilder.CreateTable(
                name: "Grade",
                columns: table => new
                {
                    GradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Lapso1 = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    Lapso2 = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    Lapso3 = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    FinalGrade = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    LapsoTotal = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true) // Esta es la línea original
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.GradeId);
                    table.ForeignKey(
                        name: "FK_Grade_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grade_User_StudentId",
                        column: x => x.StudentId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });
            */

            // Comentar la creación de la tabla 'StudentCourse' porque ya existe.
            /*
            migrationBuilder.CreateTable(
                name: "StudentCourse",
                columns: table => new
                {
                    StudentCourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCourse", x => x.StudentCourseId);
                    table.ForeignKey(
                        name: "FK_StudentCourse_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCourse_User_StudentId",
                        column: x => x.StudentId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });
            */

            // Añadir las nuevas columnas a la tabla 'User'
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "User",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "User",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdNumber",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            // Ajustar el tipo de columna 'Password' en la tabla 'User'
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "User",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            // Añadir las nuevas columnas y ajustar tipos en la tabla 'Grade'
            migrationBuilder.AddColumn<decimal>(
                name: "FinalGrade",
                table: "Grade",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LapsoTotal",
                table: "Grade",
                type: "decimal(4,2)",
                nullable: true);

            // COMENTADO: Esta línea causó el error "Column name 'LastUpdated' in table 'Grade' is specified more than once."
            // migrationBuilder.AddColumn<DateTime>(
            //    name: "LastUpdated",
            //    table: "Grade",
            //    type: "datetime2",
            //    nullable: true);

            // Ajustar el tipo de columna para Lapso1, Lapso2, Lapso3 en la tabla 'Grade'
            migrationBuilder.AlterColumn<decimal>(
                name: "Lapso1",
                table: "Grade",
                type: "decimal(4,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Lapso2",
                table: "Grade",
                type: "decimal(4,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Lapso3",
                table: "Grade",
                type: "decimal(4,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Course_TeacherId",
                table: "Course",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Grade_CourseId",
                table: "Grade",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Grade_StudentId",
                table: "Grade",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourse_CourseId",
                table: "StudentCourse",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourse_StudentId",
                table: "StudentCourse",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            // --- FIN DE CORRECCIONES EN UP() ---
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- INICIO DE CORRECCIONES EN DOWN() ---

            // Comentar los DropTable ya que las tablas no se crearon en Up()
            // Y no queremos que se eliminen de la DB si ya existían.
            /*
            migrationBuilder.DropTable(
                name: "Grade");

            migrationBuilder.DropTable(
                name: "StudentCourse");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "User");
            */

            // Para revertir los cambios de Up(), eliminamos las columnas que añadimos.
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NationalIdNumber",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "User");

            migrationBuilder.DropColumn(
                name: "FinalGrade",
                table: "Grade");

            migrationBuilder.DropColumn(
                name: "LapsoTotal",
                table: "Grade");

            // COMENTADO: Esta línea de DropColumn también debe ser eliminada/comentada, ya que no se añadió en Up().
            // migrationBuilder.DropColumn(
            //    name: "LastUpdated",
            //    table: "Grade");

            // Para revertir los cambios de AlterColumn, restauramos los tipos de datos antiguos.
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Lapso1",
                table: "Grade",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Lapso2",
                table: "Grade",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Lapso3",
                table: "Grade",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldNullable: true);

            // --- FIN DE CORRECCIONES EN DOWN() ---
        }
    }
}
