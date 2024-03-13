import 'package:json_annotation/json_annotation.dart';

part 'client_volume_response.g.dart';

@JsonSerializable()
class ClientVolumeResponse {
  int musicVolume;
  int advertVolume;

  ClientVolumeResponse({
    this.musicVolume,
    this.advertVolume,
  });

  factory ClientVolumeResponse.fromJson(Map<String, dynamic> json) =>
      _$ClientVolumeResponseFromJson(json);

  Map<String, dynamic> toJson() => _$ClientVolumeResponseToJson(this);
}
