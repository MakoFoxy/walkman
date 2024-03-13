import 'dart:async';

import 'package:chopper/chopper.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/models/client_settings.dart';
import 'package:player_mobile_app/models/object_info.dart';
import 'package:player_mobile_app/models/playlist.dart';
import 'package:player_mobile_app/models/selection.dart';
import 'package:player_mobile_app/services/http/models/adverts_response.dart';
import 'package:player_mobile_app/services/http/models/auth_request.dart';
import 'package:player_mobile_app/services/http/models/client_update_volume_request.dart';
import 'package:player_mobile_app/services/http/models/client_volume_response.dart';
import 'package:player_mobile_app/services/http/models/permissions_response.dart';
import 'package:player_mobile_app/services/http/models/selection_request.dart';
import 'package:player_mobile_app/services/http/models/selections_response.dart';
import 'package:player_mobile_app/services/http/models/user_objects_response.dart';
import 'package:player_mobile_app/stores/user_store.dart';

part 'api.chopper.dart';

@ChopperApi()
abstract class ObjectApi extends ChopperService {
  static ObjectApi create([ChopperClient client]) {
    return _$ObjectApi(client);
  }

  @Get(path: '/api/v1/object/{id}')
  Future<Response<ObjectInfo>> getObject(@Path('id') String id);

  @Get(path: '/api/v1/client/{objectId}/settings')
  Future<Response<ClientSettings>> getObjectSettings(
      @Path('objectId') String id);

  @Get(path: '/api/v1/client/{objectId}/settings/volume/{hour}')
  Future<Response<ClientVolumeResponse>> getObjectVolume(
      @Path('objectId') String id, @Path('hour') int hour);

  @Post(path: '/api/v1/client/{objectId}/settings/volume')
  Future<Response> updateObjectVolume(
      @Path('objectId') String id, @Body() ClientUpdateVolumeRequest request);
}

@ChopperApi()
abstract class UserApi extends ChopperService {
  static UserApi create([ChopperClient client]) {
    return _$UserApi(client);
  }

  @Post(path: '/api/v1/auth')
  Future<Response<String>> auth(@Body() AuthRequest authRequest);

  @Get(path: 'api/v1/current-user/permissions')
  Future<Response<PermissionsResponse>> getPermissions();

  @Get(path: 'api/v1/current-user/objects')
  Future<Response<UserObjectsResponse>> getObjects();
}

@ChopperApi()
abstract class PlaylistApi extends ChopperService {
  static PlaylistApi create([ChopperClient client]) {
    return _$PlaylistApi(client);
  }

  @Get(path: '/api/v1/playlist')
  Future<Response<PlaylistEnvelope>> getPlaylist(
      @Query('objectId') String objectId, @Query('date') DateTime date);
}

@ChopperApi()
abstract class BanMusicApi extends ChopperService {
  static BanMusicApi create([ChopperClient client]) {
    return _$BanMusicApi(client);
  }

  @Post(
      path: '/api/v1/object/{objectId}/ban-music/{musicId}', optionalBody: true)
  Future<Response> addMusicToBanList(
      @Path('objectId') String objectId, @Path('musicId') String musicId);
}

@ChopperApi()
abstract class SelectionsApi extends ChopperService {
  static SelectionsApi create([ChopperClient client]) {
    return _$SelectionsApi(client);
  }

  @Get(path: '/api/v1/selections')
  Future<Response<SelectionsResponse>> getSelections();

  @Post(path: '/api/v1/selections')
  Future<Response> createSelection(@Body() SelectionRequest selectionRequest);

  @Get(path: '/api/v1/selections/{id}')
  Future<Response<Selection>> getSelectionById(@Path('id') String id);
}

@ChopperApi()
abstract class AdvertsApi extends ChopperService {
  static AdvertsApi create([ChopperClient client]) {
    return _$AdvertsApi(client);
  }

  @Get(path: '/api/v1/adverts?objectId={objectId}')
  Future<Response<AdvertsResponse>> getAdverts(
      @Query('objectId') String objectId);
}

class BearerRequestInterceptor implements RequestInterceptor {
  @override
  FutureOr<Request> onRequest(Request request) {
    var userStore = inject<UserStore>();

    if (userStore.token.isNotEmpty) {
      return applyHeader(request, 'Authorization', 'Bearer ${userStore.token}');
    }

    return request;
  }
}

//https://github.com/google/json_serializable.dart/issues/133#issuecomment-388259781
//https://github.com/google/json_serializable.dart/blob/bb96ddc0c241d07879d06f2209a210d2e73df31b/json_serializable/test/test_files/json_test_example.dart#L125-L151
//https://medium.com/@hasimyerlikaya/flutter-custom-datetime-serialization-with-jsonconverter-5f57f93d537
