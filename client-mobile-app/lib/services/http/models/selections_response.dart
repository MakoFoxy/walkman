import 'package:json_annotation/json_annotation.dart';
import 'package:player_mobile_app/models/dictionary_item.dart';

part 'selections_response.g.dart';

@JsonSerializable()
class SelectionsResponse {
  List<DictionaryItem> result;

  SelectionsResponse({
    this.result,
  });

  factory SelectionsResponse.fromJson(Map<String, dynamic> json) =>
      _$SelectionsResponseFromJson(json);

  Map<String, dynamic> toJson() => _$SelectionsResponseToJson(this);
}
