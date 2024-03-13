import 'package:json_annotation/json_annotation.dart';

part 'permissions_response.g.dart';

@JsonSerializable()
class PermissionsResponse {
  List<String> permissions;

  PermissionsResponse({
    this.permissions,
  });

  factory PermissionsResponse.fromJson(Map<String, dynamic> json) =>
      _$PermissionsResponseFromJson(json);

  Map<String, dynamic> toJson() => _$PermissionsResponseToJson(this);
}
