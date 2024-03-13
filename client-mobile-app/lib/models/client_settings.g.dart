// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'client_settings.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClientSettings _$ClientSettingsFromJson(Map<String, dynamic> json) {
  return ClientSettings(
    advertVolume:
        (json['advertVolume'] as List)?.map((e) => e as int)?.toList(),
    musicVolume: (json['musicVolume'] as List)?.map((e) => e as int)?.toList(),
    isOnTop: json['isOnTop'] as bool,
    silentTime: json['silentTime'] as int,
  );
}

Map<String, dynamic> _$ClientSettingsToJson(ClientSettings instance) =>
    <String, dynamic>{
      'advertVolume': instance.advertVolume,
      'musicVolume': instance.musicVolume,
      'isOnTop': instance.isOnTop,
      'silentTime': instance.silentTime,
    };
