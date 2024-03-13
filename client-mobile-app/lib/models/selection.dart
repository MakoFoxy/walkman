import 'package:json_annotation/json_annotation.dart';

part 'selection.g.dart';

@JsonSerializable()
class Selection {
  String id;
  String name;
  bool isPublic;
  List<TrackInSelection> tracks;

  Selection({
    this.id,
    this.name,
    this.isPublic,
    this.tracks,
  });

  factory Selection.fromJson(Map<String, dynamic> json) =>
      _$SelectionFromJson(json);
  Map<String, dynamic> toJson() => _$SelectionToJson(this);
}

@JsonSerializable()
class TrackInSelection {
  String id;
  int index;
  String name;
  double length;
  bool selected;

  TrackInSelection({
    this.id,
    this.index,
    this.name,
    this.length,
    this.selected,
  });

  factory TrackInSelection.fromJson(Map<String, dynamic> json) =>
      _$TrackInSelectionFromJson(json);
  Map<String, dynamic> toJson() => _$TrackInSelectionToJson(this);
}
