// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'adverts_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AdvertsResponse _$AdvertsResponseFromJson(Map<String, dynamic> json) {
  return AdvertsResponse(
    result: (json['result'] as List)
        ?.map((e) =>
            e == null ? null : AdvertModel.fromJson(e as Map<String, dynamic>))
        ?.toList(),
  );
}

Map<String, dynamic> _$AdvertsResponseToJson(AdvertsResponse instance) =>
    <String, dynamic>{
      'result': instance.result,
    };
