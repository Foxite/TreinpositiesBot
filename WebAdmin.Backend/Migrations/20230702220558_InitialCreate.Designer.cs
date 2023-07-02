﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebAdmin.Backend.Entities;

#nullable disable

namespace WebAdmin.Backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230702220558_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebAdmin.Backend.Entities.Channel", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<float?>("CooldownSeconds")
                        .HasColumnType("real");

                    b.Property<string[]>("SourceNames")
                        .HasColumnType("text[]");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("WebAdmin.Backend.Entities.Guild", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<float?>("CooldownSeconds")
                        .HasColumnType("real");

                    b.Property<string[]>("SourceNames")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });
#pragma warning restore 612, 618
        }
    }
}
