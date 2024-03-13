import 'package:json_annotation/json_annotation.dart';
import 'package:player_mobile_app/models/track.dart';

part 'playlist.g.dart';

@JsonSerializable()
class Playlist {
  String id;
  List<Track> tracks = List<Track>();
  bool overloaded;
  DateTime date;

  Playlist({
    this.id,
    this.tracks,
    this.overloaded,
    this.date,
  });

  factory Playlist.fromJson(Map<String, dynamic> json) =>
      _$PlaylistFromJson(json);
  Map<String, dynamic> toJson() => _$PlaylistToJson(this);
}

@JsonSerializable()
class PlaylistEnvelope {
  Playlist playlist;

  PlaylistEnvelope({
    this.playlist,
  });

  factory PlaylistEnvelope.fromJson(Map<String, dynamic> json) =>
      _$PlaylistEnvelopeFromJson(json);
  Map<String, dynamic> toJson() => _$PlaylistEnvelopeToJson(this);
}
