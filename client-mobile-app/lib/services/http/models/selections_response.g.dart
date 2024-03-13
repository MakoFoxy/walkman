// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'selections_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SelectionsResponse _$SelectionsResponseFromJson(Map<String, dynamic> json) {
  return SelectionsResponse(
    result: (json['result'] as List)
        ?.map((e) => e == null
            ? null
            : DictionaryItem.fromJson(e as Map<String, dynamic>))
        ?.toList(),
  );
}

Map<String, dynamic> _$SelectionsResponseToJson(SelectionsResponse instance) =>
    <String, dynamic>{
      'result': instance.result,
    };
