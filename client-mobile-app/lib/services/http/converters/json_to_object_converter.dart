import 'dart:convert';

import 'package:chopper/chopper.dart';
import 'package:player_mobile_app/models/object_info.dart';
import 'package:player_mobile_app/models/playlist.dart';
import 'package:player_mobile_app/models/selection.dart';
import 'package:player_mobile_app/services/http/models/adverts_response.dart';
import 'package:player_mobile_app/services/http/models/client_volume_response.dart';
import 'package:player_mobile_app/services/http/models/permissions_response.dart';
import 'package:player_mobile_app/services/http/models/selections_response.dart';
import 'package:player_mobile_app/services/http/models/user_objects_response.dart';

class JsonToTypeConverter extends JsonConverter {
  final Map<Type, Function> typeToJsonFactoryMap;

  JsonToTypeConverter(this.typeToJsonFactoryMap);

  @override
  Request convertRequest(Request request) => super.convertRequest(request);

  @override
  Response<BodyType> convertResponse<BodyType, InnerType>(Response response) {
    if (!typeToJsonFactoryMap.containsKey(InnerType)) {
      return response;
    }

    final type = typeToJsonFactoryMap[InnerType];

    //assert(type != null, 'register type $InnerType inside getDefaultConverter');
    return response.copyWith(
      body: fromJsonData<BodyType, InnerType>(response.body, type),
    );
  }

  T fromJsonData<T, InnerType>(String jsonData, Function jsonParser) {
    assert(jsonParser != null,
        'register type $InnerType inside getDefaultConverter');

    final jsonMap = jsonDecode(jsonData);

    if (jsonMap is List) {
      return jsonMap
          .map((item) => jsonParser(item as Map<String, dynamic>) as InnerType)
          .toList() as T;
    }

    return jsonParser(jsonMap);
  }

  static JsonToTypeConverter getDefaultConverter() {
    return JsonToTypeConverter({
      PlaylistEnvelope: (json) => PlaylistEnvelope.fromJson(json),
      PermissionsResponse: (json) => PermissionsResponse.fromJson(json),
      UserObjectsResponse: (json) => UserObjectsResponse.fromJson(json),
      ObjectInfo: (json) => ObjectInfo.fromJson(json),
      SelectionsResponse: (json) => SelectionsResponse.fromJson(json),
      Selection: (json) => Selection.fromJson(json),
      ClientVolumeResponse: (json) => ClientVolumeResponse.fromJson(json),
      AdvertsResponse: (json) => AdvertsResponse.fromJson(json),
    });
  }
}
