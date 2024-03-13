// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'online_object_info.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

OnlineObjectInfo _$OnlineObjectInfoFromJson(Map<String, dynamic> json) {
  return OnlineObjectInfo(
    objectId: json['objectId'] as String,
    currentTrack: json['currentTrack'] as String,
    date: json['date'] == null ? null : DateTime.parse(json['date'] as String),
    secondsFromStart: json['secondsFromStart'] as num,
  );
}

Map<String, dynamic> _$OnlineObjectInfoToJson(OnlineObjectInfo instance) =>
    <String, dynamic>{
      'objectId': instance.objectId,
      'currentTrack': instance.currentTrack,
      'date': instance.date?.toIso8601String(),
      'secondsFromStart': instance.secondsFromStart,
    };
