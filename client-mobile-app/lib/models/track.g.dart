// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'track.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Track _$TrackFromJson(Map<String, dynamic> json) {
  return Track(
    id: json['id'] as String,
    index: json['index'] as int,
    repeatNumber: json['repeatNumber'] as int,
    type: json['type'] as String,
    name: json['name'] as String,
    playingDateTime: json['playingDateTime'] == null
        ? null
        : DateTime.parse(json['playingDateTime'] as String),
    length: (json['length'] as num)?.toDouble(),
    uniqueId: json['uniqueId'] as String,
  );
}

Map<String, dynamic> _$TrackToJson(Track instance) => <String, dynamic>{
      'id': instance.id,
      'index': instance.index,
      'repeatNumber': instance.repeatNumber,
      'type': instance.type,
      'name': instance.name,
      'playingDateTime': instance.playingDateTime?.toIso8601String(),
      'length': instance.length,
      'uniqueId': instance.uniqueId,
    };
