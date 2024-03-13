import 'package:json_annotation/json_annotation.dart';

part 'online_object_info.g.dart';

@JsonSerializable()
class OnlineObjectInfo {
  String objectId;
  String currentTrack;
  DateTime date;
  num secondsFromStart;

  OnlineObjectInfo({
    this.objectId,
    this.currentTrack,
    this.date,
    this.secondsFromStart,
  });

  factory OnlineObjectInfo.fromJson(Map<String, dynamic> json) =>
      _$OnlineObjectInfoFromJson(json);

  Map<String, dynamic> toJson() => _$OnlineObjectInfoToJson(this);
}
