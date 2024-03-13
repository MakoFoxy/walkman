import 'package:json_annotation/json_annotation.dart';
import 'package:player_mobile_app/models/dictionary_item.dart';

part 'user_objects_response.g.dart';

@JsonSerializable()
class UserObjectsResponse {
  List<DictionaryItem> objects;

  UserObjectsResponse({
    this.objects,
  });

  factory UserObjectsResponse.fromJson(Map<String, dynamic> json) =>
      _$UserObjectsResponseFromJson(json);

  Map<String, dynamic> toJson() => _$UserObjectsResponseToJson(this);
}
