// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'user_objects_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UserObjectsResponse _$UserObjectsResponseFromJson(Map<String, dynamic> json) {
  return UserObjectsResponse(
    objects: (json['objects'] as List)
        ?.map((e) => e == null
            ? null
            : DictionaryItem.fromJson(e as Map<String, dynamic>))
        ?.toList(),
  );
}

Map<String, dynamic> _$UserObjectsResponseToJson(
        UserObjectsResponse instance) =>
    <String, dynamic>{
      'objects': instance.objects,
    };
