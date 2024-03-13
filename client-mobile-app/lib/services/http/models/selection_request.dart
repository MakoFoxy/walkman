import 'package:json_annotation/json_annotation.dart';

part 'selection_request.g.dart';

@JsonSerializable()
class SelectionRequest {
  String id;
  String name;
  DateTime dateBegin;
  DateTime dateEnd;
  bool isPublic;
  List<String> tracks;

  SelectionRequest({
    this.id,
    this.name,
    this.dateBegin,
    this.dateEnd,
    this.isPublic,
    this.tracks,
  });

  factory SelectionRequest.fromJson(Map<String, dynamic> json) =>
      _$SelectionRequestFromJson(json);

  Map<String, dynamic> toJson() => _$SelectionRequestToJson(this);
}
