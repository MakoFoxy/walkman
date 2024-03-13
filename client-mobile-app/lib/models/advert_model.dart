import 'package:json_annotation/json_annotation.dart';
import 'package:player_mobile_app/models/dictionary_item.dart';

part 'advert_model.g.dart';

@JsonSerializable()
class AdvertModel {
  String id;
  String name;
  DateTime fromDate;
  DateTime toDate;
  double length;
  List<DictionaryItem> objects;

  AdvertModel({
    this.id,
    this.name,
    this.fromDate,
    this.toDate,
    this.length,
    this.objects,
  });

  factory AdvertModel.fromJson(Map<String, dynamic> json) =>
      _$AdvertModelFromJson(json);

  Map<String, dynamic> toJson() => _$AdvertModelToJson(this);
}
