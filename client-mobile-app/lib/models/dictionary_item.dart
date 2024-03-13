import 'package:json_annotation/json_annotation.dart';

part 'dictionary_item.g.dart';

@JsonSerializable()
class DictionaryItem {
  String id;
  String name;

  DictionaryItem({
    this.id,
    this.name,
  });

  factory DictionaryItem.fromJson(Map<String, dynamic> json) =>
      _$DictionaryItemFromJson(json);
  Map<String, dynamic> toJson() => _$DictionaryItemToJson(this);
}
