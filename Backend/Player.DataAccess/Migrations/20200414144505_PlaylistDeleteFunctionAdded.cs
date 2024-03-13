using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class PlaylistDeleteFunctionAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
create or replace function delete_playlist(id uuid) returns void
    language sql
as
$$
delete from ""MusicTrackPlaylists"" where ""PlaylistId"" = id;
    delete from ""AdvertPlaylists"" where ""PlaylistId"" = id;
    delete from ""Blocks"" where ""PlaylistId"" = id;    
    delete from ""Playlists"" where ""Id"" = id;
$$;

alter function delete_playlist(uuid) owner to walkman_admin;
                                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop function if exists delete_playlist(uuid)");
        }
    }
}
