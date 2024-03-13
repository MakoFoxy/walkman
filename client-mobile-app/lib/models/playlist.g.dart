// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'playlist.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Playlist _$PlaylistFromJson(Map<String, dynamic> json) {
  return Playlist(
    id: json['id'] as String,
    tracks: (json['tracks'] as List)
        ?.map(
            (e) => e == null ? null : Track.fromJson(e as Map<String, dynamic>))
        ?.toList(),
    overloaded: json['overloaded'] as bool,
    date: json['date'] == null ? null : DateTime.parse(json['date'] as String),
  );
}

Map<String, dynamic> _$PlaylistToJson(Playlist instance) => <String, dynamic>{
      'id': instance.id,
      'tracks': instance.tracks,
      'overloaded': instance.overloaded,
      'date': instance.date?.toIso8601String(),
    };

PlaylistEnvelope _$PlaylistEnvelopeFromJson(Map<String, dynamic> json) {
  return PlaylistEnvelope(
    playlist: json['playlist'] == null
        ? null
        : Playlist.fromJson(json['playlist'] as Map<String, dynamic>),
  );
}

Map<String, dynamic> _$PlaylistEnvelopeToJson(PlaylistEnvelope instance) =>
    <String, dynamic>{
      'playlist': instance.playlist,
    };
