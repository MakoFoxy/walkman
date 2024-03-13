// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'client_volume_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClientVolumeResponse _$ClientVolumeResponseFromJson(Map<String, dynamic> json) {
  return ClientVolumeResponse(
    musicVolume: json['musicVolume'] as int,
    advertVolume: json['advertVolume'] as int,
  );
}

Map<String, dynamic> _$ClientVolumeResponseToJson(
        ClientVolumeResponse instance) =>
    <String, dynamic>{
      'musicVolume': instance.musicVolume,
      'advertVolume': instance.advertVolume,
    };
