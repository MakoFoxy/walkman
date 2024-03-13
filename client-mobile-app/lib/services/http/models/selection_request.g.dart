// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'selection_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SelectionRequest _$SelectionRequestFromJson(Map<String, dynamic> json) {
  return SelectionRequest(
    id: json['id'] as String,
    name: json['name'] as String,
    dateBegin: json['dateBegin'] == null
        ? null
        : DateTime.parse(json['dateBegin'] as String),
    dateEnd: json['dateEnd'] == null
        ? null
        : DateTime.parse(json['dateEnd'] as String),
    isPublic: json['isPublic'] as bool,
    tracks: (json['tracks'] as List)?.map((e) => e as String)?.toList(),
  );
}

Map<String, dynamic> _$SelectionRequestToJson(SelectionRequest instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'dateBegin': instance.dateBegin?.toIso8601String(),
      'dateEnd': instance.dateEnd?.toIso8601String(),
      'isPublic': instance.isPublic,
      'tracks': instance.tracks,
    };
