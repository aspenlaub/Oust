using Microsoft.EntityFrameworkCore.Migrations;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Migrations {
    public partial class EntityFrameworkCore604 : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "Scripts",
                columns: table => new {
                    Guid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Scripts", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "ScriptSteps",
                columns: table => new {
                    Guid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    ScriptStepType = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormGuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormInstanceNumber = table.Column<int>(type: "int", nullable: false),
                    ControlGuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ControlName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedContents = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubScriptGuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubScriptName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdOrClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdOrClassInstanceNumber = table.Column<int>(type: "int", nullable: false),
                    ScriptGuid = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ScriptSteps", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_ScriptSteps_Scripts_ScriptGuid",
                        column: x => x.ScriptGuid,
                        principalTable: "Scripts",
                        principalColumn: "Guid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScriptSteps_ScriptGuid",
                table: "ScriptSteps",
                column: "ScriptGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "ScriptSteps");

            migrationBuilder.DropTable(
                name: "Scripts");
        }
    }
}
