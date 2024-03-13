import 'dart:async';

import 'package:mobx/mobx.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/models/track.dart';
import 'package:player_mobile_app/services/http/api.dart';
import 'package:player_mobile_app/services/http/models/auth_request.dart';
import 'package:player_mobile_app/services/store/user_settings_repository.dart';
import 'package:player_mobile_app/stores/object_store.dart';

part 'user_store.g.dart';

class UserStore = _UserStore with _$UserStore;

abstract class _UserStore with Store {
  final UserApi _userApi = inject();
  final BanMusicApi _banMusicApi = inject();
  final ObjectStore _objectStore = inject();
  final UserSettingsRepository _userSettingsRepository = inject();

  String _managerPermission = 'PartnerAccessToObject';

  @observable
  String token = '';

  @observable
  String userEmail = '';

  @observable
  List<String> permissions = List<String>();

  @computed
  bool get isManager => permissions.contains(_managerPermission);

  @action
  void setUserAuthData(String email, String token) {
    this.token = token;
    this.userEmail = email;
  }

  @action
  Future<bool> login(String email, String password) async {
    final response =
        await _userApi.auth(AuthRequest(email: email, password: password));

    if (response.isSuccessful) {
      setUserAuthData(email, response.body);
      _userSettingsRepository.saveUserToken(token);
      _userSettingsRepository.saveUserEmail(email);
      return true;
    }

    return false;
  }

  @action
  Future<bool> getPermissions() async {
    final response = await _userApi.getPermissions();

    if (response.isSuccessful) {
      permissions = response.body.permissions;
      return true;
    }
    return false;
  }

  @action
  Future<bool> banMusic(Track musicTrack) async {
    final response = await _banMusicApi.addMusicToBanList(
        _objectStore.selectedObject.id, musicTrack.id);

    return response.isSuccessful;
  }
}
