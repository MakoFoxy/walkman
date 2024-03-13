// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'permissions_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

PermissionsResponse _$PermissionsResponseFromJson(Map<String, dynamic> json) {
  return PermissionsResponse(
    permissions:
        (json['permissions'] as List)?.map((e) => e as String)?.toList(),
  );
}

Map<String, dynamic> _$PermissionsResponseToJson(
        PermissionsResponse instance) =>
    <String, dynamic>{
      'permissions': instance.permissions,
    };
