// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'advert_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AdvertModel _$AdvertModelFromJson(Map<String, dynamic> json) {
  return AdvertModel(
    id: json['id'] as String,
    name: json['name'] as String,
    fromDate: json['fromDate'] == null
        ? null
        : DateTime.parse(json['fromDate'] as String),
    toDate: json['toDate'] == null
        ? null
        : DateTime.parse(json['toDate'] as String),
    length: (json['length'] as num)?.toDouble(),
    objects: (json['objects'] as List)
        ?.map((e) => e == null
            ? null
            : DictionaryItem.fromJson(e as Map<String, dynamic>))
        ?.toList(),
  );
}

Map<String, dynamic> _$AdvertModelToJson(AdvertModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'fromDate': instance.fromDate?.toIso8601String(),
      'toDate': instance.toDate?.toIso8601String(),
      'length': instance.length,
      'objects': instance.objects,
    };
