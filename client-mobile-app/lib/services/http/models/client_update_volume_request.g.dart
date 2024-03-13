// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'client_update_volume_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClientUpdateVolumeRequest _$ClientUpdateVolumeRequestFromJson(
    Map<String, dynamic> json) {
  return ClientUpdateVolumeRequest(
    objectId: json['objectId'] as String,
    hour: json['hour'] as int,
    musicVolume: json['musicVolume'] as int,
    advertVolume: json['advertVolume'] as int,
  );
}

Map<String, dynamic> _$ClientUpdateVolumeRequestToJson(
        ClientUpdateVolumeRequest instance) =>
    <String, dynamic>{
      'objectId': instance.objectId,
      'hour': instance.hour,
      'musicVolume': instance.musicVolume,
      'advertVolume': instance.advertVolume,
    };
