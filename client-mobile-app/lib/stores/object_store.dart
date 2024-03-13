import 'dart:async';

import 'package:mobx/mobx.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/models/advert_model.dart';
import 'package:player_mobile_app/models/dictionary_item.dart';
import 'package:player_mobile_app/models/object_info.dart';
import 'package:player_mobile_app/services/http/api.dart';
import 'package:player_mobile_app/services/http/models/client_update_volume_request.dart';
import 'package:player_mobile_app/services/online_connector.dart';
import 'package:player_mobile_app/services/store/user_settings_repository.dart';

part 'object_store.g.dart';

class ObjectStore = _ObjectStore with _$ObjectStore;

abstract class _ObjectStore with Store {
  final UserApi _userApi = inject();
  final ObjectApi _objectApi = inject();
  final AdvertsApi _advertsApi = inject();
  final UserSettingsRepository _userSettingsRepository = inject();

  @observable
  var selectedObject = ObjectInfo();

  @observable
  List<DictionaryItem> objects = List<DictionaryItem>();

  @observable
  List<AdvertModel> advertsOnObject = List<AdvertModel>();

  @observable
  int musicVolume;

  @observable
  int advertVolume;

  @action
  Future<bool> setUserObject() async {
    final userObjectsResponse = await _userApi.getObjects();

    if (userObjectsResponse.isSuccessful) {
      objects = userObjectsResponse.body.objects;

      String userObjectId = _userSettingsRepository.getLastSelectedObject();
      if (!objects.any((o) => o.id == userObjectId)) {
        userObjectId = userObjectsResponse.body.objects.first.id;
        await _userSettingsRepository.saveLastSelectedObject(userObjectId);
      }

      final objectInfoSuccess = await getObjectInfo(userObjectId);
      final advertsOnObjectSuccess = await getAdvertsOnObject(userObjectId);

      return objectInfoSuccess && advertsOnObjectSuccess;
    }

    return false;
  }

  @action
  Future<bool> getObjectInfo(String objectId) async {
    final objectResponse = await _objectApi.getObject(objectId);

    if (objectResponse.isSuccessful) {
      selectedObject = objectResponse.body;

      final volumeResponse =
          await _objectApi.getObjectVolume(objectId, DateTime.now().hour);

      if (volumeResponse.isSuccessful) {
        musicVolume = volumeResponse.body.musicVolume;
        advertVolume = volumeResponse.body.advertVolume;
        return true;
      }
    }

    return false;
  }

  @action
  Future<bool> getAdvertsOnObject(String objectId) async {
    final advertsResponse = await _advertsApi.getAdverts(objectId);

    if (advertsResponse.isSuccessful) {
      this.advertsOnObject = advertsResponse.body.result;
      return true;
    }

    return false;
  }

  @action
  Future<bool> updateObjectVolume() async {
    final request = ClientUpdateVolumeRequest();
    request.objectId = selectedObject.id;
    request.musicVolume = musicVolume;
    request.advertVolume = advertVolume;
    request.hour = DateTime.now().hour;

    final response =
        await _objectApi.updateObjectVolume(selectedObject.id, request);

    return response.isSuccessful;
  }
}
