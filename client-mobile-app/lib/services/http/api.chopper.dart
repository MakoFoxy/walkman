// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'api.dart';

// **************************************************************************
// ChopperGenerator
// **************************************************************************

// ignore_for_file: always_put_control_body_on_new_line, always_specify_types, prefer_const_declarations
class _$ObjectApi extends ObjectApi {
  _$ObjectApi([ChopperClient client]) {
    if (client == null) return;
    this.client = client;
  }

  @override
  final definitionType = ObjectApi;

  @override
  Future<Response<ObjectInfo>> getObject(String id) {
    final $url = '/api/v1/object/$id';
    final $request = Request('GET', $url, client.baseUrl);
    return client.send<ObjectInfo, ObjectInfo>($request);
  }

  @override
  Future<Response<ClientSettings>> getObjectSettings(String id) {
    final $url = '/api/v1/client/$id/settings';
    final $request = Request('GET', $url, client.baseUrl);
    return client.send<ClientSettings, ClientSettings>($request);
  }

  @override
  Future<Response<ClientVolumeResponse>> getObjectVolume(String id, int hour) {
    final $url = '/api/v1/client/$id/settings/volume/$hour';
    final $request = Request('GET', $url, client.baseUrl);
    return client.send<ClientVolumeResponse, ClientVolumeResponse>($request);
  }

  @override
  Future<Response<dynamic>> updateObjectVolume(
      String id, ClientUpdateVolumeRequest request) {
    final $url = '/api/v1/client/$id/settings/volume';
    final $body = request;
    final $request = Request('POST', $url, client.baseUrl, body: $body);
    return client.send<dynamic, dynamic>($request);
  }
}

// ignore_for_file: always_put_control_body_on_new_line, always_specify_types, prefer_const_declarations
class _$UserApi extends UserApi {
  _$UserApi([ChopperClient client]) {
    if (client == null) return;
    this.client = client;
  }

  @override
  final definitionType = UserApi;

  @override
  Future<Response<String>> auth(AuthRequest authRequest) {
    final $url = '/api/v1/auth';
    final $body = authRequest;
    final $request = Request('POST', $url, client.baseUrl, body: $body);
    return client.send<String, String>($request);
  }

  @override
  Future<Response<PermissionsResponse>> getPermissions() {
    final $url = 'api/v1/current-user/permissions';
    final $request = Request('GET', $url, client.baseUrl);
    return client.send<PermissionsResponse, PermissionsResponse>($request);
  }

  @override
  Future<Response<UserObjectsResponse>> getObjects() {
    final $url = 'api/v1/current-user/objects';
    final $request = Request('GET', $url, client.baseUrl);
    return client.send<UserObjectsResponse, UserObjectsResponse>($request);
  }
}

// ignore_for_file: always_put_control_body_on_new_line, always_specify_types, prefer_const_declarations
class _$PlaylistApi extends PlaylistApi {
  _$PlaylistApi([ChopperClient client]) {
    if (client == null) return;
    this.client = client;
  }

  @override
  final definitionType = PlaylistApi;

  @override
  Future<Response<PlaylistEnvelope>> getPlaylist(
      String objectId, DateTime date) {
    final $url = '/api/v1/playlist';
    final $params = <String, dynamic>{'objectId': objectId, 'date': date};
    final $request = Request('GET', $url, client.baseUrl, parameters: $params);
    return client.send<PlaylistEnvelope, PlaylistEnvelope>($request);
  }
}

// ignore_for_file: always_put_control_body_on_new_line, always_specify_types, prefer_const_declarations
class _$BanMusicApi extends BanMusicApi {
  _$BanMusicApi([ChopperClient client]) {
    if (client == null) return;
    this.client = client;
  }

  @override
  final definitionType = BanMusicApi;

  @override
  Future<Response<dynamic>> addMusicToBanList(String objectId, String musicId) {
    final $url = '/api/v1/object/$objectId/ban-music/$musicId';
    final $request = Request('POST', $url, client.baseUrl);
    return client.send<dynamic, dynamic>($request);
  }
}

// ignore_for_file: always_put_control_body_on_new_line, always_specify_types, prefer_const_declarations
class _$SelectionsApi extends SelectionsApi {
  _$SelectionsApi([ChopperClient client]) {
    if (client == null) return;
    this.client = client;
  }

  @override
  final definitionType = SelectionsApi;

  @override
  Future<Response<SelectionsResponse>> getSelections() {
    final $url = '/api/v1/selections';
    final $request = Request('GET', $url, client.baseUrl);
    return client.send<SelectionsResponse, SelectionsResponse>($request);
  }

  @override
  Future<Response<dynamic>> createSelection(SelectionRequest selectionRequest) {
    final $url = '/api/v1/selections';
    final $body = selectionRequest;
    final $request = Request('POST', $url, client.baseUrl, body: $body);
    return client.send<dynamic, dynamic>($request);
  }

  @override
  Future<Response<Selection>> getSelectionById(String id) {
    final $url = '/api/v1/selections/$id';
    final $request = Request('GET', $url, client.baseUrl);
    return client.send<Selection, Selection>($request);
  }
}

// ignore_for_file: always_put_control_body_on_new_line, always_specify_types, prefer_const_declarations
class _$AdvertsApi extends AdvertsApi {
  _$AdvertsApi([ChopperClient client]) {
    if (client == null) return;
    this.client = client;
  }

  @override
  final definitionType = AdvertsApi;

  @override
  Future<Response<AdvertsResponse>> getAdverts(String objectId) {
    final $url = '/api/v1/adverts?objectId={objectId}';
    final $params = <String, dynamic>{'objectId': objectId};
    final $request = Request('GET', $url, client.baseUrl, parameters: $params);
    return client.send<AdvertsResponse, AdvertsResponse>($request);
  }
}
