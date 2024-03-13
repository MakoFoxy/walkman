import 'package:json_annotation/json_annotation.dart';

part 'client_settings.g.dart';

@JsonSerializable()
class ClientSettings {
  List<int> advertVolume;
  List<int> musicVolume;
  bool isOnTop;
  int silentTime;

  ClientSettings({
    this.advertVolume,
    this.musicVolume,
    this.isOnTop,
    this.silentTime,
  });

  factory ClientSettings.fromJson(Map<String, dynamic> json) =>
      _$ClientSettingsFromJson(json);
  Map<String, dynamic> toJson() => _$ClientSettingsToJson(this);
}
