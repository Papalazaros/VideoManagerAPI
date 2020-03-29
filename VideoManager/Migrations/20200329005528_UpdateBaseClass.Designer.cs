﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VideoManager;

namespace VideoManager.Migrations
{
    [DbContext(typeof(VideoManagerDbContext))]
    [Migration("20200329005528_UpdateBaseClass")]
    partial class UpdateBaseClass
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VideoManager.Models.Playlist", b =>
                {
                    b.Property<Guid>("PlaylistId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CreatedByUserUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("ModifiedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ModifiedByUserUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("PlaylistId");

                    b.HasIndex("CreatedByUserUserId");

                    b.HasIndex("ModifiedByUserUserId");

                    b.HasIndex("RoomId")
                        .IsUnique();

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("VideoManager.Models.PlaylistVideo", b =>
                {
                    b.Property<Guid>("PlaylistId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("VideoId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("PlaylistId", "VideoId");

                    b.HasIndex("VideoId");

                    b.ToTable("PlaylistVideos");
                });

            modelBuilder.Entity("VideoManager.Models.Room", b =>
                {
                    b.Property<Guid>("RoomId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CreatedByUserUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("bit");

                    b.Property<Guid?>("ModifiedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ModifiedByUserUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PlaylistId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RoomId");

                    b.HasIndex("CreatedByUserUserId");

                    b.HasIndex("ModifiedByUserUserId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("VideoManager.Models.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Auth0Id")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("VideoManager.Models.Video", b =>
                {
                    b.Property<Guid>("VideoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CreatedByUserUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("DurationInSeconds")
                        .HasColumnType("int");

                    b.Property<TimeSpan?>("EncodeTime")
                        .HasColumnType("time");

                    b.Property<long?>("EncodedLength")
                        .HasColumnType("bigint");

                    b.Property<string>("EncodedType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ModifiedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ModifiedByUserUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("OriginalFileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("OriginalLength")
                        .HasColumnType("bigint");

                    b.Property<string>("OriginalType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("ThumbnailFilePath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("VideoId");

                    b.HasIndex("CreatedByUserUserId");

                    b.HasIndex("ModifiedByUserUserId");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("VideoManager.Models.Playlist", b =>
                {
                    b.HasOne("VideoManager.Models.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedByUserUserId");

                    b.HasOne("VideoManager.Models.User", "ModifiedByUser")
                        .WithMany()
                        .HasForeignKey("ModifiedByUserUserId");

                    b.HasOne("VideoManager.Models.Room", "Room")
                        .WithOne("Playlist")
                        .HasForeignKey("VideoManager.Models.Playlist", "RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VideoManager.Models.PlaylistVideo", b =>
                {
                    b.HasOne("VideoManager.Models.Playlist", "Playlist")
                        .WithMany("PlaylistVideos")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VideoManager.Models.Video", "Video")
                        .WithMany("PlaylistVideos")
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VideoManager.Models.Room", b =>
                {
                    b.HasOne("VideoManager.Models.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedByUserUserId");

                    b.HasOne("VideoManager.Models.User", "ModifiedByUser")
                        .WithMany()
                        .HasForeignKey("ModifiedByUserUserId");

                    b.HasOne("VideoManager.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("VideoManager.Models.Video", b =>
                {
                    b.HasOne("VideoManager.Models.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedByUserUserId");

                    b.HasOne("VideoManager.Models.User", "ModifiedByUser")
                        .WithMany()
                        .HasForeignKey("ModifiedByUserUserId");
                });
#pragma warning restore 612, 618
        }
    }
}
