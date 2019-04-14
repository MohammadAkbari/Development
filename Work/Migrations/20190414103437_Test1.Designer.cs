﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Work;

namespace Work.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20190414103437_Test1")]
    partial class Test1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Work.Campaign", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date");

                    b.Property<TimeSpan>("From");

                    b.Property<TimeSpan>("Offset");

                    b.Property<TimeSpan>("To");

                    b.HasKey("Id");

                    b.ToTable("Campaigns");
                });

            modelBuilder.Entity("Work.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("DayOfWeek");

                    b.Property<Guid>("Guid");

                    b.Property<decimal>("Price");

                    b.HasKey("Id");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Work.Post", b =>
                {
                    b.Property<int>("PostId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AuthorUserId");

                    b.Property<string>("Content");

                    b.Property<string>("ContributorUserId");

                    b.Property<string>("Title");

                    b.HasKey("PostId");

                    b.HasIndex("AuthorUserId");

                    b.HasIndex("ContributorUserId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("Work.User", b =>
                {
                    b.Property<string>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Work.ValueRelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description");

                    b.HasKey("Id");

                    b.ToTable("ValueRelation");
                });

            modelBuilder.Entity("Work.Event", b =>
                {
                    b.OwnsOne("Work.Value", "Value1", b1 =>
                        {
                            b1.Property<int>("EventId")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<string>("Name");

                            b1.Property<int?>("ValueRelationId");

                            b1.HasKey("EventId");

                            b1.HasIndex("ValueRelationId");

                            b1.ToTable("Events");

                            b1.HasOne("Work.Event")
                                .WithOne("Value1")
                                .HasForeignKey("Work.Value", "EventId")
                                .OnDelete(DeleteBehavior.Cascade);

                            b1.HasOne("Work.ValueRelation", "ValueRelation")
                                .WithMany()
                                .HasForeignKey("ValueRelationId");
                        });

                    b.OwnsOne("Work.Value", "Value2", b1 =>
                        {
                            b1.Property<int>("EventId")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<string>("Name");

                            b1.Property<int?>("ValueRelationId");

                            b1.HasKey("EventId");

                            b1.HasIndex("ValueRelationId");

                            b1.ToTable("Events");

                            b1.HasOne("Work.Event")
                                .WithOne("Value2")
                                .HasForeignKey("Work.Value", "EventId")
                                .OnDelete(DeleteBehavior.Cascade);

                            b1.HasOne("Work.ValueRelation", "ValueRelation")
                                .WithMany()
                                .HasForeignKey("ValueRelationId");
                        });
                });

            modelBuilder.Entity("Work.Post", b =>
                {
                    b.HasOne("Work.User", "Author")
                        .WithMany("AuthoredPosts")
                        .HasForeignKey("AuthorUserId");

                    b.HasOne("Work.User", "Contributor")
                        .WithMany("ContributedToPosts")
                        .HasForeignKey("ContributorUserId");
                });
#pragma warning restore 612, 618
        }
    }
}
