import 'package:json_annotation/json_annotation.dart';
import 'package:player_mobile_app/models/advert_model.dart';

part 'adverts_response.g.dart';

@JsonSerializable()
class AdvertsResponse {
  List<AdvertModel> result;

  AdvertsResponse({
    this.result,
  });

  factory AdvertsResponse.fromJson(Map<String, dynamic> json) =>
      _$AdvertsResponseFromJson(json);

  Map<String, dynamic> toJson() => _$AdvertsResponseToJson(this);
}
