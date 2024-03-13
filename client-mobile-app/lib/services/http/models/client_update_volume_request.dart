import 'package:json_annotation/json_annotation.dart';

part 'client_update_volume_request.g.dart';

@JsonSerializable()
class ClientUpdateVolumeRequest {
  String objectId;
  int hour;
  int musicVolume;
  int advertVolume;

  ClientUpdateVolumeRequest({
    this.objectId,
    this.hour,
    this.musicVolume,
    this.advertVolume,
  });

  factory ClientUpdateVolumeRequest.fromJson(Map<String, dynamic> json) =>
      _$ClientUpdateVolumeRequestFromJson(json);

  Map<String, dynamic> toJson() => _$ClientUpdateVolumeRequestToJson(this);
}
