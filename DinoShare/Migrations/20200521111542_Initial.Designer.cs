﻿// <auto-generated />
using System;
using DinoShare.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DinoShare.Migrations
{
    [DbContext(typeof(AppDBContext))]
    [Migration("20200521111542_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DinoShare.Models.AccountDataModelFactory.LinkUserRole", b =>
                {
                    b.Property<Guid>("LinkUserRoleID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CreatedUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("EditUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserRoleID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LinkUserRoleID");

                    b.HasIndex("UserID");

                    b.HasIndex("UserRoleID");

                    b.ToTable("LinkUserRole");
                });

            modelBuilder.Entity("DinoShare.Models.AccountDataModelFactory.TemporaryTokensType", b =>
                {
                    b.Property<Guid>("TemporaryTokensTypeID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TemporaryTokensTypeID");

                    b.ToTable("TemporaryTokensType");
                });

            modelBuilder.Entity("DinoShare.Models.AccountDataModelFactory.User", b =>
                {
                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CreatedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("CreatedUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EditDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("EditUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EmailAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSuspended")
                        .HasColumnType("bit");

                    b.Property<int>("LoginTries")
                        .HasColumnType("int");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Surname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Timezone")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DinoShare.Models.AccountDataModelFactory.UserRole", b =>
                {
                    b.Property<Guid>("UserRoleID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserRoleID");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("DinoShare.Models.AccountDataModelFactory.UserTemporaryToken", b =>
                {
                    b.Property<Guid>("UserTemporaryTokenID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CreatedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("EditDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("EditUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TemporaryTokensTypeID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("TokenExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserTemporaryTokenID");

                    b.HasIndex("TemporaryTokensTypeID");

                    b.HasIndex("UserID");

                    b.ToTable("UserTemporaryToken");
                });

            modelBuilder.Entity("DinoShare.Models.SystemModelFactory.ApplicationLog", b =>
                {
                    b.Property<Guid>("ApplicationLogID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Exception")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Level")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LogDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LogOriginator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ApplicationLogID");

                    b.HasIndex("UserID");

                    b.ToTable("ApplicationLog");
                });

            modelBuilder.Entity("DinoShare.Models.SystemModelFactory.EmailTemplate", b =>
                {
                    b.Property<Guid>("EmailTemplateID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CreatedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EditDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("EditUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EventCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TemplateBody")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TemplateBodyTags")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EmailTemplateID");

                    b.ToTable("EmailTemplates");
                });

            modelBuilder.Entity("DinoShare.Models.SystemModelFactory.SystemConfiguration", b =>
                {
                    b.Property<Guid>("SystemConfigurationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConfigValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EditDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("EditUserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EventCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SystemConfigurationID");

                    b.ToTable("SystemConfiguration");
                });

            modelBuilder.Entity("DinoShare.Models.AccountDataModelFactory.LinkUserRole", b =>
                {
                    b.HasOne("DinoShare.Models.AccountDataModelFactory.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DinoShare.Models.AccountDataModelFactory.UserRole", "UserRole")
                        .WithMany()
                        .HasForeignKey("UserRoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DinoShare.Models.AccountDataModelFactory.UserTemporaryToken", b =>
                {
                    b.HasOne("DinoShare.Models.AccountDataModelFactory.TemporaryTokensType", "TemporaryTokensType")
                        .WithMany()
                        .HasForeignKey("TemporaryTokensTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DinoShare.Models.AccountDataModelFactory.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DinoShare.Models.SystemModelFactory.ApplicationLog", b =>
                {
                    b.HasOne("DinoShare.Models.AccountDataModelFactory.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");
                });
#pragma warning restore 612, 618
        }
    }
}