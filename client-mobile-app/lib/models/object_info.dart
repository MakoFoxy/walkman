import 'package:json_annotation/json_annotation.dart';
import 'package:player_mobile_app/models/dictionary_item.dart';

part 'object_info.g.dart';

@JsonSerializable()
class ObjectInfo {
  String id;
  String name;
  String beginTime;
  String endTime;
  bool isOnline;
  DictionaryItem city;

  ObjectInfo({
    this.id,
    this.name,
    this.beginTime,
    this.endTime,
    this.isOnline,
    this.city,
  });

  String shortTime(String time) {
    return time.substring(0, time.lastIndexOf(':'));
  }

  factory ObjectInfo.fromJson(Map<String, dynamic> json) =>
      _$ObjectInfoFromJson(json);
  Map<String, dynamic> toJson() => _$ObjectInfoToJson(this);
}
