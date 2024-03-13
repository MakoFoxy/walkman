// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'object_info.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ObjectInfo _$ObjectInfoFromJson(Map<String, dynamic> json) {
  return ObjectInfo(
    id: json['id'] as String,
    name: json['name'] as String,
    beginTime: json['beginTime'] as String,
    endTime: json['endTime'] as String,
    isOnline: json['isOnline'] as bool,
    city: json['city'] == null
        ? null
        : DictionaryItem.fromJson(json['city'] as Map<String, dynamic>),
  );
}

Map<String, dynamic> _$ObjectInfoToJson(ObjectInfo instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'beginTime': instance.beginTime,
      'endTime': instance.endTime,
      'isOnline': instance.isOnline,
      'city': instance.city,
    };
