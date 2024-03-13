// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'selection.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Selection _$SelectionFromJson(Map<String, dynamic> json) {
  return Selection(
    id: json['id'] as String,
    name: json['name'] as String,
    isPublic: json['isPublic'] as bool,
    tracks: (json['tracks'] as List)
        ?.map((e) => e == null
            ? null
            : TrackInSelection.fromJson(e as Map<String, dynamic>))
        ?.toList(),
  );
}

Map<String, dynamic> _$SelectionToJson(Selection instance) => <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'isPublic': instance.isPublic,
      'tracks': instance.tracks,
    };

TrackInSelection _$TrackInSelectionFromJson(Map<String, dynamic> json) {
  return TrackInSelection(
    id: json['id'] as String,
    index: json['index'] as int,
    name: json['name'] as String,
    length: (json['length'] as num)?.toDouble(),
    selected: json['selected'] as bool,
  );
}

Map<String, dynamic> _$TrackInSelectionToJson(TrackInSelection instance) =>
    <String, dynamic>{
      'id': instance.id,
      'index': instance.index,
      'name': instance.name,
      'length': instance.length,
      'selected': instance.selected,
    };
