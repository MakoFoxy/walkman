import 'package:json_annotation/json_annotation.dart';

part 'track.g.dart';

@JsonSerializable()
class Track {
  String id;
  int index;
  int repeatNumber;
  String type;
  String name;
  DateTime playingDateTime;
  double length;
  String uniqueId;

  Track({
    this.id,
    this.index,
    this.repeatNumber,
    this.type,
    this.name,
    this.playingDateTime,
    this.length,
    this.uniqueId,
  });

  factory Track.fromJson(Map<String, dynamic> json) => _$TrackFromJson(json);
  Map<String, dynamic> toJson() => _$TrackToJson(this);

  String nameWithoutExtension() {
    var extensionStartIndex = name.lastIndexOf('.');
    return name
        .substring(0, extensionStartIndex)
        .replaceAll(RegExp('_|-|\\+'), ' ');
  }
}
