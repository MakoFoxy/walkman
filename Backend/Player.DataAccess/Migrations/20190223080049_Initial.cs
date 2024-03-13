using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    SecondName = table.Column<string>(nullable: true),
                    RoleId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleRules",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(nullable: false),
                    RuleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRules", x => new { x.RoleId, x.RuleId });
                    table.ForeignKey(
                        name: "FK_RoleRules_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleRules_Rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adverts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FilePath = table.Column<string>(nullable: true),
                    UploaderId = table.Column<Guid>(nullable: true),
                    Length = table.Column<TimeSpan>(nullable: false),
                    TrackTypeId = table.Column<Guid>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adverts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adverts_TrackTypes_TrackTypeId",
                        column: x => x.TrackTypeId,
                        principalTable: "TrackTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Adverts_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MusicTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FilePath = table.Column<string>(nullable: true),
                    UploaderId = table.Column<Guid>(nullable: true),
                    Length = table.Column<TimeSpan>(nullable: false),
                    TrackTypeId = table.Column<Guid>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false),
                    IsHit = table.Column<bool>(nullable: false),
                    Index = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicTracks_TrackTypes_TrackTypeId",
                        column: x => x.TrackTypeId,
                        principalTable: "TrackTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MusicTracks_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Objects",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    BeginTime = table.Column<TimeSpan>(nullable: false),
                    EndTime = table.Column<TimeSpan>(nullable: false),
                    Attendance = table.Column<int>(nullable: false),
                    ActualAddress = table.Column<string>(nullable: true),
                    LegalAddress = table.Column<string>(nullable: true),
                    ActivityTypeId = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Geolocation = table.Column<string>(nullable: true),
                    ServiceCompanyId = table.Column<Guid>(nullable: true),
                    Area = table.Column<double>(nullable: false),
                    RentersCount = table.Column<int>(nullable: false),
                    Bin = table.Column<string>(nullable: true),
                    SilentPercent = table.Column<double>(nullable: false),
                    SilentBlockInterval = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Objects_ActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Objects_ServiceCompanies_ServiceCompanyId",
                        column: x => x.ServiceCompanyId,
                        principalTable: "ServiceCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Objects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdLifetimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdvertId = table.Column<Guid>(nullable: true),
                    DateBegin = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdLifetimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdLifetimes_Adverts_AdvertId",
                        column: x => x.AdvertId,
                        principalTable: "Adverts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdvertId = table.Column<Guid>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    ObjectId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdHistories_Adverts_AdvertId",
                        column: x => x.AdvertId,
                        principalTable: "Adverts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdHistories_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdTimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdvertId = table.Column<Guid>(nullable: true),
                    PlayDate = table.Column<DateTime>(nullable: false),
                    RepeatCount = table.Column<int>(nullable: false),
                    ObjectId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdTimes_Adverts_AdvertId",
                        column: x => x.AdvertId,
                        principalTable: "Adverts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdTimes_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    IsMain = table.Column<bool>(nullable: false),
                    ObjectId = table.Column<Guid>(nullable: true),
                    Size = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Intervals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TrackTypeId = table.Column<Guid>(nullable: true),
                    BeginTime = table.Column<TimeSpan>(nullable: false),
                    EndTime = table.Column<TimeSpan>(nullable: false),
                    ObjectInfoId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Intervals_Objects_ObjectInfoId",
                        column: x => x.ObjectInfoId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Intervals_TrackTypes_TrackTypeId",
                        column: x => x.TrackTypeId,
                        principalTable: "TrackTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MusicHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MusicTrackId = table.Column<Guid>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    ObjectId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicHistories_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MusicHistories_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ObjectId = table.Column<Guid>(nullable: true),
                    PlayingDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvertPlaylists",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdvertId = table.Column<Guid>(nullable: true),
                    PlaylistId = table.Column<Guid>(nullable: true),
                    PlayingDateTime = table.Column<DateTime>(nullable: false),
                    BlockIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertPlaylists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertPlaylists_Adverts_AdvertId",
                        column: x => x.AdvertId,
                        principalTable: "Adverts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvertPlaylists_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MusicTrackPlaylists",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MusicTrackId = table.Column<Guid>(nullable: true),
                    PlaylistId = table.Column<Guid>(nullable: true),
                    PlayingDateTime = table.Column<DateTime>(nullable: false),
                    BlockIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicTrackPlaylists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicTrackPlaylists_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MusicTrackPlaylists_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdHistories_AdvertId",
                table: "AdHistories",
                column: "AdvertId");

            migrationBuilder.CreateIndex(
                name: "IX_AdHistories_ObjectId",
                table: "AdHistories",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AdLifetimes_AdvertId",
                table: "AdLifetimes",
                column: "AdvertId");

            migrationBuilder.CreateIndex(
                name: "IX_AdTimes_AdvertId",
                table: "AdTimes",
                column: "AdvertId");

            migrationBuilder.CreateIndex(
                name: "IX_AdTimes_ObjectId",
                table: "AdTimes",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertPlaylists_AdvertId",
                table: "AdvertPlaylists",
                column: "AdvertId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertPlaylists_PlaylistId",
                table: "AdvertPlaylists",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_Adverts_TrackTypeId",
                table: "Adverts",
                column: "TrackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Adverts_UploaderId",
                table: "Adverts",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ObjectId",
                table: "Images",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Intervals_ObjectInfoId",
                table: "Intervals",
                column: "ObjectInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Intervals_TrackTypeId",
                table: "Intervals",
                column: "TrackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicHistories_MusicTrackId",
                table: "MusicHistories",
                column: "MusicTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicHistories_ObjectId",
                table: "MusicHistories",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrackPlaylists_MusicTrackId",
                table: "MusicTrackPlaylists",
                column: "MusicTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrackPlaylists_PlaylistId",
                table: "MusicTrackPlaylists",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_TrackTypeId",
                table: "MusicTracks",
                column: "TrackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_UploaderId",
                table: "MusicTracks",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_ActivityTypeId",
                table: "Objects",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_ServiceCompanyId",
                table: "Objects",
                column: "ServiceCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_UserId",
                table: "Objects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ObjectId",
                table: "Playlists",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleRules_RuleId",
                table: "RoleRules",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdHistories");

            migrationBuilder.DropTable(
                name: "AdLifetimes");

            migrationBuilder.DropTable(
                name: "AdTimes");

            migrationBuilder.DropTable(
                name: "AdvertPlaylists");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Intervals");

            migrationBuilder.DropTable(
                name: "MusicHistories");

            migrationBuilder.DropTable(
                name: "MusicTrackPlaylists");

            migrationBuilder.DropTable(
                name: "RoleRules");

            migrationBuilder.DropTable(
                name: "Adverts");

            migrationBuilder.DropTable(
                name: "MusicTracks");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "TrackTypes");

            migrationBuilder.DropTable(
                name: "Objects");

            migrationBuilder.DropTable(
                name: "ActivityTypes");

            migrationBuilder.DropTable(
                name: "ServiceCompanies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
