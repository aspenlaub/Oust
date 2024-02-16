﻿// <auto-generated />
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20240216084047_MicrosoftEntityFrameworkCoreTools802")]
    partial class MicrosoftEntityFrameworkCoreTools802
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities.Script", b =>
                {
                    b.Property<string>("Guid")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Guid");

                    b.ToTable("Scripts");
                });

            modelBuilder.Entity("Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities.ScriptStep", b =>
                {
                    b.Property<string>("Guid")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ControlGuid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ControlName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExpectedContents")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FormGuid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FormInstanceNumber")
                        .HasColumnType("int");

                    b.Property<string>("FormName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IdOrClass")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IdOrClassInstanceNumber")
                        .HasColumnType("int");

                    b.Property<string>("InputText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScriptGuid")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ScriptStepType")
                        .HasColumnType("int");

                    b.Property<int>("StepNumber")
                        .HasColumnType("int");

                    b.Property<string>("SubScriptGuid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SubScriptName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Guid");

                    b.HasIndex("ScriptGuid");

                    b.ToTable("ScriptSteps");
                });

            modelBuilder.Entity("Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities.ScriptStep", b =>
                {
                    b.HasOne("Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities.Script", null)
                        .WithMany("ScriptSteps")
                        .HasForeignKey("ScriptGuid");
                });

            modelBuilder.Entity("Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities.Script", b =>
                {
                    b.Navigation("ScriptSteps");
                });
#pragma warning restore 612, 618
        }
    }
}